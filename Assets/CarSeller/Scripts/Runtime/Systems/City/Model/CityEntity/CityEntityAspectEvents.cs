public abstract class CityEntityAspectEventData : GameEventData
{
    public CityEntity Entity { get; }
    public CityEntityAspect Aspect { get; }

    protected CityEntityAspectEventData(CityEntity entity, CityEntityAspect aspect)
    {
        Entity = entity;
        Aspect = aspect;
    }
}

public sealed class CityEntityAspectAddedEventData : CityEntityAspectEventData
{
    public CityEntityAspectAddedEventData(CityEntity entity, CityEntityAspect aspect)
        : base(entity, aspect) { }
}

public sealed class CityEntityAspectRemovedEventData : CityEntityAspectEventData
{
    public CityEntityAspectRemovedEventData(CityEntity entity, CityEntityAspect aspect)
        : base(entity, aspect) { }
}
