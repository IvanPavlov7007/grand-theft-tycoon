using System;
using System.Collections.Generic;
using UnityEngine;

public interface IWarehouseContextMenuProfile
{
    /// <summary>
    /// Build context menu UI for a warehouse model in a given game state.
    /// </summary>
    UIElement GenerateContent(object model, GameState gameState);
}

public sealed class WarehouseContextMenuProfileRegistry
{
    private readonly Dictionary<Type, IWarehouseContextMenuProfile> _profiles;

    public WarehouseContextMenuProfileRegistry()
    {
        _profiles = new()
        {
            { typeof(NeutralGameState),  new NeutralWarehouseContextMenuProfile() },
            // You can add more later if needed:
            // { typeof(StealingGameState), new StealingWarehouseContextMenuProfile() },
            // { typeof(SellingGameState),  new SellingWarehouseContextMenuProfile() },
            { typeof(FreeRoamGameState), new FreeRoamWarehouseContextMenuProfile() },
        };
    }

    public IWarehouseContextMenuProfile Get(GameState state)
    {
        if (state == null)
        {
            Debug.LogError("WarehouseContextMenuProfileRegistry.Get: state is null");
            return null;
        }

        var type = state.GetType();
        if (_profiles.TryGetValue(type, out var profile))
        {
            return profile;
        }

        Debug.LogWarning($"WarehouseContextMenuProfileRegistry.Get: No profile registered for state type {type.Name}");
        return null;
    }
}