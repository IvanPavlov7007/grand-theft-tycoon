public abstract class GameEventData { }
public class ProductCreatedEventData : GameEventData
{
    public Product Product { get; private set; }
    public ILocation Location { get; private set; }
    public ProductCreatedEventData(Product product, ILocation location)
    {
        Product = product;
        Location = location;
    }
}

public class SceneOpenedEventData : GameEventData
{
    public readonly GameFlowController.GameSceneType SceneType;
    public SceneOpenedEventData(GameFlowController.GameSceneType sceneType)
    {
        SceneType = sceneType;
    }
}

public class ProductDestroyedEventData : GameEventData
{
    public Product Product { get; private set; }
    public ProductDestroyedEventData(Product product)
    {
        Product = product;
    }
}   

public class ProductLocationChangedEventData : GameEventData
{
    public ProductLocationChangedEventData(Product product, ILocation newLocation, ILocation oldLocation)
    {
        Product = product;
        NewLocation = newLocation;
        OldLocation = oldLocation;
    }

    public Product Product { get; private set; }
    public ILocation NewLocation { get; private set; }
    public ILocation OldLocation { get; set; }
}

public class LocatableStateChangedEventData : GameEventData
{
    public ILocatable Locatable { get; private set; }
    public LocatableStateChangedEventData(ILocatable locatable)
    {
        Locatable = locatable;
    }
}

public class LocatableCreatedEventData : GameEventData
{
    public ILocatable Locatable { get; private set; }
    public ILocation Location { get; private set; }
    public LocatableCreatedEventData(ILocatable locatable, ILocation location)
    {
        Locatable = locatable;
        Location = location;
    }
}

public class LocatableDestroyedEventData : GameEventData
{
    public ILocatable Locatable { get; private set; }
    public LocatableDestroyedEventData(ILocatable locatable)
    {
        Locatable = locatable;
    }
}

public class LocatableLocationChangedEventData : GameEventData
{
    public LocatableLocationChangedEventData(ILocatable locatable, ILocation newLocation, ILocation oldLocation)
    {
        Locatable = locatable;
        NewLocation = newLocation;
        OldLocation = oldLocation;
    }

    public ILocatable Locatable { get; private set; }
    public ILocation NewLocation { get; private set; }
    public ILocation OldLocation { get; set; }
}

public class GameStateChangeEventData : GameEventData
{ 
    public GameState oldState { get; private set; }
    public GameState newState { get; private set; }

    public GameStateChangeEventData(GameState oldState, GameState newState)
    {
        this.oldState = oldState;
        this.newState = newState;
    }
}

public class OwnershipChangedEventData : GameEventData
{
    public readonly IOwnable Item;
    public readonly IOwnable OldOwner;
    public readonly IOwnable NewOwner;

    public OwnershipChangedEventData(IOwnable item, IOwnable oldOwner, IOwnable newOwner)
    {
        Item = item;
        OldOwner = oldOwner;
        NewOwner = newOwner;
    }
}

public class PlayerMoneyChangeEventData : GameEventData
{
    public PlayerMoneyChangeEventData(Player player, float oldMoney, float newMoney)
    {
        Player = player;
        OldMoney = oldMoney;
        NewMoney = newMoney;
    }

    public Player Player { get; private set; }
    public float OldMoney { get; private set; }
    public float NewMoney { get; private set; }
    
}

public class CityTargetReachedEventData : GameEventData
{
    public readonly CityEntity ReachedObject;
    public readonly TriggerContext TriggerContext;

    public CityTargetReachedEventData(CityEntity reachedObject, TriggerContext triggerContext)
    {
        ReachedObject = reachedObject;
        TriggerContext = triggerContext;
    }
}

public class VehicleControlStateChangedEventData : GameEventData
{
    public readonly VehicleController.VehicleControlState OldState;
    public readonly VehicleController.VehicleControlState NewState;
    public VehicleControlStateChangedEventData(VehicleController.VehicleControlState oldState, VehicleController.VehicleControlState newState)
    {
        OldState = oldState;
        NewState = newState;
    }
}