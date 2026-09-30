using UnityEngine;
public class ProductManager
{
    GenericConfigResolver configResolver = new GenericConfigResolver();
    CarConfigResolver carResolver = new CarConfigResolver();

    ProductBuilder productBuilder = new ProductBuilder();

    public Car CreateCar(CarBaseConfig baseConfig, CarVariantConfig variantConfig, ILocation location)
    {
        CarRuntimeConfig runtimeConfig = carResolver.Resolve(baseConfig, variantConfig);
        return CreateCar(runtimeConfig, location);
    }

    public Car CreateCar(CarRuntimeConfig config, ILocation location)
    {
        Car car = productBuilder.BuildCar(config);
        
        attachProductToLocation(car, location);
        G.ProductLifetimeService.RegisterProduct(car, location);
        foreach(var locations in car.GetNonEmptyProductLocations())
        {
            //not rising events here
            G.ProductLifetimeService.RegisterProduct(locations.Occupant as Product, locations);
        }
        raiseEvents(car, location);
        return car;
    }

    public Wheel CreateWheel(WheelBaseConfig baseConfig, WheelVariantConfig variantConfig, ILocation location)
    {
        WheelRuntimeConfig runtimeConfig = configResolver.Resolve<WheelBaseConfig,WheelVariantConfig,WheelRuntimeConfig>(baseConfig, variantConfig);
        Wheel wheel = productBuilder.BuildWheel(runtimeConfig) as Wheel;

        attachProductToLocation(wheel, location);

        G.ProductLifetimeService.RegisterProduct(wheel, location);
        raiseEvents(wheel, location);
        return wheel;
    }

    public Engine CreateEngine(EngineBaseConfig baseConfig, EngineVariantConfig variantConfig, ILocation location)
    {
        EngineRuntimeConfig runtimeConfig = configResolver.Resolve<EngineBaseConfig,EngineVariantConfig,EngineRuntimeConfig>(baseConfig, variantConfig);
        Engine engine = productBuilder.BuildEngine(runtimeConfig) as Engine;
        
        attachProductToLocation(engine, location);

        G.ProductLifetimeService.RegisterProduct(engine, location);
        raiseEvents(engine, location);
        return engine;
    }

    public Spoiler CreateSpoiler(SpoilerBaseConfig baseConfig, SpoilerVariantConfig variantConfig, ILocation location)
    {
        SpoilerRuntimeConfig runtimeConfig = configResolver.Resolve<SpoilerBaseConfig,SpoilerVariantConfig,SpoilerRuntimeConfig>(baseConfig, variantConfig);
        Spoiler spoiler = productBuilder.BuildSpoiler(runtimeConfig) as Spoiler;

        attachProductToLocation(spoiler, location);

        G.ProductLifetimeService.RegisterProduct(spoiler, location);
        raiseEvents(spoiler, location);
        return spoiler;
    }

    private void raiseEvents(Product product, ILocation location)
    {
        GameEvents.Instance.OnProductCreated?.Invoke(new ProductCreatedEventData(product, location));
        GameEvents.Instance.OnLocatableRegistered?.Invoke(new LocatableCreatedEventData(product as ILocatable, location));
    }

    /// <summary>
    /// internally attach product to location and log error if failed
    /// </summary>
    /// <param name="product"></param>
    /// <param name="location"></param>
    private void attachProductToLocation(Product product, ILocation location)
    {
        if(!location.Attach(product))
        {
            Debug.LogError($"Failed to place product {product.Name} at location {location}");
        }
    }
}