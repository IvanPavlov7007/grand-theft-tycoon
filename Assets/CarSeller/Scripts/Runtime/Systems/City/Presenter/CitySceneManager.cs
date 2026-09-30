using System;
using System.Collections.Generic;
using UnityEngine;

public class CitySceneManager : GlobalSingletonBehaviour<CitySceneManager>
{
    protected override CitySceneManager GlobalInstance { get => G.CitySceneManager; set => G.CitySceneManager = value; }

    public City City => World.Instance.City;

    CitySceneProfileRegistry profileRegistry = new CitySceneProfileRegistry();
    CitySceneProfile currentProfile;

    Dictionary<CityEntity, CityViewObjectController> builtObjectsViews = new Dictionary<CityEntity, CityViewObjectController>();

    private AspectsViewBuilder _aspectsViewBuilder;

    public void SetCurrentProfile(GameState state)
    {
        currentProfile = profileRegistry.Get(state);
    }

    private void Bind()
    {
        GameEvents.Instance.OnLocatableRegistered += onNewLocatableCreated;
        GameEvents.Instance.OnLocatableLocationChanged += onLocatableLocationChanged;
        GameEvents.Instance.OnLocatableDestroyed += onLocatableDestroyed;
        GameEvents.Instance.OnOwnershipChanged += onOwnershipChanged;
        GameEvents.Instance.onVehicleControlStateChanged += onVehicleControlStateChanged;

        GameEvents.Instance.OnGameStateChanged += onGameStateChanged;

        GameEvents.Instance.OnCityEntityDestroyed += onCityEntityDestroyed;
    }

    private void OnDisable()
    {
        GameEvents.Instance.OnLocatableRegistered -= onNewLocatableCreated;
        GameEvents.Instance.OnLocatableLocationChanged -= onLocatableLocationChanged;
        GameEvents.Instance.OnLocatableDestroyed -= onLocatableDestroyed;
        GameEvents.Instance.OnOwnershipChanged -= onOwnershipChanged;
        GameEvents.Instance.onVehicleControlStateChanged -= onVehicleControlStateChanged;

        GameEvents.Instance.OnGameStateChanged -= onGameStateChanged;

        GameEvents.Instance.OnCityEntityDestroyed -= onCityEntityDestroyed;
    }

    private void onCityEntityDestroyed(CityEntityDestroyedEventData data)
    {
        if (data?.DestroyedEntity == null)
            return;

        clearView(data.DestroyedEntity);
    }

    public CityViewObjectController getExiting(CityEntity entity)
    {
        if (builtObjectsViews.TryGetValue(entity, out var view))
        {
            return view;
        }
        return null;
    }

    public void InitializeCity()
    {
        if (City == null)
        {
            Debug.LogWarning("City instance is not set");
            return;
        }

        // initializeMap();

        Bind();

        rebuildSceneForState(G.GameState);
    }

    private void initializeMap()
    {
        G.cityViewStreetsBuilder.BuildStreets(City);
    }

    private void registerNewView(CityEntity entity, CityViewObjectController view)
    {
        builtObjectsViews[entity] = view;
    }

    private void clearView(CityEntity entity)
    {
        if (builtObjectsViews.TryGetValue(entity, out var view))
        {
            Destroy(view.gameObject);
            builtObjectsViews.Remove(entity);
        }
    }

    private void clearViews()
    {
        foreach (var view in builtObjectsViews.Values)
        {
            Destroy(view.gameObject);
        }
        builtObjectsViews.Clear();
    }

    private void onVehicleControlStateChanged(VehicleControlStateChangedEventData data)
    {
        var oldCar = data.OldState?.CurrentCityEntity;
        var newCar = data.NewState?.CurrentCityEntity;
        applyProfileToObject(oldCar);
        applyProfileToObject(newCar);
    }

    private void onLocatableLocationChanged(LocatableLocationChangedEventData data)
    {
        Debug.Assert(data.Locatable != null, "Locatable cannot be null in location change event data");
        if(data.OldLocation == null)
        {
            Debug.LogWarning("Old location is null in location change event data. This may indicate an issue with the locatable's location management.");
        }

        

        if (data.OldLocation?.Holder == City)
        {
            //Stupid design, but its ok to read the entity from the city even if the locatable is not there anymore, 
            //because the entity is not destroyed until after this event is processed,
            //so the view can still be cleared based on the entity data.
            if (G.City.TryGetEntity(data.Locatable, out var entity))
            {
                clearView(entity);
                return;
            }

        }

        if (data.NewLocation?.Holder == City)
        {
            if (G.City.TryGetEntity(data.Locatable, out var entity))
            {
                applyProfileToObject(entity);
                return;
            }
        }
    }

    private void onNewLocatableCreated(LocatableCreatedEventData data)
    {
        if (data.Location.Holder == City)
        {
            if (G.City.TryGetEntity(data.Locatable, out var entity))
            {
                applyProfileToObject(entity);
            }
        }
    }

    private void onLocatableDestroyed(LocatableDestroyedEventData data)
    {
        if (G.City.TryGetEntity(data.Locatable, out var entity))
        {
            clearView(entity);
        }
    }

    private void onGameStateChanged(GameStateChangeEventData data)
    {
        Debug.Assert(data.newState != null, "New game state cannot be null");
        currentProfile?.OnProfileDeactivated();
        currentProfile = profileRegistry.Get(data.newState);
        currentProfile.OnProfileActivated(data.newState);
        rebuildSceneForState(data.newState);
    }

    private void onOwnershipChanged(OwnershipChangedEventData data)
    {
        if (data.Item is not ILocatable locatable)
            return;

        if ( City.TryGetEntity(locatable, out var entity))
        {
            applyProfileToObject(entity);
        }
    }

    private void rebuildSceneForState(GameState state)
    {
        Debug.Log($"Rebuilding city scene for game state {state.GetType().Name} with profile {currentProfile.GetType().Name}");
        // rebuild views based on the current profile
        foreach (var entity in City.GetEntities().Values)
        {
            Debug.Log($"Rebuilding view for entity {entity} of type {entity.Subject} in state {state.GetType().Name}");
            applyProfileToObject(entity, state);
        }
    }

    private void applyProfileToObject(CityEntity entity, GameState state)
    {
        Debug.Assert(currentProfile != null);
        Debug.Assert(state != null);
        Debug.Assert(entity != null);

        // if rebuilding only partial views, check if the view exists
        if (!currentProfile.ShouldShow(entity, state))
        {
            clearView(entity);
            return;
        }

        var visualState = currentProfile.GetObjectViewState(entity, state);

        if (builtObjectsViews.TryGetValue(entity, out var existingView))
        {
            // update existing view
            existingView.SetViewState(visualState);
            return;
        }
        else
        {

            CityViewObjectController view = G.cityViewObjectBuilder.BuildObject(entity);
            view.SetViewState(visualState);
            registerNewView(entity, view);
        }
    }

    private void applyProfileToObject(CityEntity entity)
    {
        applyProfileToObject(entity, G.GameState);
    }
}

public sealed class CitySceneProfileRegistry
{
    private readonly Dictionary<Type, CitySceneProfile> _profiles;

    public CitySceneProfileRegistry()
    {
        _profiles = new()
        {
            { typeof(NeutralGameState), new NormalCitySceneProfile() },
            { typeof(StealingGameState), new StealingCitySceneProfile() },
            { typeof(SellingGameState), new SellingCitySceneProfile() },
            { typeof(FreeRoamGameState), new FreeRoamCitySceneProfile() },
            { typeof(MissionGameState), new MissionCitySceneProfile() }
        };
    }

    public CitySceneProfile Get(GameState state)
        => _profiles[state.GetType()];
}
