using System;
using UnityEngine;

/// <summary>
/// Defines how vision behaves around a center and how objects should be scaled/faded.
/// </summary>
[Serializable]
public sealed class VisionCenterConfig
{
    [Min(0f)] public float Radius = 2f;
}

[Serializable]
public sealed class VisibleDistanceScalerConfig
{
    [Header("Scale mapping")]
    [Range(0f, 10f)] public float ScaleAtMin = 1f;
    [Range(0f, 10f)] public float ScaleAtMax = 0.2f;

    public bool HideBeyondMax = true;

    [Header("Relative distance scaling (of radius 1)")]
    [Min(0f)] public float VisionRelativeMin = 0.75f;
    [Min(0f)] public float VisionRelativeMax = 1f;

    public struct EvaluationResult
    {
        public float Scale;
        public float Alpha;
    }
    public EvaluationResult Evaluate(float distance, float radius)
    {
        return new EvaluationResult
        {
            Scale = EvaluateScale(distance, radius),
            Alpha = EvaluateAlpha(distance, radius)
        };
    }

    public float EvaluateScale(float distance, float radius)
    {
        float relativeDistance = distance / radius;

        if (relativeDistance <= VisionRelativeMin) return ScaleAtMin;
        if (VisionRelativeMax <= VisionRelativeMin) return ScaleAtMax;
        float t = Mathf.InverseLerp(VisionRelativeMin, VisionRelativeMax, relativeDistance);
        return Mathf.Lerp(ScaleAtMin, ScaleAtMax, t);
    }

    public float EvaluateAlpha(float distance, float radius)
    {
        float relativeDistance = distance / radius;

        if (relativeDistance <= VisionRelativeMin) return 1f;
        if (VisionRelativeMax <= VisionRelativeMin) return 1f;
        float t = Mathf.InverseLerp(VisionRelativeMin, VisionRelativeMax, relativeDistance);
        return 1f - t;
    }
}