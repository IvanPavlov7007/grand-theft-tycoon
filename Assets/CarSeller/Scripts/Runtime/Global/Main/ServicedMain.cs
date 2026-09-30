using System;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// GameMains are the highest level game controllers, initializing world and game logic according to the selected game mode
/// don't talk to them from anywhere else!
/// </summary>
public sealed class ServicedMain : MonoBehaviour
{
    static bool isInitialized = false;
    PersonalVehicleShop shop;

    public GameObject cityRoot;
    
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Main()
    {
        if (!isInitialized)
        {
            GameObject servicedMain = new GameObject("GameMain");
            G.runIntialized = false;
            servicedMain.AddComponent<ServicedMain>();
            DontDestroyOnLoad(servicedMain);
            isInitialized = true;
        }
    }

    private void Awake()
    {
        Initialize(GameMainConfig.Instance.GameConfig);
    }

    private void OnDestroy()
    {
        isInitialized = false;
    }

    public void Initialize(GameConfig gameConfig)
    {
        InitializeServices();
        Debug.Log("Initializing game config");
        ResetStaticState(gameConfig);
        ResetData(gameConfig);
    }

    void InitializeServices()
    {
        G.GameManager = gameObject.AddComponent<GameManager>();
        G.InteractionController = gameObject.AddComponent<InteractionController>();
        
        G.LocatableStateChangeEventFunnel = gameObject.AddComponent<LocatableStateChangeEventFunnel>();

    }

    public void ResetData(GameConfig gameConfig)
    {
        G.runIntialized = false;
        InitializeWorld(gameConfig);
        InitializeLogic(gameConfig);
        G.runIntialized = true;
    }

    public static void GameReset()
    {
        //TODO make it just reset run and reset scene
        FindAnyObjectByType<ServicedMain>()?.ResetData(GameMainConfig.Instance.GameConfig);
    }

    public void ResetStaticState(GameConfig gameConfig)
    {
        GameEvents.Instance.Reset();
        G.Initialize(GameMainConfig.Instance.ViewBuilders);
    }
    public void InitializeWorld(GameConfig gameConfig)
    {
        G.WorldManager.InitializeWorld(gameConfig.CityConfig, gameConfig.EconomyConfig, gameConfig.WorldMissionsConfig);
        G.CarSpawnManager.SubscribeToEvents();
        initializeCityAreas();
        createShop(gameConfig.VehicleControllerConfig.vehicleShopConfig);
    }
    
    private void initializeCityAreas()
    {
        G.Areas = G.Balancing.AreaBalancingByAreaName.
            ToDictionary(area => area.Id, area => new CityArea(area));
    }
    
    public void InitializeLogic(GameConfig gameConfig)
    {
        var roamingState = new FreeRoamGameState();
        G.GameFlowController = new GameFlowController();
        G.GameFlowController.SetGameState(roamingState);

        G.VehicleController.Initialize(gameConfig.VehicleControllerConfig, shop.PersonalVehiclesList);
    }
    
    private void createShop(VehicleShopConfig config)
    {
        shop = new PersonalVehicleShop(config);
        World.Instance.VehicleShop = shop;

        //var pos = config.Marker.GetCityPosition();
        //CityEntitiesCreationHelper.CreatePersonalVehicleShop(shop, pos);
    }
}