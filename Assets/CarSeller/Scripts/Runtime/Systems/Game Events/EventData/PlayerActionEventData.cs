public enum PlayerOutcome
{
    Canceled,
    Caught,
    Succeeded
}
public class PlayerActionEventData : GameEventData
{
    
    public PlayerOutcome Outcome { get; private set; }
    public Warehouse Warehouse { get; private set; } // only set when Outcome == Succeeded

    public PlayerActionEventData(PlayerOutcome outcome, Warehouse warehouse = null)
    {
        Outcome = outcome;
        Warehouse = warehouse;
    }
}

public class PlayerAcceptedEventData : GameEventData
{
    //TODO evolve Accept into a general structure
    public readonly MissionRuntime acceptedMission;
    public PlayerAcceptedEventData(MissionRuntime mission)
    {
        acceptedMission = mission;
    }
}

public class PlayerBustedEventData : GameEventData
{
    public PlayerBustedEventData()
    {
    }
}