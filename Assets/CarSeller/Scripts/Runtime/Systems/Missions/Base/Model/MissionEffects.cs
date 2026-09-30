using System;
[Serializable]
public class UnlockMissionEffect : MissionEffect
{
    public MissionConfig mission;

    public override void Apply(MissionEffectContext context)
    {
        context.EventBus.Emit(new UnlockMissionRequestEvent(context.Mission, mission));
    }
}

[Serializable]
public class ResetMissionEffect : MissionEffect
{
    public override void Apply(MissionEffectContext context)
    {
        context.EventBus.Emit(new ResetMissionRequestEvent(context.Mission, context.Mission.Config));
    }
}



[Serializable]
public class SpawnTargetEffect : MissionEffect
{
    public CityMarkerRef targetMarker = new CityMarkerRef();
    public override void Apply(MissionEffectContext context)
    {
        context.EventBus.Emit(new SpawnTargetMissionRequestEvent(context.Mission, targetMarker));
    }
}

[Serializable]
public class SpawnMissionLauncherEffect : MissionEffect
{
    public MissionLauncherConfig launcherConfig = new MissionLauncherConfig();
    public override void Apply(MissionEffectContext context)
    {
        context.EventBus.Emit(new SpawnMissionLauncherRequestEvent(context.Mission, launcherConfig));
    }
}

[Serializable]
public class PoliceEffect : MissionEffect
{
    public override void Apply(MissionEffectContext context)
    {
        context.EventBus.Emit(new PoliceRequestEvent(context.Mission));
    }
}


[Serializable]
public class SpawnMoneyCollectablesEffect : MissionEffect
{
    public float reward = 50f;
    public int count = 20;
    public override void Apply(MissionEffectContext context)
    {
        context.EventBus.Emit(new SpawnMoneyCollectablesRequestEvent(context.Mission, reward, count));
    }
}

