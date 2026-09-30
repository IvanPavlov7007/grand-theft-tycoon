using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public sealed class CityEntityAspectsService
{
    private readonly Dictionary<Type, List<Delegate>> _added = new();
    private readonly Dictionary<Type, List<Delegate>> _removed = new();

    public void SubscribeAdded<TAspect>(Action<CityEntity, TAspect> listener) where TAspect : class, CityEntityAspect
    {
        if (listener == null) throw new ArgumentNullException(nameof(listener));
        var t = typeof(TAspect);
        if (!_added.TryGetValue(t, out var list))
        {
            list = new List<Delegate>();
            _added[t] = list;
        }
        if (!list.Contains(listener)) list.Add(listener);
    }

    public void UnsubscribeAdded<TAspect>(Action<CityEntity, TAspect> listener) where TAspect : class, CityEntityAspect
    {
        if (listener == null) throw new ArgumentNullException(nameof(listener));
        var t = typeof(TAspect);
        if (_added.TryGetValue(t, out var list))
            list.Remove(listener);
    }

    public void SubscribeRemoved<TAspect>(Action<CityEntity, TAspect> listener) where TAspect : class, CityEntityAspect
    {
        if (listener == null) throw new ArgumentNullException(nameof(listener));
        var t = typeof(TAspect);
        if (!_removed.TryGetValue(t, out var list))
        {
            list = new List<Delegate>();
            _removed[t] = list;
        }
        if (!list.Contains(listener)) list.Add(listener);
    }

    public void UnsubscribeRemoved<TAspect>(Action<CityEntity, TAspect> listener) where TAspect : class, CityEntityAspect
    {
        if (listener == null) throw new ArgumentNullException(nameof(listener));
        var t = typeof(TAspect);
        if (_removed.TryGetValue(t, out var list))
            list.Remove(listener);
    }

    public bool HasAspect<T>(CityEntity entity) where T : CityEntityAspect
        => entity != null && entity.Aspects != null && entity.Aspects.OfType<T>().Any();

    public bool TryAddAspects(CityEntity entity, IEnumerable<CityEntityAspect> aspects, bool allowDuplicateType = false)
    {
        Debug.Assert(entity != null, "CityEntityAspectsService.TryAddAspects: entity is null");
        Debug.Assert(aspects != null, "CityEntityAspectsService.TryAddAspects: aspects is null");
        if (entity == null || aspects == null) return false;
        bool allAdded = true;
        foreach (var aspect in aspects)
        {
            if (!TryAddAspect(entity, aspect, allowDuplicateType))
                allAdded = false;
        }
        return allAdded;
    }

    public bool TryAddAspect(CityEntity entity, CityEntityAspect aspect, bool allowDuplicateType = false)
    {
        Debug.Assert(entity != null, "CityEntityAspectsService.TryAddAspect: entity is null");
        Debug.Assert(aspect != null, "CityEntityAspectsService.TryAddAspect: aspect is null");
        if (entity == null || aspect == null) return false;

        if (!allowDuplicateType)
        {
            var type = aspect.GetType();
            if (entity.Aspects.Any(a => a != null && a.GetType() == type))
            {
                Debug.LogWarning($"CityEntityAspectsService: Attempted to add duplicate aspect type {type.Name} to entity {entity.Subject}");
                return false;
            }
        }

        if (!entity.TryAddAspectInternal(aspect))
        {
            Debug.LogWarning($"CityEntityAspectsService: Failed to add aspect {aspect.GetType().Name} to entity {entity.Subject}");
            return false;
        }

        if (aspect is CityEntityAspectBase b)
            b.InternalBindToEntity(entity);

        EmitAdded(entity, aspect);
        return true;
    }

    public void TryRemoveAllAspects(CityEntity cityEntity)
    {
        Debug.Assert(cityEntity != null, "CityEntityAspectsService.TryRemoveAllAspects: cityEntity is null");
        if (cityEntity == null) return;

        // Snapshot to avoid mutating the underlying HashSet while iterating.
        var snapshot = cityEntity.Aspects.ToArray();
        for (int i = 0; i < snapshot.Length; i++)
        {
            TryRemoveAspect(cityEntity, snapshot[i]);
        }
    }

    public bool TryRemoveAspect<T>(CityEntity entity) where T : CityEntityAspect
    {
        Debug.Assert(entity != null, "CityEntityAspectsService.TryRemoveAspect: entity is null");
        if (entity == null) return false;

        var aspect = entity.Aspects.OfType<T>().FirstOrDefault();
        if (aspect == null)
        {
            Debug.LogWarning($"CityEntityAspectsService: No aspect of type {typeof(T).Name} found on entity {entity.Subject} to remove");
            return false;
        }

        return TryRemoveAspect(entity, aspect);
    }

    public bool TryRemoveAspect(CityEntity entity, CityEntityAspect aspect)
    {
        Debug.Assert(entity != null, "CityEntityAspectsService.TryRemoveAspect: entity is null");
        Debug.Assert(aspect != null, "CityEntityAspectsService.TryRemoveAspect: aspect is null");
        if (entity == null || aspect == null) return false;

        if (!entity.TryRemoveAspectInternal(aspect))
        {
            Debug.LogWarning($"CityEntityAspectsService: Failed to remove aspect {aspect.GetType().Name} from entity {entity.Subject}");
            return false;
        }

        aspect.InternalUnbindFromEntity();

        EmitRemoved(entity, aspect);
        return true;
    }

    private void EmitAdded(CityEntity entity, CityEntityAspect aspect)
    {
        var t = aspect.GetType();
        if (_added.TryGetValue(t, out var list))
        {
            // snapshot to avoid mutation during iteration
            var snap = list.ToArray();
            for (int i = 0; i < snap.Length; i++)
            {
                try
                {
                    ((Action<CityEntity, object>)((e, a) => ((Delegate)snap[i]).DynamicInvoke(e, a)))(entity, aspect);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"CityEntityAspectsService: Exception in added listener for {t.Name}: {ex}");
                }
            }
        }
    }

    private void EmitRemoved(CityEntity entity, CityEntityAspect aspect)
    {
        var t = aspect.GetType();
        if (_removed.TryGetValue(t, out var list))
        {
            var snap = list.ToArray();
            for (int i = 0; i < snap.Length; i++)
            {
                try
                {
                    ((Action<CityEntity, object>)((e, a) => ((Delegate)snap[i]).DynamicInvoke(e, a)))(entity, aspect);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"CityEntityAspectsService: Exception in removed listener for {t.Name}: {ex}");
                }
            }
        }
    }
}
