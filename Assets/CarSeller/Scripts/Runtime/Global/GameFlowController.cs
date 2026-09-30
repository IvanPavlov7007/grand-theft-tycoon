using System;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Methods to control the game flow, holding the current game state
/// </summary>
public class GameFlowController
{
    public GameState GameState { get; private set; } = new NeutralGameState();
    public GameSceneType CurrentSceneType { get; private set; }
    public Warehouse CurrentWarehouse { get; private set; }
    [Serializable]
    public enum GameSceneType
    {
        None,
        City,
        Warehouse
    }

    #region GameState
    public void SetGameState(GameState newState)
    {
        Debug.Assert(newState != null, "GameState cannot be set to null");
        var oldState = GameState;
        GameState = newState;
        GameEvents.Instance.OnGameStateChanged?.Invoke(new GameStateChangeEventData(oldState, newState));
    }
    #endregion

    public void EnterWarehouse(Warehouse warehouse)
    {
        if (CurrentSceneType == GameSceneType.Warehouse && CurrentWarehouse == warehouse)
            return;
        SceneManager.LoadScene(warehouse.Config.Name);
    }

    public void EnterCity()
    {
        if (CurrentSceneType == GameSceneType.City)
            return;
        SceneManager.LoadScene(World.Instance.City.Config.SceneToLoad);
    }

    public void SetWarehouse(Warehouse warehouse)
    {
        CurrentSceneType = GameSceneType.Warehouse;
        CurrentWarehouse = warehouse;
        G.InteractionManager = new WarehouseInteractionManager();
    }

    public void SetCity()
    {
        CurrentSceneType = GameSceneType.City;
        G.InteractionManager = new CityInteractionManager();
    }
}

public readonly struct CarActionResolution
{
    public bool Allowed { get; }
    public string Reason { get; }
    public GameState NextState { get; }

    public CarActionResolution(bool allowed, GameState nextState, string reason)
    {
        Allowed = allowed;
        NextState = nextState;
        Reason = reason;
    }
}