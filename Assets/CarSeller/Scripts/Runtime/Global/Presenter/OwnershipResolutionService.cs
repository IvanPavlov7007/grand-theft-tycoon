using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Shouldn't be called directly unless
/// 
/// 1) for Transfer of ownership outside of placement (e.g. player buying/selling a warehouse, or stealing a car from someone else)
/// 
/// 2) by a specific service that 
/// creates specific things like ProductLifetimeService
/// </summary>
public class OwnershipResolutionService
{
    private Dictionary<IOwnable, HashSet<IOwnable>> ownerships => World.Instance.ownerships;

    public void RegisterOwnable(OwnableBase ownable)
    {
        Debug.Assert(ownable != null);

        // Ensure current owner relationship exists in the index.
        // (Useful if you load saved games / spawn with pre-set owners.)
        ApplyOwnershipChange(null, ownable.Owner, ownable);
    }

    public bool tryResolveOwnership(IMutableOwnable ownable, ILocation location)
    {
        if (ownable == null || location == null)
            return false;

        // Holder is typically Warehouse, HiddenSpace, CarPark, etc.
        if (location.Holder is IOwnershipContainer container)
        {
            ResolveOnPlacement(ownable, container);
            return true;
        }

        // Location doesn't participate in ownership resolution
        return true;
    }

    public void ResolveOnPlacement(IMutableOwnable item, IOwnershipContainer container)
    {
        Debug.Assert(item != null);
        Debug.Assert(container != null);

        switch (container.OwnershipResolution)
        {
            case OwnershipResolution.Container:
                SetOwner(item, container as IOwnable);
                break;

            case OwnershipResolution.OwnerOfContainerIfNull:
                if (item.Owner == null)
                    SetOwner(item, container.GetOwnerOfContainer());
                break;

            case OwnershipResolution.Clear:
                SetOwner(item, null);
                break;

            case OwnershipResolution.None:
                // Do nothing
                break;
        }
    }

    public void TransferOwnership(IMutableOwnable item, IOwnable newOwner)
    {
        Debug.Assert(item != null);
        Debug.Assert(newOwner != null);
        SetOwner(item, newOwner);
    }

    public void ResolveOnRemoval(IMutableOwnable item)
    {
        Debug.Assert(item != null);
        SetOwner(item, null);
    }

    private void SetOwner(IMutableOwnable item, IOwnable newOwner)
    {
        var oldOwner = item.Owner;
        if (ReferenceEquals(oldOwner, newOwner))
            return;

        item.SetOwner(newOwner);
        ApplyOwnershipChange(oldOwner, newOwner, item);
    }

    private void ApplyOwnershipChange(IOwnable oldOwner, IOwnable newOwner, IOwnable item)
    {
        if (ReferenceEquals(oldOwner, newOwner))
            return;

        if (oldOwner != null && ownerships.TryGetValue(oldOwner, out var oldSet))
        {
            oldSet.Remove(item);
            if (oldSet.Count == 0)
                ownerships.Remove(oldOwner);
        }

        if (newOwner != null)
        {
            if (!ownerships.TryGetValue(newOwner, out var newSet))
            {
                newSet = new HashSet<IOwnable>();
                ownerships[newOwner] = newSet;
            }
            newSet.Add(item);
        }

        GameEvents.Instance.OnOwnershipChanged?.Invoke(new OwnershipChangedEventData(item, oldOwner, newOwner));
    }
}