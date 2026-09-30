using System.Collections.Generic;

public class CarConfigResolver
{
    private readonly GenericConfigResolver _resolver = new GenericConfigResolver();
    private readonly PartSlotResolver _partSlotResolver;

    public CarConfigResolver()
    {
        _partSlotResolver = new PartSlotResolver(_resolver);
    }

    public CarRuntimeConfig Resolve(CarBaseConfig carBase, CarVariantConfig carVariant)
    {
        var result = new CarRuntimeConfig
        {
            Name = carBase.Name,
            CarFrameRuntimeConfig = ResolveFrame(carBase, carVariant),
            SlotConfigs = new List<PartSlotRuntimeConfig>()
        };
        // OWN VALUES
        // Base price
        if (carVariant != null && carVariant.OverrideBasePrice)
        {
                result.BasePrice = carVariant.BasePrice;
        }
        else
            result.BasePrice = carBase.BasePrice;

        // Speed
        if (carVariant != null && carVariant.OverrideSpeed)
        {
            result.Speed = carVariant.Speed;
        }
        else
            result.Speed = carBase.Speed;

        // Acceleration
        if (carVariant != null && carVariant.OverrideAcceleration)
        {
            result.Acceleration = carVariant.Acceleration;
        }
        else
            result.Acceleration = carBase.Acceleration;

        result.Kind = carBase.Kind;
        result.TopView = carBase.TopView;
        result.SideView = carBase.SideView;

        initializeBasicModifiers(result);

        // COMPOSITION
        // Build pools
        var basePool = carBase.SlotConfigs != null
            ? new List<PartSlotBaseConfig>(carBase.SlotConfigs)
            : new List<PartSlotBaseConfig>();

        var variantPool = (carVariant?.slotConfigs != null)
            ? new List<PartSlotVariantConfig>(carVariant.slotConfigs)
            : new List<PartSlotVariantConfig>();

        // Compare one-by-one: for each variant slot, try to find a matching base slot by type.
        foreach (var vSlot in variantPool)
        {
            var idx = basePool.FindIndex(b => b.SlotType == vSlot.SlotType);
            if (idx >= 0)
            {
                var bSlot = basePool[idx];
                result.SlotConfigs.Add(_partSlotResolver.Resolve(bSlot, vSlot));
                basePool.RemoveAt(idx);
            }
        }

        // No variants left: apply remaining base slots as-is
        foreach (var remaining in basePool)
        {
            result.SlotConfigs.Add(_partSlotResolver.Resolve(remaining, null));
        }

        return result;
    }


    private void initializeBasicModifiers(CarRuntimeConfig config)
    {
        var list = new List<CarModifier>();
        //switch (config.Kind.Type)
        //{
        //    case CarType.Bike:
        //        list.AddRange(new CarModifier[] { new CanNarrowStreet(), new CanTurnAround() });
        //        break;
        //    case CarType.Small:
        //        list.Add(new CanTurnAround());
        //        break;
        //}
        list.AddRange(new CarModifier[] { new CanNarrowStreet(), new CanTurnAround() });
        config.InitializeModifiers(list);
    }

    private CarFrameRuntimeConfig ResolveFrame(CarBaseConfig baseCfg, CarVariantConfig variantCfg)
    {
        return _resolver.Resolve<CarFrameBaseConfig, CarFrameVariantConfig, CarFrameRuntimeConfig>(
            baseCfg.CarFrameBaseConfig,
            variantCfg?.carFrameRuntimeConfig
        );
    }
}
