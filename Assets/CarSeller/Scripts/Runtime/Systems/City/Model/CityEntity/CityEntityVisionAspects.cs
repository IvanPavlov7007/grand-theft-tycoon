using System;
using UnityEngine;

/// <summary>
/// Marks this entity as a vision center.
/// The fog will reveal an area around it with the given radius.
/// Also provides the distance scaling configuration applied to other entities.
/// </summary>
public sealed class VisionCenterAspect : CityEntityAspectBase
{
    public readonly VisionCenterConfig Config;

    public VisionCenterAspect(VisionCenterConfig config)
    {
        Config = config ?? throw new ArgumentNullException(nameof(config));
    }

    public VisionCenterAspect(float radius)
    {
        Config = new VisionCenterConfig { Radius = Mathf.Max(0f, radius) };
    }
}

/// <summary>
/// Enables scaling / fading with distance to the nearest vision center.
/// The scaling curve/ranges are taken from the nearest vision center's <see cref="VisionCenterAspect"/>.
/// </summary>
public sealed class VisibleDistanceScalerAspect : CityEntityAspectBase
{
    public CityVisibleAspect VisibleAspect { get; private set; }
    public readonly VisibleDistanceScalerConfig Config;

    public VisibleDistanceScalerConfig.EvaluationResult Evaluate()
    {
        return Config.Evaluate(VisibleAspect.DistanceToNearestCenter, VisibleAspect.NearestCenter.Config.Radius);
    }

    public VisibleDistanceScalerAspect(VisibleDistanceScalerConfig config)
    {
        Config = config;
    }

    public VisibleDistanceScalerAspect()
    {
        Config = new VisibleDistanceScalerConfig();
    }

    public static VisibleDistanceScalerAspect CreateDontHide()
    {
        return new VisibleDistanceScalerAspect(new VisibleDistanceScalerConfig { HideBeyondMax = false, ScaleAtMax = 0.7f});
    }

    public override void InternalBindToEntity(CityEntity entity)
    {
        base.InternalBindToEntity(entity);
        VisibleAspect = entity.GetAspect<CityVisibleAspect>();
        Debug.Assert(VisibleAspect != null, $"Entity {entity} has {nameof(VisibleDistanceScalerAspect)} but no {nameof(CityVisibleAspect)}");
    }
}
