using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "CityViewObjectBuilder", menuName = "Configs/View/CityViewObjectBuilder")]
public class CityViewObjectBuilder : SerializedScriptableObject
{
    public GameObject carViewPrefab;
    public GameObject playerFigureViewPrefab;
    public GameObject triggerPrefab;
    public GameObject policeUnitPrefab;

    //pure view
    public GameObject collectablePrefab;

    [Header("UI Prefabs")]
    public GameObject pinUIPrefab;
    public PinStyle WarehousePinStyle;
    public PinStyle CarStashWarehousePinStyle;
    public PinStyle PersonalVehicleShopPinStyle;
    public PinStyle BuyerPinStyle;
    public Dictionary<CarType, Sprite> BuyersPinSprites;

    CityUIBuilder CityUIBuilder = new CityUIBuilder();

    public CityViewObjectController BuildObject(CityEntity entity)
    {
        if (entity == null)
        {
            Debug.LogError("CityEntity is null");
            return null;
        }

        switch (entity.Subject)
        {
            case Car car:
                return buildCar(car, entity, entity.Position);

            case PlayerFigure playerFigure:
                return buildPlayerFigure(playerFigure, entity, entity.Position);

            case Collectable collectable:
                return buildCollectable(collectable, entity, entity.Position);

            case PoliceUnit policeUnit:
                return buildPoliceUnit(policeUnit, entity, entity.Position);

            case ILocatable locatable:
                return buildGeneric(entity, entity.Position);

            default:
                Debug.LogError($"No builder for city object of type {entity.Subject.GetType().Name}");
                return null;
        }
    }

    CityViewObjectController initializeViewController(GameObject go, CityEntity cityEntity)
    {
        var viewController =
            go.AddComponent<CityViewObjectController>().Initialize(cityEntity);
        return viewController;
    }

    public CityViewObjectController buildCollectable(Collectable collectable, CityEntity entity, CityPosition position)
    {
        var worldPos = position.WorldPosition;

        GameObject collectableGO = Instantiate(triggerPrefab, worldPos, Quaternion.identity);
        Instantiate(collectablePrefab, worldPos, Quaternion.identity, collectableGO.transform);

        var viewController = initializeViewController(collectableGO, entity);
        initializeAspects(collectableGO, viewController, entity);
        return viewController;
    }

    public CityViewObjectController buildCar(Car car, CityEntity entity, CityPosition position)
    {
        GameObject carGO = Instantiate(carViewPrefab, position.WorldPosition, Quaternion.identity);

        var viewController = initializeViewController(carGO, entity);
        initializeAspects(carGO, viewController, entity);
        // This one must be after SelectableVisuals to catch selection changes
        carGO.AddComponent<ViewVisualStateChanger>();
        return viewController;
    }

    public CityViewObjectController buildPlayerFigure(PlayerFigure playerFigure, CityEntity entity, CityPosition position)
    {
        if (playerFigureViewPrefab == null)
        {
            Debug.LogError("CityViewObjectBuilder: playerFigureViewPrefab is not assigned");
            return null;
        }

        GameObject go = Instantiate(playerFigureViewPrefab, position.WorldPosition, Quaternion.identity);
        var viewController = initializeViewController(go, entity);
        initializeAspects(go, viewController, entity);
        go.AddComponent<ViewVisualStateChanger>();
        return viewController;
    }

    public CityViewObjectController buildGeneric(CityEntity entity, CityPosition position)
    {
        GameObject triggerGO = Instantiate(triggerPrefab, position.WorldPosition, Quaternion.identity);
        var viewController = initializeViewController(triggerGO, entity);
        initializeAspects(triggerGO, viewController, entity);
        return viewController;
    }

    public CityViewObjectController buildPoliceUnit(PoliceUnit policeUnit, CityEntity entity, CityPosition position)
    {
        GameObject policeGO = Instantiate(policeUnitPrefab, position.WorldPosition, Quaternion.identity);

        var viewController = initializeViewController(policeGO, entity);
        initializeAspects(policeGO, viewController, entity);
        policeGO.AddComponent<ViewVisualStateChanger>();
        return viewController;
    }

    void initializeAspects(GameObject go, CityViewObjectController viewController, CityEntity entity)
    {
        // Keep one builder instance per builder asset.
        // Note: views dictionary is managed by CitySceneManager; for now this builder instance just applies initial aspects.
        // Dynamic updates are handled by an instance created by CitySceneManager.

        foreach (var aspect in entity.Aspects)
        {
            initializeAspect(go, viewController, entity, aspect);
        }
    }

    void initializeAspect(GameObject go, CityViewObjectController viewController, CityEntity entity, CityEntityAspect aspect)
    {
        switch (aspect)
        {
            case PinAspect pinStyleAspect:
                CityUIBuilder.SetUpCityPin(
                    viewController,
                    pinUIPrefab,
                    pinStyleAspect.Style);
                break;
            case TriggerableAspect triggerableAspect:
                go.AddComponent<Triggerable>();
                break;
            case TriggerCausableAspect triggerCausableAspect:
                go.AddComponent<TriggerCausable>();
                break;
            case DragInteractableAspect dragInteractableAspect:
                var dragInteractable = go.AddComponent<DragInteractable>();
                dragInteractable.sortingOrder = dragInteractableAspect.SortingOrder;
                go.AddComponent<InteractableOrderOnSelect>();
                go.AddComponent<DragDisabler>();
                break;
            case InteractableAspect interactableAspect:
                var interactable = go.AddComponent<Interactable>();
                interactable.sortingOrder = interactableAspect.SortingOrder;
                go.AddComponent<InteractableOrderOnSelect>();
                break;
            case CarAspect carAspect:
                Car car = entity.Subject as Car;
                go.AddComponent<SpeedProviderFromCar>().Initialize(car);
                go.AddComponent<MovingPoint>().Initialize(entity);
                var sr = go.GetComponentInChildren<SpriteRenderer>();
                if(car.runtimeConfig.TopView != null)
                    sr.sprite = car.runtimeConfig.TopView;
                //sr.color = car.CarFrame.runtimeConfig.FrameColor;
                go.AddComponent<SelectableVisuals>();
                break;
            case PlayerFigureAspect playerFigureAspect:
                var figure = entity.Subject as PlayerFigure;
                go.AddComponent<PlayerFigureSpeedProvider>().Initialize(figure);
                go.AddComponent<MovingPoint>().Initialize(entity);
                // TriggerCausable is added via TriggerCausableAspect
                go.AddComponent<SelectableVisuals>();
                break;
            case PoliceUnitAspect policeUnitAspect:
                var unit = entity.Subject as PoliceUnit;
                go.AddComponent<MovingPointSimpleView>().Initialize(unit.GraphMovement);
                var policeViewController = go.AddComponent<PoliceStateViewController>();
                policeViewController.Initialize(unit);
                go.AddComponent<PoliceSpotlightVisionVisuals>().Intialize(unit, policeViewController);
                go.AddComponent<PoliceLightsViewController>().Initialize(policeViewController, go.GetComponentInChildren<PoliceLightsVisuals>());
                break;
            case RigidbodyAspect rigidbodyAspect:
                var rigidbody2D = go.AddComponent<Rigidbody2D>();
                rigidbody2D.bodyType = RigidbodyType2D.Kinematic;
                break;
            case VisibleDistanceScalerAspect visibleDistanceScalerAspect:
                go.AddComponent<VisionDistanceScaler>().Initialize(visibleDistanceScalerAspect);
                break;
        }
    }

    public PinStyle GetBuyerPinStyle(CarType requiredCarType)
    {
        var style = BuyerPinStyle.Clone();
        if (BuyersPinSprites.TryGetValue(requiredCarType, out var sprite))
        {
            style.Icon = sprite;
        }
        return style;
    }
}