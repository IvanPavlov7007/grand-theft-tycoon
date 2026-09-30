using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// global services locator
/// </summary>
public static class G
{
    public static bool runIntialized = false;
    public static GameManager GameManager;
    
    public static SellPriceWrapper CurrentSellPriceWrapped => Economy.CurrentPriceWrapper;
    public static SellPriceCalculator SellPriceCalculator => Economy.SellPriceCalculator;

    public static GameConfig Config => GameMainConfig.Instance.GameConfig;
    public static CityConfig CityConfig => Config.CityConfig;
    public static EconomyConfig EconomyConfig => Config.EconomyConfig;
    public static WorldMissionsConfig WorldMissionsConfig => Config.WorldMissionsConfig;

    //References
    public static City City => World.Instance.City;
    public static Economy Economy => World.Instance.Economy;
    public static Player Player => Economy.Player;
    public static GameState GameState => GameFlowController.GameState;

    public static TransactionProcessor TransactionProcessor;
    public static ProcessRunner ProcessRunner;

    public static ICarWarehousePolicy CarWarehousePolicy;
    public static AcquisitionResolver AcquisitionResolver = new AcquisitionResolver();

    public static CarSpawnManager CarSpawnManager => Economy.CarSpawnManager;

    public static AreaBalancingContent Balancing => Config.GameDatabaseContainer.Balancing;

    //MODEL AND VIEW MIXED (SORRY)
    public static GameObject CityRoot { get; set; }

    //REPRESENTATION

    public static ProductManager ProductManager = new ProductManager();

    public static ProductLifetimeService ProductLifetimeService = new ProductLifetimeService();
    public static OwnershipResolutionService OwnershipService = new OwnershipResolutionService();

    public static GameFlowManager GameFlowManager;
    public static GameFlowController GameFlowController = new GameFlowController();

    public static WorldManager WorldManager = new WorldManager();

    public static CarMechanicService CarMechanicService;

    public static PlayerManager PlayerManager = new PlayerManager();

    public static CarStripper CarStripper = new CarStripper();

    public static SimplifiedCarsManager SimplifiedCarsManager = new SimplifiedCarsManager();

    //City

    public static CityEntityLifetimeService CityEntityLifetimeService = new CityEntityLifetimeService();
    public static CityActionService CityActionService = new CityActionService();

    public static WarehouseEntryCooldownService WarehouseEntryCooldownService = new WarehouseEntryCooldownService();

    //Interaction
    public static IInteractionManager InteractionManager;
    public static MissionController MissionController;
    public static VehicleController VehicleController;

    //VIEW
    //View builders
    //Warehouse
    public static WarehouseProductGameObjectBuilder warehouseProductViewBuilder => physicalWarehouseProductViewBuilder;

    public static MonolithProductGameObjectBuilder monolithWarehouseProductViewBuilder => viewBuildersConfig.monolithWarehouseProductViewBuilder;
    public static PhysicalProductGameObjectBuilder physicalWarehouseProductViewBuilder => viewBuildersConfig.physicalWarehouseProductViewBuilder;
    public static CarPartGameObjectBuilder carPartViewBuilder => viewBuildersConfig.carPartViewBuilder;

    //City
    public static CityViewObjectBuilder cityViewObjectBuilder => viewBuildersConfig.cityViewObjectBuilder;
    public static CityViewStreetsBuilder cityViewStreetsBuilder = new CityViewStreetsBuilder();

    public static CarsDefinitionLibrary SimplifiedCarsCreationBuilder => viewBuildersConfig.simplifiedCarsCreationBuilder;

    static ViewBuildersConfig viewBuildersConfig;


    //to sort
    public static Dictionary<string, CityArea> Areas;
    internal static BuyerManager BuyerManager => Economy.BuyerManager;

    public static ColorPalette ColorPalette => Config.ColorPalette;

    public static PersonalVehicleShop VehicleShop => World.Instance.VehicleShop;
    public static GameCursor GameCursor { get; set; }
    public static InteractionController InteractionController { get; set; }
    public static LocatableStateChangeEventFunnel LocatableStateChangeEventFunnel { get; set; }
    public static CameraMovementManager CameraMovementManager { get; set; }
    public static CarShopSceneManager CarShopSceneManager { get; set; }
    public static CitySceneManager CitySceneManager { get; set; }
    public static StreetsSingleton StreetsSingleton { get; set; }
    public static CityFogRenderer CityFogRenderer { get; set; }
    public static ContextMenuManager ContextMenuManager { get; set; }
    public static FixedContextMenuManager FixedContextMenuManager { get; set; }
    public static DebugCustomActions DebugCustomActions { get; set; }
    public static DebugGameBootstrapper DebugGameBootstrapper { get; set; }
    public static CollectablesManager CollectablesManager { get; set; }
    public static IconBuilderManager IconBuilderManager { get; set; }
    public static PlayerInputController PlayerInputController { get; set; }
    public static PlayerInputLocator PlayerInputLocator { get; set; }
    public static UIInputController UIInputController { get; set; }
    public static PoliceManager PoliceManager { get; set; }
    public static CongradulationsMenu CongradulationsMenu { get; set; }
    public static MoneyGoalManager MoneyGoalManager { get; set; }
    public static PlayerPossessionTracker PlayerPossessionTracker { get; set; }
    public static WarehouseSceneManager WarehouseSceneManager { get; set; }
    public static DynamicCanvasGroups DynamicCanvasGroups { get; set; }
    public static BlockUIManager BlockUIManager { get; set; }
    public static CarShopUIManager CarShopUIManager { get; set; }
    public static CityUIManager CityUIManager { get; set; }
    public static VehicleControlUI VehicleControlUI { get; set; }
    public static ContextMenuCanvas ContextMenuCanvas { get; set; }
    public static FixedContextMenuCanvas FixedContextMenuCanvas { get; set; }
    public static GlobalHintManager GlobalHintManager { get; set; }
    public static PauseMenuManager PauseMenuManager { get; set; }
    public static UIEffectsDisplayer UIEffectsDisplayer { get; set; }
    public static UI_FX_Manager UIFXManager { get; set; }
    public static AreaProgressionManager AreaProgressionManager { get; set; }
    public static AudioController AudioController { get; set; }

    public static void Initialize( ViewBuildersConfig handlersConfig)
    {
        Debug.Log("Initializing global services locator");
        viewBuildersConfig = handlersConfig;

        initializeTransactionProcessor();

        SimplifiedCarsCreationBuilder.Initialize();
        CarWarehousePolicy = new CarIntoWarehousePolicy();
        VehicleController = new VehicleController();
        GameFlowManager = new GameFlowManager();
        CarMechanicService = new CarMechanicService();
        ProcessRunner = new ProcessRunner();
    }

    private static void initializeTransactionProcessor()
    {
        TransactionProcessor = new TransactionProcessor(new Dictionary<System.Type, ITransactionHandler>
        {
            { typeof(PurchaseTransaction), new PurchaseHandler() },
            { typeof(SellTransaction), new SellHandler() },
            { typeof(RewardTransaction), new RewardHandler() },
            //{ TransactionType.Lose, new LoseHandler() },
            //{ TransactionType.Confiscate, new ConfiscateHandler() },
            //{ TransactionType.Steal, new StealHandler()   },
            //{ TransactionType.Exchange, new ExchangeHandler() },
            { typeof(StripCarTransaction), new StripCarHandler() },
            { typeof(PullCarFromWarehouseTransaction), new PullCarFromWarehouseHandler() },
            { typeof(PutProductsInHolderTransaction), new PutProductsInHolderHandler() },
        });
    }
}
