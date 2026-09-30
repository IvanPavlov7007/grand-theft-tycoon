using System;
using UnityEngine;

public class WorldManager
{
    //TODO register more things in the world registry
    World World => World.Instance;

    //TODO extract into a specific creation service
    // On bootstrap, create the initial cities and warehouses
    private void addWarehouse(City city, WarehouseConfig warehouseConfig)
    {
        Warehouse warehouse = new Warehouse(warehouseConfig);

        CityPosition pos;

        if (warehouseConfig.Marker != null && warehouseConfig.Marker.IsValid)
        {
            var markerData = warehouseConfig.Marker.GetMarkerData(warehouseConfig);

            if (markerData != null)
            {
                var anchor = markerData.Anchor;
                //TODO differtiate based on anchor kind -> use node pos 
                // if on node, edge pos if on edge, world point if world point
                pos = city.GetClosestPosition(anchor.WorldPoint);
            }
            else
            {
                pos = city.GetClosestPosition(warehouseConfig.warehouseClosestInitialPosition);
            }
        }
        else
        {
            Debug.LogWarning($"WarehouseConfig '{warehouseConfig.name}' does not have a valid marker assigned. Falling back to closest position.");
            pos = city.GetClosestPosition(warehouseConfig.warehouseClosestInitialPosition);
        }
        //TODO extract into a specific creation service
        initializeWarehouse(warehouse, warehouseConfig);
        G.OwnershipService.RegisterOwnable(warehouse);
        CityEntitiesCreationHelper.CreateWarehouse(warehouse, pos);
    }

    private void initializeWarehouse(Warehouse warehouse, WarehouseConfig warehouseConfig)
    {
        foreach (var productToSpawn in warehouseConfig.initialProductsToSpawn)
        {
            ILocation insideWarehouseLocation = warehouse.GetEmptyLocation();
            switch (productToSpawn.productBaseConfig)
            {
                case CarBaseConfig carBaseConfig:
                    CarVariantConfig carVariantConfig = productToSpawn.productVariantConfig as CarVariantConfig;
                    G.ProductManager.CreateCar(carBaseConfig, carVariantConfig, insideWarehouseLocation);
                    break;
                case WheelBaseConfig wheelBaseConfig:
                    WheelVariantConfig wheelVariantConfig = productToSpawn.productVariantConfig as WheelVariantConfig;
                    G.ProductManager.CreateWheel(wheelBaseConfig, wheelVariantConfig, insideWarehouseLocation);
                    break;
                case EngineBaseConfig engineBaseConfig:
                    EngineVariantConfig engineVariantConfig = productToSpawn.productVariantConfig as EngineVariantConfig;
                    G.ProductManager.CreateEngine(engineBaseConfig, engineVariantConfig, insideWarehouseLocation);
                    break;
                case SpoilerBaseConfig spoilerBaseConfig:
                    SpoilerVariantConfig spoilerVariantConfig = productToSpawn.productVariantConfig as SpoilerVariantConfig;
                    G.ProductManager.CreateSpoiler(spoilerBaseConfig, spoilerVariantConfig, insideWarehouseLocation);
                    break;
            }
        }
        World.WorldRegistry.Register<Warehouse>(warehouse, warehouseConfig);
    }

    public void InitializeWorld(CityConfig cityConfig, EconomyConfig economyConfig, WorldMissionsConfig worldMissionsConfig)
    {
        Debug.Log($"InitializeWorld start - CityRoot: {(G.CityRoot != null ? $"{G.CityRoot.name} (id: {G.CityRoot.GetInstanceID()})" : "null")}");
        World.Reset();
        initializeCity(cityConfig);
        // Initialize economy after city so that economy can reference city objects if needed
        initializeEconomy(economyConfig);
        initializeWorldMissions(worldMissionsConfig);
        Debug.Log($"InitializeWorld end - CityRoot: {(G.CityRoot != null ? $"{G.CityRoot.name} (id: {G.CityRoot.GetInstanceID()})" : "null")}");
    }

    //1.
    private void initializeCity(CityConfig cityConfig)
    {
        // Check if the previously stored CityRoot is still valid (not destroyed)
        createCityRoot(cityConfig);
        
        World.Instance.City = new City(cityConfig, G.CityRoot.transform, createAspectsSystem());
        var createdTrafficLights = CityTrafficLightsSpawner.Spawn(World.Instance.City, cityConfig.CityGraph, G.CityRoot.transform);
        World.Instance.City.InitializeTrafficLights(createdTrafficLights);
        // Area overlays (hidden by default; you control them at runtime).
        CityAreasVisualsController.Ensure(World.Instance.City);

        foreach (var warehouseConfig in cityConfig.warehouseConfigs)
        {
            //TODO extract into a specific creation service
            addWarehouse(World.Instance.City, warehouseConfig);
        }

        initializeCarStashWarehouses(cityConfig.carStashWarehouseConfigs);
    }
    
    private void createCityRoot(CityConfig cityConfig)
    {
        if(G.CityRoot != null)
            return;
        var obj = GameObject.Instantiate(cityConfig.CityGraph.PrefabRoot);
        // var obj = new GameObject();
        obj.AddComponent<CityRootManager>();
        obj.name = "CityRoot"; // Ensure consistent naming
        GameObject.DontDestroyOnLoad(G.CityRoot);
    }

    AspectsSystem createAspectsSystem()
    {
            return new AspectsSystem();
    }

    //TODO make this more organic, don't just duplicate the warehouse code, extract commonalities
    private void initializeCarStashWarehouses(CarStashWarehouseConfig[] carStashWarehouseConfigs)
    {
        foreach (var carStashWarehouseConfig in carStashWarehouseConfigs)
        {
            addCarStashWarehouse(World.Instance.City, carStashWarehouseConfig);
        }
    }

    private void addCarStashWarehouse(City city, CarStashWarehouseConfig carStashWarehouseConfig)
    {
        CarStashWarehouse carStashWarehouse = new CarStashWarehouse(carStashWarehouseConfig);
        var pos = carStashWarehouseConfig.Marker.GetCityPosition();
        CityEntitiesCreationHelper.CreateCarStashWarehouse(carStashWarehouse, pos);
    }

    [Obsolete("Spawning products in city is deprecated")]
    private void spawnProductsInCity(City city, CityConfig cityConfig)
    {
        foreach (var productToSpawn in cityConfig.initialProductsToSpawn)
        {
            CityPosition randomOutsidePosition = city.GetRandomPosition();
            switch (productToSpawn.productBaseConfig)
            {
                case CarBaseConfig carBaseConfig:
                    CarVariantConfig carVariantConfig = productToSpawn.productVariantConfig as CarVariantConfig;
                    CityEntitiesCreationHelper.CreateNewCar(
                        carBaseConfig,
                        carVariantConfig,
                        randomOutsidePosition);
                    break;
                default:
                    Debug.LogError($"Only cars can be spawned outside the warehouse. Tried to spawn {productToSpawn.productBaseConfig}.");
                    break;
            }
        }
    }

    //2.
    private void initializeEconomy(EconomyConfig economyConfig)
    {
        World.Instance.Economy = new Economy(economyConfig);
    }

    private void initializeWorldMissions(WorldMissionsConfig worldMissionsConfig)
    {
        if(worldMissionsConfig == null)
        {
            Debug.LogWarning("WorldMissionsConfig is null. Skipping mission controller initialization.");
            return;
        }

        clearMissions();

        G.MissionController = new MissionController(worldMissionsConfig.allMissions);
        // Unlock starting missions
        foreach (var startingMission in worldMissionsConfig.startingMissions)
        {
            G.MissionController.UnlockMission(startingMission);
        }
    }

    private void clearMissions()
    {
        if(G.MissionController != null)
        {
            G.MissionController.Disable();
            G.MissionController = null;
        }
    }
}