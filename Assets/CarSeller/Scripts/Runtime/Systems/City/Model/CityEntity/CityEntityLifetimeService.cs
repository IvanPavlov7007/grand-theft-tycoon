using System.Collections.Generic;
using UnityEngine;

public class CityEntityLifetimeService
{
    public bool TryCreate(ILocatable subject, CityPosition position, out CityEntity entity, params CityEntityAspect[] aspects)
    {
        Debug.Assert(subject != null);

        if (G.City.TryGetEntity(subject, out entity))
        {
            Debug.LogError($"City-only locatable {subject} is already in city.");
            return false;
        }

        entity = new CityEntity(G.City, subject, position, aspects);
        G.City.Entities[subject] = entity;
        GameEvents.Instance.OnLocatableRegistered?.Invoke(new LocatableCreatedEventData(subject, entity));

        return true;
    }

    public bool TryMoveToCity(Product product, CityPosition position, out CityEntity entity, params CityEntityAspect[] aspects)
    {
        Debug.Assert(product != null);

        if (G.City.TryGetEntity(product, out entity))
        {
            Debug.LogError($"City-only locatable {product} is already in city.");
            entity = G.City.Entities[product];
            return false;
        }

        entity = new CityEntity(G.City, position, aspects);
        G.City.Entities[product] = entity;
        return G.ProductLifetimeService.MoveProduct(product, entity);
    }

    private readonly HashSet<ILocatable> _destroyInProgress = new HashSet<ILocatable>();

    public void Destroy(CityEntity entity)
    {
        if (entity == null)
            return;
        Destroy(entity.Subject);
    }

    public void Destroy(ILocatable anyLocatable)
    {
        if (anyLocatable == null)
            return;

        // Prevent double-destroy / re-entrancy (e.g., onBeingDestroyed handler calls Destroy again)
        if (_destroyInProgress.Contains(anyLocatable))
            return;

        _destroyInProgress.Add(anyLocatable);
        try
        {
            // Notify internal destroy trackers no matter who initiated the destruction.
            if (anyLocatable is IDestroyTracker tracker)
            {
                tracker.NotifyDestroyed();
            }

            // IMPORTANT: emit CityEntityDestroyed while we can still resolve the entity (before detach / subject null).
            if (G.City != null && G.City.TryGetEntity(anyLocatable, out var cityEntity) && cityEntity != null)
            {
                GameEvents.Instance.OnCityEntityDestroyed?.Invoke(new CityEntityDestroyedEventData(cityEntity));
            }

            switch (anyLocatable)
            {
                case Product p:
                    // DestroyProduct() will detach from its current ILocation.
                    // If that location is a CityEntity, it already removes the entity from G.City.Entities.
                    G.ProductLifetimeService.DestroyProduct(p);

                    // Best-effort cleanup if something left a stale mapping behind.
                    TryDetachFromCity(p);
                    break;

                default:
                    // Calling this first because some destroy trackers might rely on the entity still being in the city (e.g., for position info)
                    GameEvents.Instance.OnLocatableDestroyed?.Invoke(new LocatableDestroyedEventData(anyLocatable));

                    if (!TryDetachFromCity(anyLocatable))
                        Debug.LogError($"Failed to detach city entity for locatable {anyLocatable}");
                    break;
            }
        }
        finally
        {
            _destroyInProgress.Remove(anyLocatable);
        }
    }

    private bool TryDetachFromCity(ILocatable locatable)
    {
        Debug.Assert(locatable != null);
        if (locatable == null)
            return false;

        if (!G.City.TryGetEntity(locatable, out var entity) || entity == null)
            return false;

        entity.Detach();
        return true;
    }
}
