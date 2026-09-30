using System.Collections.Generic;
using UnityEngine;

//Configuration class for creating city entities

//TODO either creating car or moving car should use this also. Make the causes also a fixed code source

/// <summary>
/// Design:
/// There are 2 types that define city entities:
/// 1) (high level) CityEntitiesCreationHelper - helper tools for city entities that created in the city,
///     employs CityEntityLifetimeService
/// 2) (low level) CityEntityLifetimeService - used when moving products into the city
///     [Maybe there should be a defined service that decides how their entities are created?]
/// </summary>
public static class CityEntitiesCreationHelper
{

    public static CityEntity CreateBuyer(Buyer buyer, CityPosition position)
    {
        var specificPinStyle = G.cityViewObjectBuilder.GetBuyerPinStyle(buyer.RequiredCarType);

        return CreateTriggerInteractable(buyer, position,
            new CityVisibleAspect(),
            VisibleDistanceScalerAspect.CreateDontHide(),
            new PinAspect(specificPinStyle));
    }

    public static CityEntity CreateMissionLauncher(MissionLauncher missionLauncher)
    {
        var config = missionLauncher.Config;
        var marker = config.cityMarkerRef.GetMarker();
        return CreateTriggerInteractable(missionLauncher, marker.PositionOnGraph.Value,
            CityVisibleAspect.CreateDiscovered(),
            VisibleDistanceScalerAspect.CreateDontHide(),
            new PinAspect(config.pinStyle, true),
            new MarkerReferenceAspect(marker));
    }

    public static CityEntity CreateWarehouse(Warehouse warehouse, CityPosition position)
    {
        return CreateTriggerInteractable(warehouse, position, new PinAspect(G.cityViewObjectBuilder.WarehousePinStyle));
    }

    public static CityEntity CreateCarStashWarehouse(CarStashWarehouse carStashWarehouse, CityPosition position)
    {
        return CreateTriggerInteractable(carStashWarehouse, position,
            new CityVisibleAspect(),
            VisibleDistanceScalerAspect.CreateDontHide(),
            new PinAspect(G.cityViewObjectBuilder.CarStashWarehousePinStyle,true));
    }

    public static CityEntity CreatePersonalVehicleShop(PersonalVehicleShop personalVehicleShop, CityPosition position)
    {
        return CreateTriggerInteractable(personalVehicleShop, position,
            CityVisibleAspect.CreateDiscovered(),
            VisibleDistanceScalerAspect.CreateDontHide(),
            new PinAspect(G.cityViewObjectBuilder.PersonalVehicleShopPinStyle,true));
    }

    public static CityEntity CreatePinnedMarkerReferencedTriggerInteractable(ILocatable locatable, CityPosition position, PinStyle pinStyle, CityMarkerRef markerRef, bool confinedToScreen = true)
    {
        return CreateTriggerInteractable(locatable, position,
            CityVisibleAspect.CreateDiscovered(),
            VisibleDistanceScalerAspect.CreateDontHide(),
            new PinAspect(pinStyle, confinedToScreen),
            new MarkerReferenceAspect(markerRef.GetMarker()));
    }

    public static CityEntity CreateTriggerInteractable(ILocatable locatable, CityPosition position, params CityEntityAspect[] additionalAspects)
    {
        var aspects = new List<CityEntityAspect>
        {
            new InteractableAspect(5),
            new TriggerableAspect(),
        };
        aspects.AddRange(additionalAspects);

        if (City.EntityLifetimeService.TryCreate(locatable, position, out CityEntity entity, aspects.ToArray()))
        {
            return entity;
        }
        Debug.LogError("Failed to create police unit entity");
        return null;
    }

    public static CityEntity CreatePoliceUnit(PoliceUnit policeUnit, CityPosition position)
    {
        if (City.EntityLifetimeService.TryCreate(policeUnit, position, out CityEntity entity,
            new CityEntityAspect[]{
                new RigidbodyAspect(),
                new InteractableAspect(0),
                new TriggerableAspect(),
                new PoliceUnitAspect(),
                }))
        {
            return entity;
        }
        Debug.LogError("Failed to create police unit entity");
        return null;
    }

    public static CityEntity CreateNewCar(CarBaseConfig baseConfig, CarVariantConfig variantConfig, CityPosition position)
    {
        // 1) creating via product Manager, hack for now
        var hidden = World.Instance.HiddenSpace.GetEmptyLocation();
        var car = G.ProductManager.CreateCar(baseConfig, variantConfig, hidden);

        // 2) moving into city
        return MoveInExistingCar(car, position);
    }

    public static CityEntity MoveInExistingCar(Car car, CityPosition position)
    {
        if (City.EntityLifetimeService.TryMoveToCity(car, position, out CityEntity entity, CreateCarAspects()))
        {
            return entity;
        }
        Debug.LogError("Failed to move in existing car entity");
        return null;
    }

    public static CityEntity CreatePlayerFigure(PlayerFigure playerFigure, CityPosition position)
    {
        Debug.Assert(playerFigure != null);

        if (City.EntityLifetimeService.TryCreate(playerFigure, position, out CityEntity entity, CreatePlayerFigureAspects()))
        {
            return entity;
        }

        Debug.LogError("Failed to create player figure entity");
        return null;
    }

    static CityEntityAspect[] CreateCarAspects()
    {
        return new CityEntityAspect[]
        {
            new RigidbodyAspect(),
            new DragInteractableAspect(1),
            new TriggerableAspect(),
            new TriggerCausableAspect(),
            new CarAspect(),
            new CityVisibleAspect(),
            new VisibleDistanceScalerAspect()
        };
    }

    static CityEntityAspect[] CreatePlayerFigureAspects()
    {
        return new CityEntityAspect[]
        {
            new RigidbodyAspect(),
            new DragInteractableAspect(1),
            new PlayerFigureAspect(),
            new TriggerCausableAspect(),
            new CityVisibleAspect(),
            new VisibleDistanceScalerAspect()
        };
    }
}
