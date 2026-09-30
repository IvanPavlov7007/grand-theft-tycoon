using System;
using UnityEngine;
[Serializable]
public abstract class PartSlotBaseConfig
{
    public abstract PartSlotType SlotType { get; }
    public abstract IBaseConfig BaseConfig { get; }
    //Initial value here
    public PartSlotData partSlotData = new PartSlotData { LocalScale = Vector3.one };
}

[Serializable]
public class EngineSlotBaseConfig : PartSlotBaseConfig
{
    public override PartSlotType SlotType => PartSlotType.Engine;

    public override IBaseConfig BaseConfig => engineBaseConfig;

    public EngineBaseConfig engineBaseConfig;
}
[Serializable]

public class WheelSlotBaseConfig : PartSlotBaseConfig
{
    public override PartSlotType SlotType => PartSlotType.Wheels;
    public override IBaseConfig BaseConfig => wheelBaseConfig;
    public WheelBaseConfig wheelBaseConfig;
}
[Serializable]

public class SpoilerSlotBaseConfig : PartSlotBaseConfig
{
    public override PartSlotType SlotType => PartSlotType.Spoiler;
    public override IBaseConfig BaseConfig => spoilerBaseConfig;
    public SpoilerBaseConfig spoilerBaseConfig;
}