public sealed class Engine : Product
{
    public EngineRuntimeConfig runtimeConfig;

    public override string Name => runtimeConfig.Name;
    public override float BasePrice => runtimeConfig.BasePrice;

    public Engine(EngineRuntimeConfig runtimeConfig)
    {
        this.runtimeConfig = runtimeConfig;
    }

    public override T GetRepresentation<T>(IProductViewBuilder<T> builder)
    {
        return builder.BuildEngine(this);
    }
}