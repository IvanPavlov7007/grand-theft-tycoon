/// <summary>
/// Tracking location of product in warehouse
/// </summary>
public class WarehouseProductView : ProductView
{
    protected Warehouse.WarehouseProductLocation warehouseLocation;

    public override void Initialize(Product product, ILocation representedProductLocation)
    {
        base.Initialize(product, representedProductLocation);
        warehouseLocation = representedProductLocation as Warehouse.WarehouseProductLocation;
    }

    private void FixedUpdate()
    {
        Warehouse.DimensionalPositionData positionData = new Warehouse.DimensionalPositionData 
        { LocalPosition = transform.localPosition, LocalRotation = transform.localEulerAngles };
        warehouseLocation.Position = positionData;
    }
}