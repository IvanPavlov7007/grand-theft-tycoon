using System.Diagnostics;

public class PutCarInWarehouseOffer : AcceptOnceOffer
{
    public readonly Car car;
    public readonly Warehouse warehouse;

    public PutCarInWarehouseOffer(Car car, Warehouse warehouse)
    {
        this.car = car;
        this.warehouse = warehouse;
    }

    public override Transaction Accept()
    {
        Debug.Assert(CanAccept());
        IsAccepted = true;
        return new PutProductsInHolderTransaction(
                new WarehouseHolderAdapter(warehouse),car);
    }

    public override bool CanAccept()
    {
        return base.CanAccept() && GameRules.WarehouseCanStoreCar.Check(car, warehouse);
    }
}

public class WarehousePullCarOffer : AcceptOnceOffer
{
    Car car;
    Warehouse warehouse;

    public override Transaction Accept()
    {
        Debug.Assert(CanAccept());
        IsAccepted = true;
        return new PullCarFromWarehouseTransaction(car, warehouse);
    }

    public override bool CanAccept()
    {
        return base.CanAccept() && GameRules.CanRideOutCar.Check(car, warehouse);
    }
}

public class WarehouseStripCarOffer : AcceptOnceOffer
{
    Car car;
    Warehouse warehouse;
    IStrippingPolicy strippingPolicy;

    public WarehouseStripCarOffer(Car car, Warehouse warehouse, IStrippingPolicy strippingPolicy)
    {
        this.car = car;
        this.warehouse = warehouse;
        this.strippingPolicy = strippingPolicy;
    }

    public override Transaction Accept()
    {
        Debug.Assert(CanAccept());
        IsAccepted = true;
        StrippingProcess strippingProcess = G.CarStripper.Strip(car, strippingPolicy);
        return new StripCarTransaction(
            car, new HiddenSpaceHolderAdapter(),
            strippingProcess);
    }

    public override bool CanAccept()
    {
        return base.CanAccept() && GameRules.WarehouseCanStripCar.Check(car, warehouse);
    }
}