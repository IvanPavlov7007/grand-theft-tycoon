using System;
using System.Collections.Generic;

/// <summary>
/// Defines all logical operations for mission management and subscribes to all internal events
/// </summary>
public abstract class MissionControllerBase
{
    MissionEventBus eventBus;
    Dictionary<MissionConfig, MissionRuntime> runtimes;
    Queue<GameEventData> eventQueue = new Queue<GameEventData>();

    public MissionControllerBase(List<MissionConfig> configs)
    : this(configs, new MissionEventBus())
    {
    }
    protected MissionControllerBase(List<MissionConfig> configs, MissionEventBus eventBus)
    {
        // Initialize Event Bus
        this.eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        InitializeEventBus();
        InitializeRuntimes(configs);
    }
    protected MissionEventBus EventBus => eventBus; // Expose for testing
    protected IReadOnlyDictionary<MissionConfig, MissionRuntime> Runtimes => runtimes; // Expose for testing

    private void InitializeEventBus()
    {
        // Subscribe to mission internal events
        // General mission events
        eventBus.Subscribe<UnlockMissionInternalEvent>(onUnlockMissionInternalEvent);
        eventBus.Subscribe<StartMissionInternalEvent>(onStartMissionRequestEvent);
        eventBus.Subscribe<CompleteMissionInternalEvent>(onCompleteMissionRequestEvent);
        eventBus.Subscribe<FailMissionInternalEvent>(onFailMissionRequestEvent);

        // Specific mission requests
        eventBus.Subscribe<UnlockMissionRequestEvent>(onUnlockMissionRequestEvent);
        eventBus.Subscribe<ResetMissionRequestEvent>(onMissionResetRequestEvent);
        eventBus.Subscribe<SpawnTargetMissionRequestEvent>(onSpawnTargetMissionRequestEvent);
        eventBus.Subscribe<SpawnMissionLauncherRequestEvent>(onSpawnMissionLauncherRequestEvent);
        eventBus.Subscribe<SpawnMoneyCollectablesRequestEvent>(onSpawnMoneyCollectablesRequestEvent);
        eventBus.Subscribe<PoliceRequestEvent>(onPoliceRequestEvent);

    }

    private void InitializeRuntimes(List<MissionConfig> configs)
    {
        runtimes = new Dictionary<MissionConfig, MissionRuntime>();
        foreach (var config in configs)
        {
            runtimes[config] = InitializeRuntime(config);
        }
    }
    private MissionRuntime InitializeRuntime(MissionConfig config)
    {
        var runtime = new MissionRuntime(config, eventBus);
        return runtime;
    }
    // Mission Control API
    /// <summary>
    /// Use this method to unlock a mission from outside the mission system.
    /// </summary>
    /// <param name="mission"></param>
    public void UnlockMission(MissionConfig mission)
    {
        var runtime = runtimes[mission];
        runtime.Unlock();
        // GameEvents.onMissionUnlocked?.Invoke(runtime);
    }
    /// <summary>
    /// Used to clean up mission-owned objects when mission status changes.
    /// </summary>
    /// <param name="mission"></param>
    void CleanupMissionObjects(MissionRuntime mission)
    {
        var newStatus = mission.Status;
        switch (newStatus)
        {
            case MissionStatus.Available:
            case MissionStatus.Active:
            case MissionStatus.Succeeded:
            case MissionStatus.Failed:
                CleanupMissionObjectsImp(mission);
                break;
            default:
                break;
        }
    }
    /// <summary>
    /// Implementation of mission-owned object cleanup. Left to derived classes.
    /// </summary>
    /// <param name="mission"></param>
    protected abstract void CleanupMissionObjectsImp(MissionRuntime mission);

    // Mission Event Bus
    //Internal Event Handlers

    void onUnlockMissionInternalEvent(UnlockMissionInternalEvent e)
    {
        CleanupMissionObjects(e.Mission);
        executeMissionEffects(e.Mission.UnlockEffects, createMissionEffectContext(e.Mission));
        GameEvents.Instance.onMissionUnlocked?.Invoke(new MissionUnlockedEventData(e.Mission));
    }
    void onStartMissionRequestEvent(StartMissionInternalEvent e)
    {
        CleanupMissionObjects(e.Mission);
        executeMissionEffects(e.Mission.StartEffects, createMissionEffectContext(e.Mission));
        GameEvents.Instance.onMissionStarted?.Invoke(new MissionStartedEventData(e.Mission));
    }
    void onCompleteMissionRequestEvent(CompleteMissionInternalEvent e)
    {
        CleanupMissionObjects(e.Mission);
        executeMissionEffects(e.Mission.CompleteEffects, createMissionEffectContext(e.Mission));
        GameEvents.Instance.onMissionCompleted?.Invoke(new MissionCompletedEventData(e.Mission));
    }
    void onFailMissionRequestEvent(FailMissionInternalEvent e)
    {
        CleanupMissionObjects(e.Mission);
        executeMissionEffects(e.Mission.FailEffects, createMissionEffectContext(e.Mission));
        GameEvents.Instance.onMissionFailed?.Invoke(new MissionFailedEventData(e.Mission));
    }

    void executeMissionEffects(List<MissionEffect> effects, MissionEffectContext context)
    {
        foreach (var effect in effects)
        {
            effect.Apply(context);
        }
    }

    MissionEffectContext createMissionEffectContext(MissionRuntime mission)
    {
        return new MissionEffectContext
        {
            Mission = mission,
            EventBus = eventBus
        };
    }

    // MissionEffects Event Handlers
    void onUnlockMissionRequestEvent(UnlockMissionRequestEvent requestEvent)
    {
        UnlockMission(requestEvent.toUnlock);
    }
    void onMissionResetRequestEvent(ResetMissionRequestEvent requestEvent)
    {
        //TODO Be careful with resetting by overwriting the runtime. Consider preserving some state if needed
        // Check if we need to do any cleanup before resetting or is it possible to reset directly
        runtimes[requestEvent.toReset] = new MissionRuntime(requestEvent.toReset, eventBus);
        runtimes[requestEvent.toReset].Unlock();
    }
    protected abstract void onSpawnTargetMissionRequestEvent(SpawnTargetMissionRequestEvent requestEvent);
    protected abstract void onSpawnMissionLauncherRequestEvent(SpawnMissionLauncherRequestEvent requestEvent);
    protected abstract void onSpawnMoneyCollectablesRequestEvent(SpawnMoneyCollectablesRequestEvent requestEvent);
    protected abstract void onPoliceRequestEvent(PoliceRequestEvent requestEvent);

    // Game Events Funneling
    #region globalGameEvents

    // Game Events Funneling

    public virtual void OnPlayerAccepted(PlayerAcceptedEventData playerAcceptedEvent)
    {
        updateMissionRuntimes(playerAcceptedEvent);
    }

    public virtual void OnCityTargetReached(CityTargetReachedEventData targetReachedEvent)
    {
        updateMissionRuntimes(targetReachedEvent);
    }
    public virtual void OnPlayerBusted(PlayerBustedEventData playerBustedEventData)
    {
        updateMissionRuntimes(playerBustedEventData);
    }
    public virtual void OnUpdate(float deltaTime)
    {
        updateMissionRuntimes(new TimePassEvent(deltaTime));
    }
    #endregion
    protected void updateMissionRuntimes(GameEventData gameEventData)
    {
        eventQueue.Enqueue(gameEventData);
        processEventQueue();
    }

    bool queueProcessing = false;

    private void processEventQueue()
    {
        if (queueProcessing)
            return;
        queueProcessing = true;
        while (eventQueue.Count > 0)
        {
            var gameEventData = eventQueue.Dequeue();
            processSingleEvent(gameEventData);
        }
        queueProcessing = false;
    }

    private void processSingleEvent(GameEventData gameEventData)
    {
        var snapshotRuntimes = new List<MissionRuntime>(runtimes.Values);
        foreach (var runtime in snapshotRuntimes)
        {
            runtime.OnEvent(gameEventData);
        }
    }

}