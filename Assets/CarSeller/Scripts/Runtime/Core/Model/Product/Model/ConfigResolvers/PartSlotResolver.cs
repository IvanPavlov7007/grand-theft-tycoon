using System;
using UnityEngine;

//Todo maybe move type switch to classes themselves

public class PartSlotResolver
{
    private readonly GenericConfigResolver _resolver;

    public PartSlotResolver(GenericConfigResolver resolver)
    {
        _resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="baseConfig"></param>
    /// <param name="variantSlotConfig"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="Exception"></exception>
    internal PartSlotRuntimeConfig Resolve(PartSlotBaseConfig baseConfig, PartSlotVariantConfig variantSlotConfig)
    {
        if (baseConfig == null) throw new ArgumentNullException(nameof(baseConfig));
        if(variantSlotConfig != null)
        Debug.Assert(baseConfig.SlotType == variantSlotConfig?.SlotType,
            $"Mismatched slot types: base '{baseConfig.SlotType}', variant '{variantSlotConfig?.SlotType}'");

        //check if any base config is present in either base or variant (via fallback) - otherwise only slot data is resolved
        
        bool resolveProduct = IsResolveProductNeeded(baseConfig,variantSlotConfig);

        switch (baseConfig.SlotType)
        {
            case PartSlotType.Wheels:
                var wheelRuntimeSlot = CreateWheelRuntimeSlot(baseConfig as WheelSlotBaseConfig);
                if(resolveProduct)
                    wheelRuntimeSlot.wheelConfig = 
                        ResolveWheel(baseConfig as WheelSlotBaseConfig, variantSlotConfig as WheelSlotVariantConfig);
                return wheelRuntimeSlot;
            case PartSlotType.Engine:
                var engineRuntimeSlot = CreateEngineRuntimeSlot(baseConfig as EngineSlotBaseConfig);
                if(resolveProduct)
                    engineRuntimeSlot.engineConfig =
                        ResolveEngine(baseConfig as EngineSlotBaseConfig, variantSlotConfig as EngineSlotVariantConfig);
                return engineRuntimeSlot;

            case PartSlotType.Spoiler:
                var spoilerRuntimeSlot = CreateSpoilerRuntimeSlot(baseConfig as SpoilerSlotBaseConfig);
                if(resolveProduct)
                    spoilerRuntimeSlot.spoilerConfig =
                        ResolveSpoiler(baseConfig as SpoilerSlotBaseConfig, variantSlotConfig as SpoilerSlotVariantConfig);
                return spoilerRuntimeSlot;

            default:
                throw new Exception($"Unknown slot base config type: {baseConfig.GetType().Name}");
        }
    }

    private bool IsResolveProductNeeded(PartSlotBaseConfig baseConfig, PartSlotVariantConfig variantSlotConfig)
    {
        bool variantConfigForcesNull =
            variantSlotConfig != null &&
            variantSlotConfig.VariantConfig != null &&
            variantSlotConfig.VariantConfig.FallbackBase == null &&
            variantSlotConfig.VariantConfig.ForceFallback;

        if(variantConfigForcesNull)
            return false;

        return baseConfig.BaseConfig != null ||
            (variantSlotConfig?.VariantConfig != null && variantSlotConfig.VariantConfig.FallbackBase != null);
    }


    private WheelSlotRuntimeConfig CreateWheelRuntimeSlot(WheelSlotBaseConfig baseCfg)
    {
        var runtimeSlot = new WheelSlotRuntimeConfig
        {
            partSlotData = baseCfg.partSlotData
        };
        return runtimeSlot;
    }

    private EngineSlotRuntimeConfig CreateEngineRuntimeSlot(EngineSlotBaseConfig baseCfg)
    {
        var runtimeSlot = new EngineSlotRuntimeConfig
        {
            partSlotData = baseCfg.partSlotData
        };
        return runtimeSlot;
    }

    private SpoilerSlotRuntimeConfig CreateSpoilerRuntimeSlot(SpoilerSlotBaseConfig baseCfg)
    {
        var runtimeSlot = new SpoilerSlotRuntimeConfig
        {
            partSlotData = baseCfg.partSlotData
        };
        return runtimeSlot;
    }

    private WheelRuntimeConfig ResolveWheel(WheelSlotBaseConfig baseCfg, WheelSlotVariantConfig variantCfg)
    {
        return _resolver.Resolve<
            WheelBaseConfig,
            WheelVariantConfig,
            WheelRuntimeConfig
            >(
                baseCfg.wheelBaseConfig,
                variantCfg?.wheelVariantConfig
            ); ;
    }

    private EngineRuntimeConfig ResolveEngine(EngineSlotBaseConfig baseCfg, EngineSlotVariantConfig variantCfg)
    {
        return _resolver.Resolve<
            EngineBaseConfig,
            EngineVariantConfig,
            EngineRuntimeConfig
            >(
                baseCfg.engineBaseConfig,
                variantCfg?.engineVariantConfig
            );
    }

    private SpoilerRuntimeConfig ResolveSpoiler(SpoilerSlotBaseConfig baseCfg, SpoilerSlotVariantConfig variantCfg)
    {
        return _resolver.Resolve<
                SpoilerBaseConfig,
                SpoilerVariantConfig,
                SpoilerRuntimeConfig
            >(
                baseCfg.spoilerBaseConfig,
                variantCfg?.spoilerVariantConfig
            ); 
    }
}