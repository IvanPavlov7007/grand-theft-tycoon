public class CarIntoWarehousePolicy : ICarWarehousePolicy
{
    public readonly IStrippingPolicy productStrippingPolicy = 
        new EnsureAtLeastOneStrippedPolicy(ProbabilityBasedStrippingPolicy.Create01(1f));

    public IOffer Resolve(Car car, Warehouse warehouse, OperationContext ctx)
    {
        if (GameRules.CarBelongsToPlayer.Check(car))
        {
            if(GameRules.WarehouseCanStoreCar.Check(car, warehouse))
                return new PutCarInWarehouseOffer(car, warehouse);
        }
        else if (GameRules.WarehouseCanStripCar.Check(car,warehouse))
        {
            return new WarehouseStripCarOffer(car, warehouse, productStrippingPolicy);
        }
        return null;
    }
}

public interface ICarWarehousePolicy
{
    IOffer Resolve(Car car, Warehouse warehouse, OperationContext ctx);
}

public class OperationContext
{

}