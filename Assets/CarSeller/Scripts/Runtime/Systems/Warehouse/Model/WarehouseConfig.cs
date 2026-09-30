using Sirenix.OdinInspector;
using System;
using UnityEngine;

[CreateAssetMenu(fileName = "WarehouseConfig", menuName = "Configs/Economy/WarehouseConfig")]
public class WarehouseConfig : CityLocatedConfig
{
    [InfoBox("Used as WarehouseIdentifier and the same as the scene name")]
    public string Name;

    public Sprite image;
    public string SizeCategory = "Small garage";
    public int CarParkingSpots = 5;

    [HideInInspector]
    public Vector2 warehouseClosestInitialPosition;

    public SimpleProductSpawnConfig[] initialProductsToSpawn;
    public SimpleCarSpawnConfig[] initialCarsToSpawn;
}

public class CityLocatedConfig : SerializedScriptableObject 
{
    [Title("Placement")]
    [InlineProperty, HideLabel]
    public CityMarkerRef Marker;
}

[Serializable]
public class SimpleProductSpawnConfig
{
    public ScriptableObject productBaseConfig;
    public ScriptableObject productVariantConfig;
}