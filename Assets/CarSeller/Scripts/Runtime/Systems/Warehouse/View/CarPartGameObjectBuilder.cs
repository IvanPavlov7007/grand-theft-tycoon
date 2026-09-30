using Sirenix.OdinInspector;
using System;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "CarPartViewBuilder", menuName = "Configs/View/Car Part View Builder")]
public class CarPartGameObjectBuilder : ScriptableObject, IProductViewBuilder<GameObject>
{
    public GameObject baseWheelPrefab;
    public GameObject baseSpoilerPrefab;
    public GameObject baseEnginePrefab;

    [ShowInInspector]
    public static string frameChildName = "body";
    [ShowInInspector]
    public static string windshieldChildName = "windshield";

    IProductViewComponentBuilder<ProductView> viewComponentBuilder = new CarPartViewComponentBuilder();

    public GameObject BuildCar(Car car)
    {
        Debug.LogError("CarPartViewBuilder does not support building full Car views. " + car.UniqueName);
        return null;
    }

    //TODO make a better and faster implementation, maybe change the stored value from prefab to something else
    public GameObject BuildCarFrame(CarFrame carFrame)
    {
        var frameGO = Instantiate(carFrame.runtimeConfig.Prefab);
        frameGO.name = carFrame.Name;
        var windshieldSpriteRenderer = CommonTools.AllChildren(frameGO?.transform).
            First(item => item.name.Contains(windshieldChildName, StringComparison.OrdinalIgnoreCase))?.GetComponent<SpriteRenderer>();
        var frameSpriteRenderer = CommonTools.AllChildren(frameGO?.transform).
            First(item => item.name.Contains(frameChildName, StringComparison.OrdinalIgnoreCase))?.GetComponent<SpriteRenderer>();

        windshieldSpriteRenderer.color = carFrame.runtimeConfig.WindshieldColor;
        frameSpriteRenderer.color = carFrame.runtimeConfig.FrameColor;
        viewComponentBuilder.BuildViewComponent(frameGO, carFrame);
        CollisionBuilder.InitializeCollision(frameSpriteRenderer);
        return frameGO;
    }

    public GameObject BuildEngine(Engine engine)
    {
        var engineGO = Instantiate(baseEnginePrefab);
        engineGO.name = engine.Name;
        var engineSpriteRenderer = engineGO.GetComponent<SpriteRenderer>();
        //TODO add engine sprite etc
        CollisionBuilder.InitializeCollision(engineSpriteRenderer);
        viewComponentBuilder.BuildViewComponent(engineGO, engine);
        return engineGO;
    }

    public GameObject BuildSpoiler(Spoiler spoiler)
    {
        var spoilerGO = Instantiate(baseSpoilerPrefab);
        spoilerGO.name = spoiler.Name;
        var spoilerSpriteRenderer = spoilerGO.GetComponent<SpriteRenderer>();
        spoilerSpriteRenderer.color = spoiler.runtimeConfig.Color;
        spoilerSpriteRenderer.sprite = spoiler.runtimeConfig.Sprite;
        spoilerSpriteRenderer.sortingOrder--;

        var slotLocation = G.ProductLifetimeService.GetProductLocation(spoiler) as Car.CarPartLocation;
        var data = slotLocation.PartSlotRuntimeConfig.partSlotData;

        // Apply slot positioning data
        var spoilerTransform = spoilerGO.transform;
        spoilerTransform.localScale = data.LocalScale * spoiler.runtimeConfig.Size;
        spoilerTransform.localRotation = Quaternion.Euler(data.LocalRotation);
        spoilerTransform.localPosition = data.LocalPosition;


        viewComponentBuilder.BuildViewComponent(spoilerGO, spoiler);
        return spoilerGO;
    }

    public GameObject BuildWheel(Wheel wheel)
    {
        var wheelGO = Instantiate(baseWheelPrefab);

        wheelGO.name = wheel.Name;

        var wheelSpriteRenderer = wheelGO.GetComponent<SpriteRenderer>();
        wheelSpriteRenderer.color = wheel.runtimeConfig.Color;

        var slotLocation = G.ProductLifetimeService.GetProductLocation(wheel) as Car.CarPartLocation;
        var chosenSprite = slotLocation.PartSlotRuntimeConfig.partSlotData.facingBackwards ?
            wheel.runtimeConfig.BackSideViewSprite : wheel.runtimeConfig.FrontSideViewSprite;
        var data = slotLocation.PartSlotRuntimeConfig.partSlotData;

        // Apply slot positioning data
        var wheelTransform = wheelGO.transform;
        wheelTransform.localRotation = Quaternion.Euler(data.LocalRotation);
        wheelTransform.localPosition = data.LocalPosition;
        wheelTransform.localScale = data.LocalScale * wheel.runtimeConfig.SideViewSize;

        wheelSpriteRenderer.sprite = chosenSprite;
        if(data.facingBackwards)
            wheelSpriteRenderer.sortingOrder -= 1;

        CollisionBuilder.InitializeCollision(wheelSpriteRenderer);
        viewComponentBuilder.BuildViewComponent(wheelGO, wheel);
        return wheelGO;
    }
    public class CarPartViewComponentBuilder : IProductViewComponentBuilder<ProductView>
    {
        public ProductView BuildViewComponent(GameObject gameObject, Product product)
        {
            Debug.Assert(product != null, "Cannot build view component for null product.");
            Debug.Assert(gameObject!= null, "Cannot build view component on null game object for product: " + product.UniqueName);

            var controller = gameObject.AddComponent<ProductView>();
            controller.Initialize(product, G.ProductLifetimeService.GetProductLocation(product));
            return controller;
        }
    }
}

public static class CarPartViewPlacementHelper
{
    public static GameObject BuildCarPartAtPosition(Car.CarPartLocation carPartLocation, Transform parentCarViewTransform, CarPartGameObjectBuilder carPartViewBuilder)
    {
        var slotData = carPartLocation.PartSlotRuntimeConfig.partSlotData;

        //if hidden or not occupied, skip
        if (slotData.Hidden == true || carPartLocation.Occupant == null)
            return null;

        var part = (carPartLocation.Occupant as Product).GetRepresentation(carPartViewBuilder);
        if (part != null)
        {
            part.transform.SetParent(parentCarViewTransform);
            part.transform.localPosition = slotData.LocalPosition;
            part.transform.localRotation = Quaternion.Euler(slotData.LocalRotation);
            part.transform.localScale = slotData.LocalScale;
        }

        return part;
    }
}