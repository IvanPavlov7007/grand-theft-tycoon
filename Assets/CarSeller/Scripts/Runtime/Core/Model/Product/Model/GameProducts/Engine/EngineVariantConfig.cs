using Sirenix.OdinInspector;
using System;
using UnityEngine;

[CreateAssetMenu(fileName = "EngineVariantConfig", menuName = "Configs/Products/Engine/Engine Variant Config")]
public class EngineVariantConfig : ScriptableObject, ISimpleVariantConfig
{
    public bool OverrideBase;
    public EngineBaseConfig FallbackBase;
    public ConfigOverrides Overrides;

    bool ISimpleVariantConfig.ForceFallback => OverrideBase;
    IConfigOverrides ISimpleVariantConfig.Overrides => Overrides;
    IBaseConfig ISimpleVariantConfig.FallbackBase => FallbackBase;

    [Serializable]
    public class ConfigOverrides : IConfigOverrides
    {
        public bool OverrideName;
        [ShowIf("OverrideName")]
        public string Name;

        public bool OverrideLevel;
        [ShowIf("OverrideLevel")]
        public int Level = 1;

        public bool OverrideBasePrice;
        [ShowIf("OverrideBasePrice")]
        public float BasePrice;
    }
}