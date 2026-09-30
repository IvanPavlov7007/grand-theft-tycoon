using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PersonalVehicleShopConfig", menuName = "Configs/Player/PersonalVehicleShopConfig")]
public class VehicleShopConfig : CityLocatedConfig
{
    public List<string> allCarOptions;
    public List<string> initiallyUnlockedOptions;
}