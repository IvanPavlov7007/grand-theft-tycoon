using Sirenix.OdinInspector;
using System;
using UnityEngine;

[CreateAssetMenu(fileName = "WheelVariantConfig", menuName = "Configs/Products/Wheel/Wheel Variant Config")]
public class WheelVariantConfig : ScriptableObject, ISimpleVariantConfig
{
    public WheelBaseConfig BaseFallback;
    public bool ForceFallback;
    public WheelOverrides Overrides;

    public IBaseConfig FallbackBase => BaseFallback;
    bool ISimpleVariantConfig.ForceFallback => ForceFallback;
    IConfigOverrides ISimpleVariantConfig.Overrides => Overrides;
}

[Serializable]
public class WheelOverrides : IConfigOverrides
{
    public bool OverrideName;
    [ShowIf("OverrideName")]    
    public string Name;

    public bool OverrideColor;
    [ShowIf("OverrideColor")]
    public Color Color = Color.white;

    public bool OverrideSideViewSize;
    [ShowIf("OverrideSideViewSize")]
    public float SideViewSize = 1f;

    public bool OverrideTopViewSize;
    [ShowIf("OverrideTopViewSize")]
    public float TopViewSize;

    public bool OverrideBasePrice;
    [ShowIf("OverrideBasePrice")]
    public float BasePrice;
}