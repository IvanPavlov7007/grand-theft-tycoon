using System;
using UnityEngine;

public struct CityObjectState
{
    public ViewObjectVisualState visualState;
    public bool draggable;
    public bool interactable;

    public static CityObjectState Default =>
        new CityObjectState(ViewObjectVisualState.Normal, true, true);

    public CityObjectState(ViewObjectVisualState visualState = ViewObjectVisualState.Normal, bool draggable = true, bool interactable = true)
    {
        this.visualState = visualState;
        this.draggable = draggable;
        this.interactable = interactable;
    }
}

public abstract class CitySceneProfile
{
    public abstract bool ShouldShow(CityEntity obj, GameState gameState);

    public virtual CityObjectState GetObjectViewState(CityEntity obj, GameState gameState)
    {
        return CityObjectState.Default;
    }

    public virtual void OnProfileActivated(GameState gameState) { }
    public virtual void OnProfileDeactivated() { }
}


public sealed class NormalCitySceneProfile : CitySceneProfile
{
    public override bool ShouldShow(CityEntity obj, GameState gameState)
    {
        return obj.IsValid();
    }

    public override CityObjectState GetObjectViewState(CityEntity obj, GameState gameState)
    {
        switch (obj.Subject)
        {
            // disable dragging for cars in normal profile
            case Car car:
                return new CityObjectState(ViewObjectVisualState.Normal, draggable: false);
        }

        return base.GetObjectViewState(obj, gameState);
    }
}

[Obsolete]
public sealed class StealingCitySceneProfile : CitySceneProfile
{
    public override bool ShouldShow(CityEntity obj, GameState gameState)
    {
        Debug.Assert(gameState is StealingGameState, "StealingCitySceneProfile: gameState is not StealingGameState");
        var stealingState = gameState as StealingGameState;


        //TODO add police
        switch (obj.Subject)
        {
            case Car car:
                return car == stealingState.StealingCar;
            case Warehouse warehouse:
                {
                    return true;
                }
            default:
                Debug.LogError($"StealingCitySceneProfile: Unsupported object type {obj.GetType()}");
                return false;
        }
    }

    public override CityObjectState GetObjectViewState(CityEntity obj, GameState gameState)
    {
        switch (obj.Subject)
        {
            case Warehouse warehouse:
                {
                    var stealingState = gameState as StealingGameState;
                    if (warehouse.AvailableCarParkingSpots > 0)
                        return CityObjectState.Default;
                    else
                        return new CityObjectState(ViewObjectVisualState.Disabled);
                }
            default:
                {
                    return base.GetObjectViewState(obj, gameState);
                }
        }
    }
}
[Obsolete]
public sealed class SellingCitySceneProfile : CitySceneProfile
{
    public override bool ShouldShow(CityEntity obj, GameState gameState)
    {
        Debug.Assert(gameState is SellingGameState, "SellingCitySceneProfile: gameState is not SellingGameState");
        var sellingState = gameState as SellingGameState;
        switch (obj.Subject)
        {
            case Car car:
                return car == sellingState.SellingCar;
            case Warehouse warehouse:
                {
                    return warehouse == sellingState.SellingWarehouse;
                }
            case Buyer buyer:
                return buyer == sellingState.Buyer;
            default:
                Debug.LogError($"SellingCitySceneProfile: Unsupported object type {obj.GetType()}");
                return false;
        }
    }
}

public sealed class FreeRoamCitySceneProfile : CitySceneProfile
{
    public override bool ShouldShow(CityEntity obj, GameState gameState)
    {
        return obj.IsValid();
    }

    public override CityObjectState GetObjectViewState(CityEntity obj, GameState gameState)
    {
        var controlledCar = G.VehicleController.CurrentCar;

        if (controlledCar != null && obj.Subject == controlledCar)
        {
            return new CityObjectState(ViewObjectVisualState.Selected, draggable: true, interactable: true);
        }

        // Cars are still interactable, but not draggable unless controlled
        if (obj.Subject is Car)
        {
            return new CityObjectState(ViewObjectVisualState.Normal, draggable: false, interactable: true);
        }

        return base.GetObjectViewState(obj, gameState);
    }
}

public sealed class MissionCitySceneProfile : CitySceneProfile
{
    public override bool ShouldShow(CityEntity obj, GameState gameState)
    {
        if(!obj.IsValid())
            return false;
        var missionState = gameState as MissionGameState;
        switch (obj.Subject)
        {
            case Warehouse:
                    return false;
            case MissionLauncher:
                return false;
            default:
                return true;
        }
    }
    public override CityObjectState GetObjectViewState(CityEntity obj, GameState gameState)
    {
        var missionState = gameState as MissionGameState;

        if (obj.Subject is Car car)
        {

            bool isFocusedCar = car == G.VehicleController.CurrentCar;
            ViewObjectVisualState visualState = isFocusedCar ? ViewObjectVisualState.Selected : ViewObjectVisualState.Normal;
            return new CityObjectState(visualState, draggable: isFocusedCar, interactable: true);
        }

        return base.GetObjectViewState(obj, gameState);
    }
}