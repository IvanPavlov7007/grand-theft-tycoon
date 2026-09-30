using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MissionRuntime
{
    public readonly MissionConfig Config;
    public MissionStatus Status { get; private set; }
    public void SetStatus(MissionStatus newStatus)
    {
        Status = newStatus;
    }

    public bool IsActive => Status == MissionStatus.Active;

    public List<MissionCondition.MissionConditionRuntime> startConditions =
        new List<MissionCondition.MissionConditionRuntime>();
    public List<MissionCondition.MissionConditionRuntime> successConditions =
        new List<MissionCondition.MissionConditionRuntime>();
    public List<MissionCondition.MissionConditionRuntime> failureConditions =
        new List<MissionCondition.MissionConditionRuntime>();
    public List<MissionEffect> UnlockEffects => Config.MissionUnlockEffects;
    public List<MissionEffect> StartEffects => Config.MissionStartEffects;
    public List<MissionEffect> CompleteEffects => Config.MissionCompleteEffects;
    public List<MissionEffect> FailEffects => Config.MissionFailEffects;

    public List<RewardBundle> RewardBundles => Config.RewardBundles;

    public event Action<MissionInternalEvent> onComplete;

    readonly MissionEventBus EventBus;

    public MissionRuntime(MissionConfig config, MissionEventBus eventBus)
    {
        Config = config;
        EventBus = eventBus;
        Status = MissionStatus.Locked;

        initializeConditions();
    }
    void initializeConditions()
    {
        foreach (var condition in Config.StartConditions)
        {
            var conditionRuntime = condition.CreateRuntime(this);
            startConditions.Add(conditionRuntime);
        }
        foreach (var condition in Config.SuccessConditions)
        {
            var conditionRuntime = condition.CreateRuntime(this);
            successConditions.Add(conditionRuntime);
        }
        foreach (var condition in Config.FailConditions)
        {
            var conditionRuntime = condition.CreateRuntime(this);
            failureConditions.Add(conditionRuntime);
        }
    }

    // Called from within this class
    void Start()
    {
        switch (Status)
        {
            case MissionStatus.Locked:
                Debug.LogWarning($"Mission {this} is calling Start(), while not being available");
                break;
            case MissionStatus.Available:
                break;
            case MissionStatus.Active:
            case MissionStatus.Succeeded:
            case MissionStatus.Failed:
            default:
                Debug.LogWarning($"Mission {this} is calling Start(), while being already started");
                break;
        }

        SetStatus(MissionStatus.Active);
        EventBus.Emit(new StartMissionInternalEvent(this));
    }
    void Complete()
    {
        if (Status != MissionStatus.Active)
        {
            Debug.LogWarning($"Mission {this} is calling Complete(), while not being active");
        }
        else if (Status == MissionStatus.Succeeded)
        {
            Debug.LogWarning($"Mission {this} is calling Complete(), while already being completed");
            return;
        }

        SetStatus(MissionStatus.Succeeded);

        EventBus.Emit(new CompleteMissionInternalEvent(this));
    }
    void Fail()
    {
        if (Status != MissionStatus.Active)
        {
            Debug.LogWarning($"Mission {this} is calling Fail(), while not being active");
        }
        else if (Status == MissionStatus.Failed)
        {
            Debug.LogWarning($"Mission {this} is calling Fail(), while already being failed");
            return;
        }
        SetStatus(MissionStatus.Failed);
        EventBus.Emit(new FailMissionInternalEvent(this));
    }
    // Exception: called by MissionController to unlock mission
    /// <summary>
    /// Should be called by MissionController(only!) to unlock the mission
    /// </summary>
    public void Unlock()
    {
        if (Status != MissionStatus.Locked)
        {
            Debug.LogWarning($"Mission {this} is calling Unlock(), while not being locked");
            return;
        }
        SetStatus(MissionStatus.Available);
        EventBus.Emit(new UnlockMissionInternalEvent(this));
    }

    public void Reset()
    {
        if (Status == MissionStatus.Locked)
        {
            Debug.LogWarning($"Mission {this} is calling Reset(), while being locked");
            return;
        }
        SetStatus(MissionStatus.Locked);
        foreach (var c in startConditions)
        {
            c.Reset();
        }
        foreach (var c in successConditions)
        {
            c.Reset();
        }
        foreach (var c in failureConditions)
        {
            c.Reset();
        }
        // No event emitted on reset
    }


    /// <summary>
    /// Dispatch event to conditions and check for completion
    /// </summary>
    /// <param name="data"></param>
    public void OnEvent(GameEventData data)
    {
        switch (Status)
        {
            case MissionStatus.Locked:
                // Locked missions should not react to events
                return;

            case MissionStatus.Available:
                HandleAvailable(data);
                break;

            case MissionStatus.Active:
                HandleActive(data);
                break;

            case MissionStatus.Succeeded:
            case MissionStatus.Failed:
                // Terminal states: ignore events
                return;
        }
    }
    void HandleAvailable(GameEventData data)
    {
        foreach (var c in startConditions)
            c.OnEvent(data);

        if (startConditions.All(c => c.IsSatisfied()))
        {
            Start();
        }
    }
    void HandleActive(GameEventData data)
    {
        foreach (var c in successConditions)
            c.OnEvent(data);
        foreach (var c in failureConditions)
            c.OnEvent(data);

        // Failure has priority
        if (failureConditions.Any(c => c.IsSatisfied()))
        {
            Fail();
            return;
        }

        if (successConditions.All(c => c.IsSatisfied()))
        {
            Complete();
        }
    }

    public override string ToString()
    {
        return base.ToString() + " with status " + Status.ToString();
    }
}
public enum MissionStatus
{
    Locked,
    Available,
    Active,
    Succeeded,
    Failed,
}