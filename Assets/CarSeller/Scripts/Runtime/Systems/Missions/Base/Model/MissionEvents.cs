/// <summary>
/// Class for communication with the mission controller over Event Bus.
/// Signalizing "intentions"
/// </summary>
public abstract class MissionInternalEvent {
    public readonly MissionRuntime Mission;
    protected MissionInternalEvent(MissionRuntime mission)
    {
        this.Mission = mission;
    }
}

public class UnlockMissionInternalEvent : MissionInternalEvent
{
    public UnlockMissionInternalEvent(MissionRuntime mission)
        : base(mission)
    {
    }
}
public class StartMissionInternalEvent : MissionInternalEvent
{
    public StartMissionInternalEvent(MissionRuntime mission)
        : base(mission)
    {
    }
}

public class CompleteMissionInternalEvent : MissionInternalEvent
{
    public CompleteMissionInternalEvent(MissionRuntime mission)
        : base(mission)
    {

    }
}

public class FailMissionInternalEvent : MissionInternalEvent
{
    public FailMissionInternalEvent(MissionRuntime mission)
        : base(mission)
    {
    }
}

// Request Events

public abstract class SpawnTemporaryRequestEvent : MissionInternalEvent
{
    public SpawnTemporaryRequestEvent(MissionRuntime mission) 
        : base(mission)
    {
    }
}

public class SpawnTargetMissionRequestEvent : SpawnTemporaryRequestEvent
{
    public readonly CityMarkerRef TargetMarker;
    public SpawnTargetMissionRequestEvent(MissionRuntime mission, CityMarkerRef targetMarker) 
        : base(mission)
    {
        this.TargetMarker = targetMarker;
    }
}

public class SpawnMissionLauncherRequestEvent : SpawnTemporaryRequestEvent
{
    public readonly MissionLauncherConfig LauncherConfig;
    public SpawnMissionLauncherRequestEvent(MissionRuntime mission, MissionLauncherConfig launcherConfig) 
        : base(mission)
    {
        this.LauncherConfig = launcherConfig;
    }
}

public class PoliceRequestEvent : SpawnTemporaryRequestEvent
{
    public PoliceRequestEvent(MissionRuntime mission) : base(mission)
    {
    }
}

public class UnlockMissionRequestEvent : MissionInternalEvent
{
    public readonly MissionConfig toUnlock;

    public UnlockMissionRequestEvent(MissionRuntime thisMission, MissionConfig toUnlock)
        : base(thisMission)
    {
        this.toUnlock = toUnlock;
    }
}

internal class ResetMissionRequestEvent : MissionInternalEvent
{
    public readonly MissionConfig toReset;

    public ResetMissionRequestEvent(MissionRuntime thisMission, MissionConfig toReset)
        : base(thisMission)
    {
        this.toReset = toReset;
    }
}

public class SpawnMoneyCollectablesRequestEvent : SpawnTemporaryRequestEvent
{
    public readonly float reward;
    public readonly int count;

    public SpawnMoneyCollectablesRequestEvent(MissionRuntime mission, float reward, int count)
        : base(mission)
    {
        this.reward = reward;
        this.count = count;
    }
}


