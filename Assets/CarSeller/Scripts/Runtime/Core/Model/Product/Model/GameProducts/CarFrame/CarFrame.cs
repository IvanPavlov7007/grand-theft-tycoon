public class CarFrame : Product
{
    public CarFrameRuntimeConfig runtimeConfig;

    public override string Name => runtimeConfig.Name;
    public override float BasePrice => runtimeConfig.BasePrice;

    public CarFrame(CarFrameRuntimeConfig runtimeConfig)
    {
        this.runtimeConfig = runtimeConfig;
    }

    public override T GetRepresentation<T>(IProductViewBuilder<T> builder)
    {
        return builder.BuildCarFrame(this);
    }
}