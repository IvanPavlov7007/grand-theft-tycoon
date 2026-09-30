// example: [CreateAssetMenu(fileName = "WheelBaseConfig", menuName = "Configs/Products/Wheel/Wheel Base Config")]
public interface IBaseConfig
{
}


public interface IVariantConfig { }

//example: [CreateAssetMenu(fileName = "WheelVariantConfig", menuName = "Configs/Products/Wheel/Wheel Variant Config")]
public interface ISimpleVariantConfig : IVariantConfig
{
    IBaseConfig FallbackBase { get; }
    bool ForceFallback { get; }
    IConfigOverrides Overrides { get; }
}



public interface IConfigOverrides
{
}

public interface IRuntimeConfig
{
    
}