public sealed class Wheel : Product
{
    public WheelRuntimeConfig runtimeConfig;

    public override string Name => runtimeConfig.Name;
    public override float BasePrice => runtimeConfig.BasePrice;

    public Wheel(WheelRuntimeConfig runtimeConfig)
    {
        this.runtimeConfig = runtimeConfig;
    }

    public override T GetRepresentation<T>(IProductViewBuilder<T> builder)
    {
        return builder.BuildWheel(this);
    }
}