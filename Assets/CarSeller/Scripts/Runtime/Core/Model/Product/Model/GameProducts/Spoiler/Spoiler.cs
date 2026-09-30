public sealed class Spoiler : Product
{
    public override string Name => runtimeConfig.Name;

    public override float BasePrice => runtimeConfig.BasePrice;

    public SpoilerRuntimeConfig runtimeConfig;

    public Spoiler(SpoilerRuntimeConfig runtimeConfig)
    {
        this.runtimeConfig = runtimeConfig;
    }

    public override T GetRepresentation<T>(IProductViewBuilder<T> builder)
    {
        return builder.BuildSpoiler(this);
    }
}