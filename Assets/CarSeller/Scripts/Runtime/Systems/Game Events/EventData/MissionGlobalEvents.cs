// Mission Global Events

public class MissionUnlockedEventData : GameEventData
{
    public readonly MissionRuntime Mission;
    public MissionUnlockedEventData(MissionRuntime mission)
    {
        Mission = mission;
    }
}

public class MissionStartedEventData : GameEventData
{
    public readonly MissionRuntime Mission;

    public MissionStartedEventData(MissionRuntime mission)
    {
        Mission = mission;
    }
}

public class MissionCompletedEventData : GameEventData
{
    public readonly MissionRuntime Mission;
    public MissionCompletedEventData(MissionRuntime mission)
    {
        Mission = mission;
    }
}

public class MissionFailedEventData : GameEventData
{
    public readonly MissionRuntime Mission;
    public MissionFailedEventData(MissionRuntime mission)
    {
        Mission = mission;
    }
}