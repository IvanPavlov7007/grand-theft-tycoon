using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="VehicleControllerConfig", menuName = "Configs/Player/VehicleControllerConfig")]
public class VehicleControllerConfig : ScriptableObject
{
    public Vector2 initialPosition = Vector2.zero;

    public int initialPrimeVehicleIndex = 0;

    public VehicleShopConfig vehicleShopConfig;

    public int maxOwnedCars = 10;
    public List<SimpleCarSpawnConfig> ownedCars;
    public CityPosition GetInitialCityPosition()
    {
        return G.City.GetClosestPosition(initialPosition);
    }
}