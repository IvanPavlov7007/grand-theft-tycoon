using UnityEngine;

[CreateAssetMenu(fileName = "WarehouseProductViewBuilder", menuName = "Configs/View/WarehouseProductViewBuilder")]
public class MonolithProductGameObjectBuilder : WarehouseProductGameObjectBuilder
{
    public override GameObject BuildCar(Car car)
    {
        GameObject carGO = new GameObject(car.Name);
        carGO.AddComponent<Rigidbody2D>();

        var frameGO = car.CarFrame.GetRepresentation(carPartViewBuilder);
        frameGO.transform.SetParent(carGO.transform);
        frameGO.transform.localPosition = Vector3.zero;

        foreach (var partLocation in car.carParts.Keys)
        {
            CarPartViewPlacementHelper.BuildCarPartAtPosition(partLocation, carGO.transform, carPartViewBuilder);
        }

        productViewComponentBuilder.BuildViewComponent(carGO, car);

        return carGO;
    }

    public override GameObject BuildCarFrame(CarFrame carFrame)
    {
        Debug.LogError("WarehouseProductViewBuilder does not support building CarFrame views. " + carFrame.UniqueName);
        return null;
    }
}