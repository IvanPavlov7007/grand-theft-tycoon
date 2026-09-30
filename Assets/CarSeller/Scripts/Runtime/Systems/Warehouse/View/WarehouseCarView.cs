using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Car view in the warehouse on a change, view partially updates itself based on what changed
/// </summary>
public class WarehouseCarView : WarehouseProductView
{
    public List<CarPartView> parts;
    public Car car { get; private set; }

    // Which builder created this car view (monolith, physical, etc.)
    private WarehouseProductGameObjectBuilder builder;

    protected override void OnEnable()
    {
        base.OnEnable();
        GameEvents.Instance.OnProductLocationChanged += onAnyProductLocationChanged;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        GameEvents.Instance.OnProductLocationChanged -= onAnyProductLocationChanged;
    }

    public override void Initialize(Product product, ILocation representedProductLocation)
    {
        Debug.Assert(product is Car, "CarView can represent only a Car");
        base.Initialize(product, representedProductLocation);
        car = (Car)product;

        // Default to global builder if none is set explicitly
        if (builder == null)
        {
            builder = G.warehouseProductViewBuilder;
        }
    }

    /// <summary>
    /// Allow the creator (builder) to inject itself so we know
    /// whether this car is physical or monolithic, etc.
    /// </summary>
    public void SetBuilder(WarehouseProductGameObjectBuilder productBuilder)
    {
        builder = productBuilder;
    }

    void onAnyProductLocationChanged(ProductLocationChangedEventData data)
    {
        if (data.OldLocation?.Holder == car)
        {
            // Handle removal from old location if needed
        }

        if (data.NewLocation?.Holder == car)
        {
            var carPartLocation = data.NewLocation as Car.CarPartLocation;
            if (carPartLocation == null) return;

            // Delegate to the builder-specific logic
            HandlePartAttached(carPartLocation);
        }
    }

    private void HandlePartAttached(Car.CarPartLocation partLocation)
    {
        // If we somehow don't have a builder, fall back to old behavior
        if (builder == null)
        {
            CarPartViewPlacementHelper.BuildCarPartAtPosition(
                partLocation,
                transform,
                G.carPartViewBuilder);
            return;
        }

        // PHYSICAL BUILDER: add physics/joints, etc.
        if (builder is PhysicalProductGameObjectBuilder physicalBuilder)
        {
            physicalBuilder.AttachPartToCarView(this, partLocation);
        }
        else
        {
            // MONOLITH (or other) BUILDER: plain visual placement
            CarPartViewPlacementHelper.BuildCarPartAtPosition(
                partLocation,
                transform,
                builder.carPartViewBuilder);
        }
    }
}