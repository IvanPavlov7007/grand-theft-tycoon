using System;

public interface CityEntityAspect {
    public CityEntity Entity { get; }

    void InternalUnbindFromEntity();
}

/// <summary>
/// Base class for aspects that need to know which CityEntity they belong to.
/// The reference is assigned by `CityEntityAspectsService` when the aspect is added.
/// </summary>
public abstract class CityEntityAspectBase : CityEntityAspect
{
    public CityEntity Entity { get; private set; }
    public virtual void InternalBindToEntity(CityEntity entity)
    {
        Entity = entity;
    }

    public virtual void InternalUnbindFromEntity()
    {
        Entity = null;
    }

}

public sealed class MarkerReferenceAspect : CityEntityAspectBase
{
    public readonly City.CityMarker CityMarker;

    public MarkerReferenceAspect(City.CityMarker cityMarker)
    {
        CityMarker = cityMarker;
    }
}
public sealed class PinAspect : CityEntityAspectBase
{
    public readonly PinStyle Style;
    public bool IsScreenConfined = false;

    public PinAspect(PinStyle style, bool isScreenConfined = false)
    {
        Style = style ?? throw new ArgumentNullException(nameof(style));
        IsScreenConfined = isScreenConfined;
    }
}
public sealed class TriggerableAspect : CityEntityAspectBase
{
}

public class InteractableAspect : CityEntityAspectBase
{
    public readonly int SortingOrder;

    public InteractableAspect(int sortingOrder)
    {
        SortingOrder = sortingOrder;
    }
}

public sealed class DragInteractableAspect : InteractableAspect
{
    public DragInteractableAspect(int sortingOrder) : base(sortingOrder)
    {
    }
}

public sealed class CarAspect : CityEntityAspectBase
{
}

public sealed class PlayerFigureAspect : CityEntityAspectBase
{
}

public sealed class TriggerCausableAspect : CityEntityAspectBase
{
}

public sealed class PoliceUnitAspect : CityEntityAspectBase
{
}

public sealed class RigidbodyAspect : CityEntityAspectBase
{

}