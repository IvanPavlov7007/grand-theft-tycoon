using Sirenix.OdinInspector;
using UnityEngine;

public class DebugProductSpawner : MonoBehaviour
{
    public CarBaseConfig carBaseConfig;
    public CarVariantConfig carVariantConfig;

    public WheelBaseConfig wheelBaseConfig;
    public WheelVariantConfig wheelVariantConfig;

    public EngineBaseConfig engineBaseConfig;
    public EngineVariantConfig engineVariantConfig;

    public SpoilerBaseConfig spoilerBaseConfig;
    public SpoilerVariantConfig spoilerVariantConfig;

    public CarFrameBaseConfig carFrameBaseConfig;
    public CarFrameVariantConfig carFrameVariantConfig;

    static Warehouse CurrentWarehouse => G.GameFlowController.CurrentWarehouse;

    [Button]
    public void SpawnCar()
    {
        G.ProductManager.CreateCar(
            carBaseConfig,
            carVariantConfig,
           CurrentWarehouse.GetEmptyLocation() );
    }

    [Button]
    public void SpawnWheel()
    {
        G.ProductManager.CreateWheel(
            wheelBaseConfig,
            wheelVariantConfig,
           CurrentWarehouse.GetEmptyLocation() );
    }
    [Button]
    public void SpawnEngine()
    {
        G.ProductManager.CreateEngine(
            engineBaseConfig,
            engineVariantConfig,
            CurrentWarehouse.GetEmptyLocation() );
    }
    [Button]
    public void SpawnSpoiler()
    {
        G.ProductManager.CreateSpoiler(
            spoilerBaseConfig,
            spoilerVariantConfig,
            CurrentWarehouse.GetEmptyLocation() );
    }

}