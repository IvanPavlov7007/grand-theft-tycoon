using System;
using UnityEngine;

public interface ICityContextMenuProfile
{
    /// <summary>
    /// Build context menu for a clicked city model in a given game state.
    /// </summary>
    Widget GenerateContent(CityEntity model, GameState gameState);
}

public interface ICityTriggerProfile
{
    /// <summary>
    /// Decide if a trigger interaction can proceed and, if so, what it should do.
    /// </summary>
    TriggerAction GenerateTriggerAction(
        TriggerContext triggerContext);
}

/// <summary>
/// Registry for context menu profiles per GameState type.
/// </summary>
public sealed class CityContextMenuProfileRegistry
{
    private readonly System.Collections.Generic.Dictionary<Type, ICityContextMenuProfile> _profiles;

    public CityContextMenuProfileRegistry()
    {
        _profiles = new()
        {
            { typeof(NeutralGameState),  new NormalCityContextMenuProfile()  },
            { typeof(StealingGameState), new StealingCityContextMenuProfile() },
            { typeof(SellingGameState),  new SellingCityContextMenuProfile()  },
            { typeof(FreeRoamGameState), new FreeRoamCityContextMenuProfile() },
            { typeof(MissionGameState),  new MissionCityContextMenuProfile()  },
        };
    }

    public ICityContextMenuProfile Get(GameState state)
    {
        if (state == null)
        {
            Debug.LogError("CityContextMenuProfileRegistry.Get: state is null");
            return null;
        }

        var type = state.GetType();
        if (_profiles.TryGetValue(type, out var profile))
        {
            return profile;
        }

        Debug.LogError($"CityContextMenuProfileRegistry.Get: No profile registered for state type {type.Name}");
        return null;
    }
}

/// <summary>
/// Registry for trigger profiles per GameState type.
/// </summary>
public sealed class CityTriggerProfileRegistry
{
    private readonly System.Collections.Generic.Dictionary<Type, ICityTriggerProfile> _profiles;

    public CityTriggerProfileRegistry()
    {
        _profiles = new()
        {
            { typeof(NeutralGameState),  new NormalCityTriggerProfile()  },
            { typeof(StealingGameState), new StealingCityTriggerProfile() },
            { typeof(SellingGameState),  new SellingCityTriggerProfile()  },
            { typeof(FreeRoamGameState), new FreeRoamCityTriggerProfile() },
            { typeof(MissionGameState),  new MissionCityTriggerProfile()  },
        };
    }

    public ICityTriggerProfile Get(GameState state)
    {
        if (state == null)
        {
            Debug.LogError("CityTriggerProfileRegistry.Get: state is null");
            return null;
        }

        var type = state.GetType();
        if (_profiles.TryGetValue(type, out var profile))
        {
            return profile;
        }

        Debug.LogError($"CityTriggerProfileRegistry.Get: No profile registered for state type {type.Name}");
        return null;
    }
}

public class TriggerContext
{
    public enum TriggerKind
    {
        Enter,
        DragEnd
    }

    public CityEntity Trigger { get; private set; }
    public CityEntity TriggerCause { get; private set; }
    public GameObject TriggerView { get; set; }
    public GameObject TriggerCauseView { get; set; }
    public GameState GameState { get; set; }
    public TriggerKind Kind { get; private set; }

    public TriggerContext(CityEntity trigger, CityEntity triggerCause, GameState gameState, GameObject triggerView, GameObject triggerCauseView, TriggerKind kind = TriggerKind.Enter)
    {
        Trigger = trigger;
        TriggerCause = triggerCause;
        GameState = gameState;
        TriggerView = triggerView;
        TriggerCauseView = triggerCauseView;
        Kind = kind;
    }
}

/// <summary>
/// Result of evaluating a trigger in the city.
/// </summary>
public class TriggerAction : IInteractionContent
{
    public TriggerAction()
    {
        CanProceed = false;
        Action = null;
    }

    public TriggerAction(bool canProceed, Action action)
    {
        CanProceed = canProceed;
        Action = action;
    }

    public bool CanProceed { get; private set; }
    public Action Action { get; private set; }
}
