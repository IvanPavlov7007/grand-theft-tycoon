using Sirenix.OdinInspector;
using System;
using UnityEngine;

[CreateAssetMenu(fileName = "SpoilerVariantConfig", menuName = "Configs/Products/Spoiler/Spoiler Variant Config")]
public class SpoilerVariantConfig : ScriptableObject, ISimpleVariantConfig
{
    public bool OverrideBase;
    public SpoilerBaseConfig FallbackConfig;
    public SpoilerConfigOverrides Overrides;

    bool ISimpleVariantConfig.ForceFallback => OverrideBase;
    IConfigOverrides ISimpleVariantConfig.Overrides => Overrides;
    IBaseConfig ISimpleVariantConfig.FallbackBase => FallbackConfig;

    [Serializable]
    public class SpoilerConfigOverrides : IConfigOverrides
    {
        public bool OverrideName;
        [ShowIf("OverrideName")]
        public string Name;
        public bool OverrideSprite;
        [ShowIf("OverrideSprite")]
        public Sprite Sprite;
        public bool OverrideColor;
        [ShowIf("OverrideColor")]
        public Color Color = Color.white;
        public bool OverrideSize;
        [ShowIf("OverrideSize")]
        public float Size = 1f;
        public bool OverrideBasePrice;
        [ShowIf("OverrideBasePrice")]
        public float BasePrice;
    }

}