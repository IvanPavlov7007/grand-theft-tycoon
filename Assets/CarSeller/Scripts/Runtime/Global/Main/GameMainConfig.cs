using System;
using UnityEngine;

[CreateAssetMenu(fileName = "GameMainConfig", menuName = "Configs/GameMain Config")]
public sealed class GameMainConfig : SingletonScriptableObject<GameMainConfig>
{
    public GameConfig GameConfig;
    public ViewBuildersConfig ViewBuilders;
}

/// <summary>
/// Heap of various view builders used by presenters to create views
/// </summary>
[Serializable]
public struct ViewBuildersConfig
{
    [Header("Warehouse")]
    public MonolithProductGameObjectBuilder monolithWarehouseProductViewBuilder;
    public PhysicalProductGameObjectBuilder physicalWarehouseProductViewBuilder;
    public CarPartGameObjectBuilder carPartViewBuilder;

    [Header("City")]
    public CityViewObjectBuilder cityViewObjectBuilder;

    [Header("Simplified")]
    public CarsDefinitionLibrary simplifiedCarsCreationBuilder;
}