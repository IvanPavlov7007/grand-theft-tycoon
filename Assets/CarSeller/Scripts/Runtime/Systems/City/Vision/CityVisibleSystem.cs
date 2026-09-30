using System.Collections.Generic;

public abstract class AspectSystem<TAspect> : IDisposable where TAspect : class, CityEntityAspect
{
    protected Dictionary<CityEntity, TAspect> aspects = new();
    CityEntityAspectsService aspectsService;

    protected AspectSystem(CityEntityAspectsService aspectsService)
    {
        Subscribe(aspectsService);
    }
    public virtual void Dispose()
    {
        Unsubscribe();
    }

    void Subscribe(CityEntityAspectsService service)
    {
        aspectsService = service;
        aspectsService.SubscribeAdded<TAspect>(onAspectAdded);
        aspectsService.SubscribeRemoved<TAspect>(onAspectRemoved);
    }

    void Unsubscribe()
    {
        if(aspectsService == null)
            return;
        aspectsService.UnsubscribeAdded<TAspect>(onAspectAdded);
        aspectsService.UnsubscribeRemoved<TAspect>(onAspectRemoved);
        aspectsService = null;
    }

    private void onAspectAdded(CityEntity entity, TAspect aspect)
    {
        aspects.Add(entity, aspect);
        OnAspectAdded(entity, aspect);
    }

    private void onAspectRemoved(CityEntity entity, TAspect aspect)
    {
        aspects.Remove(entity);
        OnAspectRemoved(entity, aspect);
    }

    protected virtual void OnAspectAdded(CityEntity entity, TAspect aspect) { }
    protected virtual void OnAspectRemoved(CityEntity entity, TAspect aspect) { }
}

public class CityVisibleSystem : AspectSystem<CityVisibleAspect>
{
    CityVisionCentersSystem CityVisionCentersSystem;

    public bool cheatVisible = false;

    public CityVisibleSystem(CityEntityAspectsService aspectsService, CityVisionCentersSystem cityVisionCentersSystem) : base(aspectsService)
    {
        this.CityVisionCentersSystem = cityVisionCentersSystem;
    }

    public void Update()
    {
        //how many
        foreach (var asp in aspects.Values)
        {
            if(cheatVisible)
            {
                asp.SetVisible(true);
                asp.NearestCenter = null;
                asp.DistanceToNearestCenter = 0f;
                if (asp.Discovered == false)
                    asp.Discover();
                continue;
            }
            updateAspect(asp);
        }
    }

    void updateAspect(CityVisibleAspect aspect)
    {
        //check WorldPOsiton
        if (CityVisionCentersSystem.TryGetNearestCenter(aspect.Entity.Position.WorldPosition, out var center, out float distance)
            && distance < center.Config.Radius)
        {
            aspect.SetVisible(true);
            aspect.Discover();
        }
        else
        {
            aspect.SetVisible(VisionLogic.VisibleWhenNoCenter);
        }
        aspect.DistanceToNearestCenter = distance;
        aspect.NearestCenter = center;
    }
}

public static class VisionLogic
{
    public const bool VisibleWhenNoCenter = true;
}

public class CityVisibleAspect : CityEntityAspectBase
{
    public event System.Action<CityVisibleAspect> OnDiscovered;
    public bool Discovered { get; private set; }
    public bool Visible { get; private set; }
    public float DistanceToNearestCenter { get; internal set; }
    public VisionCenterAspect NearestCenter { get; internal set; }
    public void Discover()
    {
        if (Discovered)
            return;
        Discovered = true;
        OnDiscovered?.Invoke(this);
    }

    public static CityVisibleAspect CreateDiscovered()
    {
        var aspect = new CityVisibleAspect();
        aspect.Discover();
        return aspect;
    }

    public void SetVisible(bool visible)
    {
        Visible = visible;
    }
}