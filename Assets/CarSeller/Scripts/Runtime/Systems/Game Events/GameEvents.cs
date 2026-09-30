using System;

public class GameEvents
{
    public static GameEvents Instance = new GameEvents();

    public void Reset()
    {
        OnGamePaused = null;
        OnGameUnpaused = null;
        OnProductLocationChanged = null;
        OnProductCreated = null;
        OnProductDestroyed = null;
        OnGameStateChanged = null;

        OnPlayerCancel = null;
        OnPlayerCaught = null;
        OnPlayerSucceed = null;
        OnPlayerAccept = null;

        OnLocatableStateChanged = null;
        OnLocatableRegistered = null;
        OnLocatableDestroyed = null;
        OnLocatableLocationChanged = null;

        OnOwnershipChanged = null;

        OnTargetReached = null;
        OnPlayerMoneyChanged = null;
        OnTransactionComplete = null;

        onMissionUnlocked = null;
        onMissionStarted = null;
        onMissionCompleted = null;
        onMissionFailed = null;

        onPlayerBusted = null;

        onVehicleControlStateChanged = null;

        onAreaProgressed = null;
        onAreaLevelUp = null;

        OnCityEntityDestroyed = null;

        Instance = new GameEvents();
    }

    public Action OnGamePaused;
    public Action OnGameUnpaused;

    public Action<GameStateChangeEventData> OnGameStateChanged;

    public Action<ProductLocationChangedEventData> OnProductLocationChanged;
    public Action<ProductCreatedEventData> OnProductCreated;
    public Action<ProductDestroyedEventData> OnProductDestroyed;

    public Action<LocatableStateChangedEventData> OnLocatableStateChanged;
    public Action<LocatableCreatedEventData> OnLocatableRegistered;
    public Action<LocatableDestroyedEventData> OnLocatableDestroyed;
    public Action<LocatableLocationChangedEventData> OnLocatableLocationChanged;

    public Action<OwnershipChangedEventData> OnOwnershipChanged;

    public Action<CityTargetReachedEventData> OnTargetReachDragEnded;
    public Action<CityTargetReachedEventData> OnTargetReached;
    public Action<PlayerMoneyChangeEventData> OnPlayerMoneyChanged;

    public Action<TransactionEventData> OnTransactionComplete;

    // Game Flow Events
    public Action<PlayerActionEventData> OnPlayerCancel;
    public Action<PlayerActionEventData> OnPlayerCaught;
    public Action<PlayerActionEventData> OnPlayerSucceed;
    public Action<PlayerAcceptedEventData> OnPlayerAccept;

    // Mission Events from MissionController
    public Action<MissionUnlockedEventData> onMissionUnlocked;
    public Action<MissionStartedEventData> onMissionStarted;
    public Action<MissionCompletedEventData> onMissionCompleted;
    public Action<MissionFailedEventData> onMissionFailed;

    public Action<PlayerBustedEventData> onPlayerBusted;

    // Player Interaction Events
    public Action<VehicleControlStateChangedEventData> onVehicleControlStateChanged;

    public Action<AreaProgressEventData> onAreaProgressed;
    public Action<AreaLevelUpEventData> onAreaLevelUp;

    public Action<CityEntityDestroyedEventData> OnCityEntityDestroyed;
}

public class CityEntityDestroyedEventData : GameEventData
{
    public readonly CityEntity DestroyedEntity;
    public CityEntityDestroyedEventData(CityEntity destroyedEntity)
    {
        DestroyedEntity = destroyedEntity;
    }
}

public class AreaProgressEventData : GameEventData
{
    public readonly CityArea Area;
    public readonly float InitialXP;
    public readonly float NewXP;

    public readonly AreaLevel InitialLevel;
    public readonly AreaLevel NewLevel;
    public AreaProgressEventData(CityArea area, float initialXP, float newXP, AreaLevel initialLevel, AreaLevel newLevel)
    {
        Area = area;
        InitialXP = initialXP;
        NewXP = newXP;
        InitialLevel = initialLevel;
        NewLevel = newLevel;
    }
}

public class AreaLevelUpEventData : GameEventData
{
    public readonly CityArea Area;
    public readonly int NewLevel;
    public readonly bool IsMaxLevel;
    public AreaLevelUpEventData(CityArea area, int newLevel, bool isMaxLevel)
    {
        Area = area;
        NewLevel = newLevel;
        IsMaxLevel = isMaxLevel;
    }
}