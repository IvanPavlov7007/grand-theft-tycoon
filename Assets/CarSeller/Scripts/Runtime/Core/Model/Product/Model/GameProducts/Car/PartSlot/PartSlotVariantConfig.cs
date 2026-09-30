[System.Serializable]
public abstract class PartSlotVariantConfig
{
    public abstract PartSlotType SlotType { get; }
    public abstract ISimpleVariantConfig VariantConfig { get; }
}

public class EngineSlotVariantConfig : PartSlotVariantConfig
{
    public override PartSlotType SlotType => PartSlotType.Engine;

    public override ISimpleVariantConfig VariantConfig => engineVariantConfig;

    public EngineVariantConfig engineVariantConfig;
}

public class WheelSlotVariantConfig : PartSlotVariantConfig
{
    public override PartSlotType SlotType => PartSlotType.Wheels;

    public override ISimpleVariantConfig VariantConfig => wheelVariantConfig;

    public WheelVariantConfig wheelVariantConfig;
}

public class SpoilerSlotVariantConfig : PartSlotVariantConfig
{
    public override PartSlotType SlotType => PartSlotType.Spoiler;

    public override ISimpleVariantConfig VariantConfig => spoilerVariantConfig;

    public SpoilerVariantConfig spoilerVariantConfig;
}