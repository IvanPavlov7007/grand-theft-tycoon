using System.Collections.Generic;
using UnityEngine;

public class PoliceManager : GlobalSingletonBehaviour<PoliceManager>
{
    protected override PoliceManager GlobalInstance { get => G.PoliceManager; set => G.PoliceManager = value; }

    PoliceAISystem aiSystem = new PoliceAISystem();
    PoliceAIContext stateMachine;

    List<PoliceUnit> policeUnits = new List<PoliceUnit>();
    public SpotlightColors SpotlightColors;
    public SliceVisionSettings SliceVisionSettings;

    public SpeedVarations policeSpeedVariations;

    public float stopDistanceToSuspect = 0.3f;

    bool active = false;

    CityPosition SuspectPosition
    {
        get
        {
            if (G.GameState == null)
            {
                Debug.LogWarning("GameState is null in PoliceManager.SuspectPosition");
                return default;
            }
            var location = G.VehicleController.CurrentVehicleEntity;
            if (location == null)
            {
                Debug.LogWarning("FocusedCar location is null in PoliceManager.SuspectPosition");
                return default;
            }
            var pos = location.Position;
            Debug.Assert(pos.Edge != null || pos.Node != null, $"SuspectPosition is invalid: edge and node are null");
            return location.Position;
        }
    }

    

    public void CreatePolice()
    {
        active = true;

        foreach (var marker in G.City.QueryMarkers("cop"))
        {
            CreatePoliceEntity(marker);
        }

        stateMachine = new PoliceAIContext(policeUnits.ToArray());
    }


    public void ClearPolice()
    {
        active = false;
        foreach (var unit in policeUnits)
        {
            unit.Destroy();
        }
        policeUnits.Clear();
    }

    private CityEntity CreatePoliceEntity(City.CityMarker marker)
    {
        if (!tryGetMarkerPosition(marker, out var position))
            return null;

        var unit = CreateUnit();

        if (!tryCreatePoliceEntity(marker, unit, position, out var entity))
            return null;

        registerUnit(unit, entity);
        return entity;
    }
    private bool tryGetMarkerPosition(City.CityMarker marker, out CityPosition position)
    {
        var markerPosition = marker.PositionOnGraph;
        if (markerPosition == null)
        {
            Debug.LogWarning($"Police marker {marker.Id} has no position on graph, skipping");
            position = default;
            return false;
        }

        position = markerPosition.Value;
        return true;
    }
    private bool tryCreatePoliceEntity(City.CityMarker marker, PoliceUnit unit, CityPosition position, out CityEntity entity)
    {
        entity = CityEntitiesCreationHelper.CreatePoliceUnit(unit, position);
        if (entity == null)
        {
            Debug.LogWarning($"Failed to create police unit entity for marker {marker.Id}, skipping");
            return false;
        }

        return true;
    }
    private void registerUnit(PoliceUnit unit, CityEntity entity)
    {
        unit.Initialize(entity);
        policeUnits.Add(unit);
    }
    private PoliceUnit CreateUnit()
    {
        return new PoliceUnit(
            policeSpeedVariations,
            SliceVisionSettings,
            PersonalityTag.Blinky);
    }

    private void Update()
    {
        if (!active) return;
        updateStateMachineData();
        aiSystem.UpdateState(stateMachine);
        updateUnits(Time.deltaTime);
    }

    void updateStateMachineData()
    {
        stateMachine.StopDistance = stopDistanceToSuspect;
        stateMachine.SuspectRealPosition = SuspectPosition;
    }

    void updateUnits(float deltaTime)
    {
        // actually each unit updates itself as well
        foreach (var unit in policeUnits)
        {
            unit.Update(deltaTime, aiSystem, stateMachine);
        }
    }


    private void OnEnable()
    {
        GameEvents.Instance.OnTargetReached += onTriggerReached;
    }

    private void OnDisable()
    {
        GameEvents.Instance.OnTargetReached -= onTriggerReached;
    }

    void onTriggerReached(CityTargetReachedEventData data)
    {
        if(data.ReachedObject.Subject is PoliceUnit policeUnit)
        {
            Debug.Log("PoliceManager: Player busted by police!");
            Debug.Assert(GameEvents.Instance.onPlayerBusted != null);
            GameEvents.Instance.onPlayerBusted(new PlayerBustedEventData());
        }
    }
}