using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Implementation that wires up the MissionControllerBase to the game systems.
/// Extends with mission-owned object tracking and cleanup.
/// </summary>
public class MissionController : MissionControllerBase
{
    Dictionary<MissionRuntime, HashSet<IDestroyable>> missionOwnedObjects = new();
    TinyMonoBehaviourHelper monoBehaviourHelper;

    public MissionController(List<MissionConfig> configs) : base(configs)
    {
        monoBehaviourHelper = TinyMonoBehaviourHelper.Create("MissionControllerHelper");
        GameObject.DontDestroyOnLoad(monoBehaviourHelper.gameObject);
        Enable();
    }

    public void Enable()
    {
        // Extension internal event handlers
        GameEvents.Instance.OnTargetReachDragEnded += tryCheckIfMissionLauncherAndShowPopUp;
        GameEvents.Instance.onMissionCompleted += showMissionCompletedInfo;
        GameEvents.Instance.onMissionCompleted += rewardPlayerForMissionCompletion;
        GameEvents.Instance.onMissionStarted += enterMissionStateOnMissionStart;
        GameEvents.Instance.onMissionCompleted += exitMissionStateOnMissionCompleted;
        GameEvents.Instance.onMissionFailed += exitMissionStateOnMissionFailed;
        GameEvents.Instance.onMissionFailed += showMissionFailedInfo;

        // External to mission event handlers
        monoBehaviourHelper.OnUpdateEvent += OnUpdate;
        GameEvents.Instance.OnTargetReachDragEnded += OnCityTargetReached;
        GameEvents.Instance.OnPlayerAccept += OnPlayerAccepted;
        GameEvents.Instance.onPlayerBusted += OnPlayerBusted;
    }

    public void Disable()
    {
        GameEvents.Instance.OnTargetReachDragEnded -= tryCheckIfMissionLauncherAndShowPopUp;
        GameEvents.Instance.onMissionCompleted -= showMissionCompletedInfo;
        GameEvents.Instance.onMissionCompleted -= rewardPlayerForMissionCompletion;
        GameEvents.Instance.onMissionStarted -= enterMissionStateOnMissionStart;
        GameEvents.Instance.onMissionCompleted -= exitMissionStateOnMissionCompleted;
        GameEvents.Instance.onMissionFailed -= exitMissionStateOnMissionFailed;
        GameEvents.Instance.onMissionFailed -= showMissionFailedInfo;

        monoBehaviourHelper.OnUpdateEvent -= OnUpdate;
        GameEvents.Instance.OnTargetReachDragEnded -= OnCityTargetReached;
        GameEvents.Instance.OnPlayerAccept -= OnPlayerAccepted;
        GameEvents.Instance.onPlayerBusted -= OnPlayerBusted;

        GameObject.Destroy(monoBehaviourHelper.gameObject);
    }

    // Internal functions
    protected override void CleanupMissionObjectsImp(MissionRuntime mission)
    {
        if (!missionOwnedObjects.TryGetValue(mission, out var set))
            return;

        // Work on a snapshot to avoid modification-during-iteration issues
        var snapshot = new List<IDestroyable>(set);

        foreach (var obj in snapshot)
        {
            obj.Destroy();
        }

        // Now clear and drop the reference
        set.Clear();
        missionOwnedObjects.Remove(mission);
    }

    // Utility helpers functions

    void registerMissionObject(MissionRuntime mission, IDestroyable obj)
    {
        if (!missionOwnedObjects.TryGetValue(mission, out var set))
        {
            set = new HashSet<IDestroyable>();
            missionOwnedObjects[mission] = set;
        }
        set.Add(obj);
        obj.onBeingDestroyed += onDestroyableDestroyed;
    }

    // Internal Event Handlers Overrides
    protected override void onSpawnTargetMissionRequestEvent(
    SpawnTargetMissionRequestEvent request)
    {
        var target = new CityDestroyable();
        if(CityEntitiesCreationHelper.CreatePinnedMarkerReferencedTriggerInteractable(
            target,
            request.TargetMarker.GetCityPosition(),
            G.WorldMissionsConfig.finishPinStyle,
            request.TargetMarker) != null)
            registerMissionObject(request.Mission, target);
    }

    protected override void onSpawnMissionLauncherRequestEvent(SpawnMissionLauncherRequestEvent requestEvent)
    {
        var missionLauncher = new MissionLauncher(requestEvent.Mission, requestEvent.LauncherConfig);
        if(CityEntitiesCreationHelper.CreateMissionLauncher(missionLauncher) != null)
            registerMissionObject(requestEvent.Mission, missionLauncher);
    }
    protected override void onSpawnMoneyCollectablesRequestEvent(SpawnMoneyCollectablesRequestEvent requestEvent)
    {
        var markers = G.City.QueryMarkers("cash");
        var positions = markers.Select(a => a.PositionOnGraph.Value).ToArray();
        positions.Shuffle();
        for (int i = 0; i < requestEvent.count; i++)
        {
            var collectable = new Collectable(new CollectableConfig { MoneyAmount = requestEvent.reward });
            if(CityEntitiesCreationHelper.CreateTriggerInteractable(
                collectable,
                positions[i % positions.Length]) 
                != null)
            registerMissionObject(requestEvent.Mission, collectable);
        }
    }
    protected override void onPoliceRequestEvent(PoliceRequestEvent requestEvent)
    {
        var policeDuringMission = new PoliceMissionLifetimeWrapper();
        registerMissionObject(requestEvent.Mission, policeDuringMission);
    }
    class PoliceMissionLifetimeWrapper : IDestroyable
    {
        public event Action<IDestroyable> onBeingDestroyed;
        public PoliceMissionLifetimeWrapper()
        {
            G.PoliceManager.CreatePolice();
        }
        public void Destroy()
        {
            G.PoliceManager.ClearPolice();
            onBeingDestroyed?.Invoke(this);
        }

        public void NotifyDestroyed()
        {
            Debug.LogWarning("PoliceMissionWrapper: NotifyDestroyed called directly, which should not happen.");
        }
    }

    // External Event Handlers
    void tryCheckIfMissionLauncherAndShowPopUp(CityTargetReachedEventData evt)
    {
        if (evt.ReachedObject.Subject is MissionLauncher missionLauncher)
        {
            G.ContextMenuManager.CreateContextMenu(evt.TriggerContext.TriggerView, CTX_Menu_Tools.MissionLauncherTrigger(missionLauncher));
        }
    }

    void onDestroyableDestroyed(IDestroyable destroyable)
    {
        Debug.Assert(destroyable != null);
        foreach (var kvp in missionOwnedObjects)
        {
            if (kvp.Value.Remove(destroyable))
                break;
        }
        destroyable.onBeingDestroyed -= onDestroyableDestroyed;
    }

    void showMissionCompletedInfo(MissionCompletedEventData missionCompletedEvent)
    {
        G.FixedContextMenuManager.CreateContextMenu(CTX_Menu_Tools.MissionCompletedInfo(missionCompletedEvent.Mission));
    }

    void showMissionFailedInfo(MissionFailedEventData missionFailedEvent)
    {
        G.FixedContextMenuManager.CreateContextMenu(CTX_Menu_Tools.MissionFailedInfo(missionFailedEvent.Mission));
    }

    void rewardPlayerForMissionCompletion(MissionCompletedEventData e)
    {
        var rewards = e.Mission.Config.RewardBundles;

        foreach (var reward in rewards)
        {
            var result = G.TransactionProcessor.Process(reward.CreateTransaction());
            if (result.Type != TransactionResultType.Success)
            {
                Debug.LogError($"Failed to process mission reward transaction: {result.Type}");
            }
        }
    }

    #region GameState controlling
    void enterMissionStateOnMissionStart(MissionStartedEventData e)
    {
        MissionGameState missionGameState = new MissionGameState(e.Mission);
        G.GameFlowController.SetGameState(missionGameState);
    }
    void exitMissionStateOnMissionCompleted(MissionCompletedEventData e)
    {
        FreeRoamGameState missionGameState = new FreeRoamGameState();
        G.GameFlowController.SetGameState(missionGameState);
    }
    void exitMissionStateOnMissionFailed(MissionFailedEventData e)
    {
        FreeRoamGameState missionGameState = new FreeRoamGameState();
        G.GameFlowController.SetGameState(missionGameState);
    }
    #endregion
}

//potential problem: if mission target's CityEntity is destroyed outside of mission cleanup,
//mission controller won't be notified
public class MissionLauncher : CityDestroyable
{
    public readonly MissionLauncherConfig Config;
    public readonly MissionRuntime MissionRuntime;
    public MissionLauncher( MissionRuntime missionRuntime, MissionLauncherConfig launcherConfig)
    {
        this.MissionRuntime = missionRuntime;
        this.Config = launcherConfig;
    }
}

public class CityDestroyable : IDestroyable, ILocatable
{
    public event Action<IDestroyable> onBeingDestroyed;
    public void Destroy()
    {
        City.EntityLifetimeService.Destroy(this);
    }
    public void NotifyDestroyed()
    {
        onBeingDestroyed?.Invoke(this);
    }
}