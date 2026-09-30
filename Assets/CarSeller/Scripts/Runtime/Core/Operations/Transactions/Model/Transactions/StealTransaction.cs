public class StealTransaction : Transaction
{
    public Car Car { get; private set; }
    public Warehouse TargetWarehouse { get; set; }
    public StealTransaction(Car car, Warehouse targetWarehouse)
    {
        Car = car;
        TargetWarehouse = targetWarehouse;
    }
}