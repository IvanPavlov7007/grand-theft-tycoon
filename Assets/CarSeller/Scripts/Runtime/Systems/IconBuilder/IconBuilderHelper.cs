using System;
using UnityEngine;
public static class IconBuilderHelper
{
    [Obsolete]
    public static Sprite BuildCarSprite(Car car)
    {
        return BuildProdutSpite(car);
    }
    [Obsolete]
    public static Sprite BuildCarFrameSprite(CarFrame carFrame)
    {
        return BuildProdutSpite(carFrame);
    }
    [Obsolete]
    public static Sprite BuildWheelSprite(Wheel wheel)
    {
        return BuildProdutSpite(wheel);
    }
    [Obsolete]
    public static Sprite BuildSpoilerSprite(Spoiler spoiler)
    {
        return BuildProdutSpite(spoiler);
    }
    [Obsolete]
    public static Sprite BuildEngineSprite(Engine engine)
    {
        return BuildProdutSpite(engine);
    }

    public static Sprite BuildProdutSpite(Product product)
    {
        if (product == null)
            return null;

        // Use the visitor implementation on Product to pick the right BuildX method.
        // IconBuilderManager implements IProductViewBuilder<Sprite>.
        if (product is Car car)
        {
            return car.runtimeConfig.SideView;
        }

        return product.GetRepresentation(G.IconBuilderManager);
    }
}