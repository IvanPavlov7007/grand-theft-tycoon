using Sirenix.OdinInspector;
using System;
using UnityEngine;
[Serializable]
public abstract class PartSlotRuntimeConfig : IPartSlot
{
    public PartSlotData partSlotData;
    public abstract PartSlotType SlotType { get; }
    public abstract bool TryAccept(Product product);
    public abstract bool CanAccept(Product product);

    public abstract void Detach();
    public abstract IRuntimeConfig RuntimeConfig { get; }

    public abstract Product BuildOccupyingProduct(IProductBuilder productBuilder);
}
[Serializable]
public struct PartSlotData
{
    public bool Required;
    public bool Hidden; // not shown, e g engine
    [HideIf("Hidden")]
    public Vector2 LocalPosition;
    [HideIf("Hidden")]
    public Vector3 LocalRotation;
    [HideIf("Hidden")]
    public Vector3 LocalScale;
    [HideIf("Hidden")]
    public bool facingBackwards;

}

[Serializable]
public class EngineSlotRuntimeConfig : PartSlotRuntimeConfig
{
    public override PartSlotType SlotType => PartSlotType.Engine;
    public EngineRuntimeConfig engineConfig;

    public override bool TryAccept(Product product)
    {
        Engine engine = product as Engine;
        if (engine != null)
        {
            engineConfig = engine.runtimeConfig;
            return true;
        }
        return false;
    }

    public override bool CanAccept(Product product)
    {
        return product is Engine;
    }

    public override void Detach()
    {
        engineConfig = null;
    }

    public override IRuntimeConfig RuntimeConfig => engineConfig;

    public override Product BuildOccupyingProduct(IProductBuilder productBuilder)
    {
        if (engineConfig == null) return null;
        return productBuilder.BuildEngine(engineConfig);
    }
    
}
[Serializable]
public class WheelSlotRuntimeConfig : PartSlotRuntimeConfig
{
    public override PartSlotType SlotType => PartSlotType.Wheels;
    public WheelRuntimeConfig wheelConfig;
    public override bool TryAccept(Product product)
    {
        Wheel wheel = product as Wheel;
        if (wheel != null)
        {
            wheelConfig = wheel.runtimeConfig;
            return true;
        }
        return false;
    }

    public override bool CanAccept(Product product)
    {
        return product is Wheel;
    }

    public override void Detach()
    {
        wheelConfig = null;
    }

    public override IRuntimeConfig RuntimeConfig => wheelConfig;

    public override Product BuildOccupyingProduct(IProductBuilder productBuilder)
    {
        if (wheelConfig == null) return null;
        return productBuilder.BuildWheel(wheelConfig);
    }
}

[Serializable]
public class SpoilerSlotRuntimeConfig : PartSlotRuntimeConfig
{
    public override PartSlotType SlotType => PartSlotType.Spoiler;
    public SpoilerRuntimeConfig spoilerConfig;
    public override bool TryAccept(Product product)
    {
        Spoiler spoiler = product as Spoiler;
        if (spoiler != null)
        {
            spoilerConfig = spoiler.runtimeConfig;
            return true;
        }
        return false;
    }

    public override bool CanAccept(Product product)
    {
        return product is Spoiler;
    }

    public override void Detach()
    {
        spoilerConfig = null;
    }

    public override IRuntimeConfig RuntimeConfig => spoilerConfig;

    public override Product BuildOccupyingProduct(IProductBuilder productBuilder)
    {
        if (spoilerConfig == null) return null;
        return productBuilder.BuildSpoiler(spoilerConfig);
    }
}