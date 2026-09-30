public class CityActionService
{
    public bool PutCarOutsideWarehouse(Car car, Warehouse warehouse)
    {
        return CityEntitiesCreationHelper.MoveInExistingCar(
            car,
            CityLocatorHelper.GetCityEntity(warehouse).Position
            ) != null;
    }
}