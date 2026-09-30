using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public sealed class CityVisionCentersSystem : AspectSystem<VisionCenterAspect>
{

    public IReadOnlyList<VisionCenterAspect> VisionCenters => aspects.Values.ToList().AsReadOnly();

    public event Action<CityEntity, VisionCenterAspect> OnCenterAdded;
    public event Action<CityEntity, VisionCenterAspect> OnCenterRemoved;


    // Current controlled center
    private CityEntity currentActorEntity;
    // Additional vision centers granted by other rules (e.g., ownership)
    private readonly Dictionary<ILocatable, CityEntity> ownedCenterEntities = new();
    // Default configs (kept internal; can later be moved to config asset)
    private readonly VisionCenterConfig defaultActorVision = new VisionCenterConfig { Radius = 4f};
    private readonly VisionCenterConfig defaultWarehouseVision = new VisionCenterConfig { Radius = 1f};

    public CityVisionCentersSystem(CityEntityAspectsService aspectsService) : base(aspectsService)
    {
        Initialize();
    }

    public bool TryGetNearestCenter(Vector2 worldPos, out VisionCenterAspect nearestCenter, out float distance)
    {
        nearestCenter = null;
        distance = float.PositiveInfinity;

        if (aspects.Count == 0) return false;
        foreach (var center in aspects.Values)
        {
            float curDist = (center.Entity.Position.WorldPosition - worldPos).magnitude;
            if (curDist < distance)
            {
                distance = curDist;
                nearestCenter = center;
            }
        }

        return nearestCenter != null;
    }

    public void Initialize()
    {

        Debug.Assert(GameEvents.Instance != null, "CityVision.Initialize: GameEvents.Instance is null");

        // React to global game/domain changes.
        //GameEvents.Instance.OnOwnershipChanged += OnOwnershipChanged;
        GameEvents.Instance.onVehicleControlStateChanged += OnVehicleControlStateChanged;

        // Apply initial state once.
        //ApplyActorFromVehicleState(G.VehicleController.CurrentState);
    }

    private void OnVehicleControlStateChanged(VehicleControlStateChangedEventData data)
    {
        ApplyActorFromVehicleState(data.NewState);
    }

    public override void Dispose()
    {
        Unsubscribe();
        base.Dispose();
    }

    public void Unsubscribe()
    {
        if (GameEvents.Instance != null)
        {
            //GameEvents.Instance.OnOwnershipChanged -= OnOwnershipChanged;
            GameEvents.Instance.onVehicleControlStateChanged -= OnVehicleControlStateChanged;
        }
        // Do not automatically remove aspects on shutdown; keep state as-is.
        currentActorEntity = null;
        ownedCenterEntities.Clear();
    }

    protected override void OnAspectAdded(CityEntity entity, VisionCenterAspect aspect)
    {
        OnCenterAdded?.Invoke(entity, aspect);
    }


    protected override void OnAspectRemoved(CityEntity entity, VisionCenterAspect aspect)
    {
        OnCenterRemoved?.Invoke(entity, aspect);
    }

    private void OnOwnershipChanged(OwnershipChangedEventData data)
    {
        if (data == null) return;

        // Only care about city-locatable items.
        if (data.Item is not ILocatable locatable)
            return;
        if (data.NewOwner == G.Player)
        {
            TryAddOwnedWarehouseCenter(locatable);
        }
        else if (ownedCenterEntities.TryGetValue(locatable, out var entity))
        {
            // If it was previously granted via warehouse ownership but ownership changed away -> remove.
            ownedCenterEntities.Remove(locatable);
            if (entity != null)
                entity.TryRemoveAspect<VisionCenterAspect>();
        }
    }

    private void TryAddOwnedWarehouseCenter(ILocatable locatable)
    {
        if (locatable == null) return;
        if (ownedCenterEntities.ContainsKey(locatable)) return;

        if (G.City == null)
        {
            Debug.LogWarning("CityVision: G.City is null; cannot add warehouse-owned center");
            return;
        }

        if (!G.City.TryGetEntity(locatable, out var entity) || entity == null)
        {
            Debug.LogWarning($"CityVision: Owned locatable {locatable} has no CityEntity (not in city?)");
            return;
        }

        if (entity.Aspects.OfType<VisionCenterAspect>().Any())
        {
            ownedCenterEntities[locatable] = entity;
            return;
        }

        bool ok = entity.TryAddAspect(new VisionCenterAspect(defaultWarehouseVision));
        if (!ok)
        {
            Debug.LogWarning($"CityVision: Failed to add VisionCenterAspect for warehouse-owned {locatable}");
            return;
        }

        ownedCenterEntities[locatable] = entity;
    }

    private void ApplyActorFromVehicleState(VehicleController.VehicleControlState state)
    {
        // Remove prior center if any.
        ClearActorCenter();

        CityEntity nextActorEntity = state.CurrentCityEntity;

        // Add new center.
        if (!nextActorEntity.TryAddAspect(new VisionCenterAspect(defaultActorVision)))
        {
            Debug.LogWarning($"CityVision: Failed to add VisionCenterAspect on controlled actor {nextActorEntity.Subject}");
            currentActorEntity = null;
            return;
        }
        currentActorEntity = nextActorEntity;
    }

    private void ClearActorCenter()
    {
        if (currentActorEntity != null)
        {
            currentActorEntity.TryRemoveAspect<VisionCenterAspect>();
        }

        currentActorEntity = null;
    }
}
