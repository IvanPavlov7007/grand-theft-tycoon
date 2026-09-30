using Sirenix.OdinInspector;
using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Frame", menuName = "Configs/Products/Frame/Frame Variant Config")]
public class CarFrameVariantConfig : ScriptableObject, ISimpleVariantConfig
{
    public bool OverrideBase;
    public CarFrameBaseConfig BaseConfig;
    public CarFrameVariantOverrides CarFrameVariantOverrides;

    public IBaseConfig FallbackBase => BaseConfig;

    bool ISimpleVariantConfig.ForceFallback => OverrideBase;

    public IConfigOverrides Overrides => CarFrameVariantOverrides;
}

[Serializable]
public class CarFrameVariantOverrides : IConfigOverrides
{
    public bool OverrideName;
    [ShowIf("OverrideName")]
    public string Name;

    public bool OverrideFrameColor;
    [ShowIf("OverrideFrameColor")]
    public Color FrameColor = Color.white;

    public bool OverrideWindshieldColor;
    [ShowIf("OverrideWindshieldColor")]
    public Color WindshieldColor = Color.white;

    public bool OverrideIcon;
    [ShowIf("OverrideIcon")]
    public Sprite Icon;

    public bool OverrideBasePrice;
    [ShowIf("OverrideBasePrice")]
    public float BasePrice;
}