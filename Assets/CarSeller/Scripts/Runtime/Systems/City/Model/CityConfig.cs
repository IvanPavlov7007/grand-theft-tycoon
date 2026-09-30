using UnityEngine;

[CreateAssetMenu(fileName = "CityConfig", menuName = "Configs/CityConfig")]
public class CityConfig : ScriptableObject
{
    public string cityName;
    public string SceneToLoad;

    public Vector2 cityLeftUpPos;
    public int gridNodesCountX;
    public int gridNodesCountY;
    public float gridNodeWorldSize;
    public CityGraphAsset CityGraph;
    public SpatialGridConfig SpatialGridConfig;

    //TODO make this more generic, e.g. List<ILocatableEntitySpawnConfig> and then have separate spawn configs for warehouses, car stashes, buyers, etc.
    public WarehouseConfig[] warehouseConfigs;
    public CarStashWarehouseConfig[] carStashWarehouseConfigs;
    public SimpleProductSpawnConfig[] initialProductsToSpawn;
}