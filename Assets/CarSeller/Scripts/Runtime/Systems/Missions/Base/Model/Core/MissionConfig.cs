using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "MissionConfig", menuName = "Configs/Missions/Mission Config")]
public class MissionConfig : SerializedScriptableObject
{
    public string MissionId;
    public string Description;

    [PropertyTooltip("Things for bringing content into the world for the duration of the mission. Important: those effects should be owned by mission runtimes and destroyed with the missions. ")]
    public List<MissionEffect> MissionUnlockEffects = new List<MissionEffect>();
    public List<MissionCondition> StartConditions = new List<MissionCondition>();

    [PropertyTooltip("Things for bringing content into the world for the duration of the mission. Important: those effects should be owned by mission runtimes and destroyed with the missions. ")]
    public List<MissionEffect> MissionStartEffects = new List<MissionEffect>();

    public List<MissionCondition> SuccessConditions = new List<MissionCondition>();
    [PropertyTooltip("Things to change in the <i>world</i> after finishing mission, like unlocking next missions.")]
    public List<MissionEffect> MissionCompleteEffects = new List<MissionEffect>();

    public List<MissionCondition> FailConditions = new List<MissionCondition>();
    [PropertyTooltip("Things to change in the <i>world</i> after failing mission, like locking retry for time or reputation loss.")]
    public List<MissionEffect> MissionFailEffects = new List<MissionEffect>()
    {
        new ResetMissionEffect()
    };

    public List<RewardBundle> RewardBundles = new List<RewardBundle>();
}

/// <summary>
/// Checks for conditions, no control/creation
/// </summary>
public abstract class MissionCondition
{
    public abstract MissionConditionRuntime CreateRuntime(MissionRuntime missionRuntime);
    public abstract class MissionConditionRuntime : IResettable
    {
        protected MissionRuntime missionRuntime;

        protected MissionConditionRuntime(MissionRuntime missionRuntime)
        {
            this.missionRuntime = missionRuntime;
        }

        public abstract void OnEvent(GameEventData data);
        public abstract bool IsSatisfied();
        public abstract void Reset();
    }
}

public struct MissionEffectContext
{
    public MissionRuntime Mission;
    public MissionEventBus EventBus;
}

public abstract class MissionEffect
{
    public abstract void Apply(MissionEffectContext context);
}

//Minimal reward bundle structure
public abstract class RewardBundle
{
    public abstract Transaction CreateTransaction();
    public abstract string GetRewardDescription();
}

[Serializable]
public class MissionLauncherConfig
{
    /// <summary>
    /// Description to show in the context menu when interacting with the mission launcher
    /// </summary>
    public string ctxDescription = "Start Mission";
    public PinStyle pinStyle = new PinStyle();
    public CityMarkerRef cityMarkerRef = new CityMarkerRef();
}

[Serializable]
public class PinStyle
{
    public Sprite Icon;
    public string Text = "";
    public Color ForegroundColor = Color.black;
    public Color BackgroundColor = Color.white;

    public PinStyle Clone()
    {
        return new PinStyle
        {
            Icon = this.Icon,
            Text = this.Text,
            ForegroundColor = this.ForegroundColor,
            BackgroundColor = this.BackgroundColor
        };
    }
}

public interface IResettable
{
    void Reset();
}

public interface IExplainable
{
    string GetExplanation();
}