using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CarStashWarehouseConfig", menuName = "Configs/CarStashWarehouseConfig")]
public class CarStashWarehouseConfig : WarehouseConfig
{
    public List<SimpleCarSpawnConfig> initiallyFilledCars;
    public int maxCount;
}