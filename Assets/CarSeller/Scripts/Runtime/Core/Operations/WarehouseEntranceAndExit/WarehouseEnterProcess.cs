using System.Collections;
using System.Linq;
using UnityEngine;

public class WarehouseEnterProcess : IProcess
{
    public readonly Car Car;
    public readonly PlayerFigure PlayerFigure;
    public readonly Warehouse Warehouse;

    public WarehouseEnterProcess(Car car, Warehouse warehouse)
    {
        Car = car;
        Warehouse = warehouse;
    }

    public WarehouseEnterProcess(PlayerFigure playerFigure, Warehouse warehouse)
    {
        PlayerFigure = playerFigure;
        Warehouse = warehouse;
    }

    public IEnumerator Run()
    {
        if (PlayerFigure != null)
        {
            // Figure entrance: just enter WH and remove figure.
            yield return enterWHOnFoot();
            yield break;
        }

        var offer = G.CarWarehousePolicy.Resolve(Car, Warehouse, new OperationContext());
        switch (offer)
        {
            case PutCarInWarehouseOffer carOffer:
                yield return puttingWholeCar(carOffer);
                break;
            case WarehouseStripCarOffer stripOffer:
                yield return strippingCar(stripOffer);
                break;
            default:
                Debug.LogWarning("No appropriate offer created by warehouse policy");
                break;
        }
        yield break;
    }

    IEnumerator enterWHOnFoot()
    {
        // asking for confirmation to enter on foot
        bool done = false; bool accepted = false;
        var element = CTX_Menu_Tools.EnterWarehouseOnFoot(
        Warehouse,
        onAccept: () => { accepted = true; done = true; },
        onCancel: () => { done = true; });

        var menu = G.FixedContextMenuManager.CreateContextMenu(element);

        // If the menu closes for any other reason (e.g., blocker close), also proceed.
        menu.Closed += _ => done = true;
        yield return new WaitUntil(() => done);
        if (!accepted)
            yield break;
        // confirmed -> enter warehouse on foot
        City.EntityLifetimeService.Destroy(PlayerFigure);
        G.GameFlowController.EnterWarehouse(Warehouse);

    }

    IEnumerator puttingWholeCar(PutCarInWarehouseOffer carOffer)
    {
        bool done = false; bool accepted = false;
        var element = CTX_Menu_Tools.PutCarInsideWarehoseConfirmation(
        carOffer,
        onAccept: () => { accepted = true; done = true; },
        onCancel: () => { done = true; });

        var menu = G.FixedContextMenuManager.CreateContextMenu(element);

        // If the menu closes for any other reason (e.g., blocker close), also proceed.
        menu.Closed += _ => done = true;

        yield return new WaitUntil(() => done);

        if (!accepted)
            yield break;

        // confirmed -> put car inside
        var transaction = carOffer.Accept();
        var result = G.TransactionProcessor.Process(transaction);
        if (result.Type != TransactionResultType.Success)
        {
            Debug.LogError("Failed to put car inside warehouse: " + result.Type);
        }
        G.GameFlowController.EnterWarehouse(Warehouse);
    }

    IEnumerator strippingCar(WarehouseStripCarOffer stripOffer)
    {
        //bring up confirmation window
        bool done = false; bool accepted = false;
        var element = CTX_Menu_Tools.StripCarInsideWarehoseConfirmation(
        stripOffer,
        onAccept: () => { accepted = true; done = true; },
        onCancel: () => { done = true; });

        var menu = G.FixedContextMenuManager.CreateContextMenu(element);
        menu.Closed += _ => done = true;

        yield return new WaitUntil(() => done);
        if (!accepted)
            yield break;
        //if confirmed open Warehouse Scene
        G.GameFlowController.EnterWarehouse(Warehouse);

        yield return new WaitUntil(() => G.GameFlowController.CurrentSceneType == GameFlowController.GameSceneType.Warehouse);
        yield return new WaitForSeconds(0.2f);

        var stripTransaction = stripOffer.Accept();

        var stripResult = G.TransactionProcessor.Process(stripTransaction);
        if (stripResult.Type != TransactionResultType.Success)
        {
            Debug.LogError("Failed to strip car inside warehouse: " + stripResult.Type);
            yield break;
        }

        //open results window with claim button
        var stripData = stripTransaction as StripCarTransaction;
        element = CTX_Menu_Tools.StipReslutsClaim(stripData.StrippingProcess.StrippedParts);
        menu = G.FixedContextMenuManager.CreateContextMenu(element);
        done = false;
        menu.Closed += _ => done = true;
        yield return new WaitUntil(() => done);
        //on claim put things inside

        var putTransaction = new PutProductsInHolderTransaction(new WarehouseHolderAdapter(Warehouse),
                stripData.StrippingProcess.StrippedParts.ToArray()
            );
        var putResult = G.TransactionProcessor.Process(putTransaction);

        if (putResult.Data is ProductsPlacingResultData placingData)
        {
            Debug.Log("Products placed inside warehouse: " + placingData.PuttingResult.skippedProducts.Count);
            if (placingData.PuttingResult.CarsSkipped)
            {
                G.GlobalHintManager.ShowHint("Couldn't store care base: no place available");
            }
        }
    }
}