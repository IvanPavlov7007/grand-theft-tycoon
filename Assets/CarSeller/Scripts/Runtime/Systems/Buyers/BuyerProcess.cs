using System.Collections;
using UnityEngine;

public class BuyerProcess : IProcess
{
    public readonly CityEntity BuyerEntity;

    public readonly Buyer Buyer;
    public readonly Car Car;
    public BuyerProcess(Buyer buyer, Car car)
    {
        this.Buyer = buyer;
        this.Car = car;
        BuyerEntity = CityLocatorHelper.GetCityEntity(buyer);
    }

    public IEnumerator Run()
    {
        var requirementState = GameRules.CarCanBeSoldToBuyer.Check(Car, Buyer);

        switch (requirementState)
        {
            case CanCantBeSoldReason.None:
                {
                    var result = G.TransactionProcessor.Process(new SellTransaction(Car, Buyer, G.CurrentSellPriceWrapped),
                        new TransactionFeedbackLocation(TransactionLocationType.WorldSpace, BuyerEntity.Position.WorldPosition));
                    if (result.Type != TransactionResultType.Success)
                    {
                        Debug.LogError($"Failed to sell car to buyer {Buyer}. Reason: {result.Data}");
                    }
                    G.VehicleController.ExitWorldVehicle();
                    break;
                }
            case CanCantBeSoldReason.CarBelongsToPlayer:
                G.FixedContextMenuManager.CreateContextMenu(new GenericInfoWidget(Buyer.Name, "You can't sell your own vehicle."));
                break;
            case CanCantBeSoldReason.CarNotOfRequiredType:
                G.FixedContextMenuManager.CreateContextMenu(new GenericInfoWidget(Buyer.Name, "This buyer is not interested in this type of vehicle."));
                break;
        }
        yield break;
    }
}