using System;
using UnityEngine;

[Obsolete]
public sealed class StealingCityContextMenuProfile : ICityContextMenuProfile
{
    public Widget GenerateContent(CityEntity model, GameState gameState)
    {
        return null;
    }

    private UIElement generateStealingCarContent(StealingGameState stealingGameState, Car car)
    {
        var elementsList = CTX_Menu_Tools.CarBaseInfoElements(car);
        if (stealingGameState.StealingCar == car)
        {
            elementsList.Add(CTX_Menu_Tools.Hint("This is the car you are currently stealing. Find a warehouse to store it in."));
            elementsList.Add(CTX_Menu_Tools.CancelButton());
        }
        else
        {
            elementsList.Add(CTX_Menu_Tools.Hint("You are already stealing another car. Find a warehouse to store it in before stealing another."));
        }

        return new UIElement
        {
            Type = UIElementType.Container,
            Children = elementsList
        };
    }

    private UIElement generateStealingWarehouseContent(StealingGameState stealingGameState, Warehouse warehouse)
    {
        var warehouseOffer = G.Economy.WarehouseOfferProvider.GetOfferForWarehouse(warehouse);

        var elementsList = CTX_Menu_Tools.WarehouseBaseInfoElements(warehouse);
        if (warehouseOffer != null)
        {
            elementsList.AddRange(CTX_Menu_Tools.WarehousePurchaseElements(warehouseOffer));
        }
        else
        {
            elementsList.Add(
                CTX_Menu_Tools.Hint(
                    warehouse.AvailableCarParkingSpots > 0
                        ? "Ride into this warehouse to complete your car-theft"
                        : "Not enough space to store an additional auto"));
        }

        return new UIElement
        {
            Type = UIElementType.Container,
            Children = elementsList
        };
    }
}

[Obsolete]
public sealed class StealingCityTriggerProfile : ICityTriggerProfile
{
    public TriggerAction GenerateTriggerAction(TriggerContext ctx)
    {
        var stealingState = ctx.GameState as StealingGameState;
        if (stealingState == null)
        {
            Debug.LogError($"StealingCityTriggerProfile used with non-stealing state {ctx.GameState?.GetType().Name}");
            return new TriggerAction(false, null);
        }

        var warehouse = ctx.Trigger.Subject as Warehouse;
        Debug.Assert(warehouse != null, "StealingCityTriggerProfile: trigger is not Warehouse");
        Debug.Assert(stealingState.StealingCar != null, "StealingCityTriggerProfile: StealingCar is null");

        bool canProceed =
            G.Player.Owns(warehouse) &&
            warehouse.AvailableCarParkingSpots > 0 &&
            ctx.TriggerCause.Subject as Car == stealingState.StealingCar;

        System.Action action = null;
        if (canProceed)
        {
            action = () =>
            {
                GameEvents.Instance.OnPlayerSucceed(
                    new PlayerActionEventData(PlayerOutcome.Succeeded, warehouse));
            };
        }

        return new TriggerAction(canProceed, action);
    }
}