using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;



public class DeployStashSessionDisplayer : StashSessionDisplayer
{
    public DeployStashSessionDisplayer(IReadOnlyList<StashSlot> shownSlots, Action<StashSlot> onSlotSelected) : base(shownSlots, onSlotSelected)
    {
    }
    protected override Widget createSlotElement(StashSlot slot, Action<StashSlot> action)
    {
        return createDeploySlotElement(slot, action);
    }
}

public class StoreStashSessionDisplayer : StashSessionDisplayer
{
    public StoreStashSessionDisplayer(IReadOnlyList<StashSlot> shownSlots, Action<StashSlot> onSlotSelected) : base(shownSlots, onSlotSelected)
    {
    }
    protected override Widget createSlotElement(StashSlot slot, Action<StashSlot> action)
    {
        return createStoreSlotElement(slot, action);
    }
}

public abstract class StashSessionDisplayer
{
    IReadOnlyList<StashSlot> ShownSlots;
    Action<StashSlot> onSlotSelected;

    public StashSessionDisplayer(IReadOnlyList<StashSlot> shownSlots, Action<StashSlot> onSlotSelected)
    {
        ShownSlots = shownSlots;
        this.onSlotSelected = onSlotSelected;
    }

    public Widget generateOffers()
    {
        var container = new VerticalContentWidget(CarStashWarehouse.DisplayName);
        populateContainerWithSlots(container, ShownSlots, onSlotSelected);
        return container;
    }

    protected void populateContainerWithSlots(Widget container, IReadOnlyList<StashSlot> slots, Action<StashSlot> onClickAction)
    {
        foreach(var slot in slots)
        {
            container.Children.Add(createSlotElement(slot, onClickAction));
        }
    }

    protected abstract Widget createSlotElement(StashSlot slot, Action<StashSlot> action);

    protected Widget createDeploySlotElement(StashSlot slot, Action<StashSlot> action)
    {
        return CarStashEntryWidgetCreator.CreateDeployEntry(slot.Car, () => action(slot));
    }

    protected Widget createStoreSlotElement(StashSlot slot, Action<StashSlot> action)
    {
        if (slot.Car != null)
        {
            return createSwapStoreSlotElement(slot, action);
        }
        else
        {
            return createEmptyStoreSlotElement(slot, action);
        }
    }

    protected Widget createEmptyStoreSlotElement(StashSlot slot, Action<StashSlot> action)
    {
        return CarStashEntryWidgetCreator.CreateEmptyStoreEntry(() => action(slot));
    }

    protected Widget createSwapStoreSlotElement(StashSlot slot, Action<StashSlot> action)
    {
        return CarStashEntryWidgetCreator.CreateSwapStoreEntry(slot.Car, () => action(slot));
    }
}

public class PickToDeployCarStashSession : CarStashSession
{
    public readonly IReadOnlyList<StashSlot> AvailableStashSlots;
    public readonly CityPosition EntrancePosition;

    public PickToDeployCarStashSession(CityPosition entrancePosition,CarStashWarehouse warehouse) : base(warehouse)
    {
        EntrancePosition = entrancePosition;
        AvailableStashSlots = stashSlots.Where(s => s.Occupant != null).ToList();
    }

    public override StashSessionDisplayer GetDisplayer(Action<StashSlot> slotSelectedCallback)
    {
        var list = AvailableStashSlots.ToList();
        return new DeployStashSessionDisplayer(list, slotSelectedCallback);
    }

    public override CarStashSession MakeChoice(StashSlot choice)
    {
        Debug.Assert(AvailableStashSlots.Contains(choice), "Attempted to make a pick-to-deploy choice with a stash slot that is not available.");
        var vehicle =choice.Deploy(EntrancePosition);
        G.VehicleController.DriveWorldVehicle(vehicle);
        return null;
    }
}

public class PickToStoreCarStashSession : CarStashSession
{
    public readonly CityEntity CarOnEntrance;
    public readonly IReadOnlyList<StashSlot> slotsToSwapWithCar;
    public readonly IReadOnlyList<StashSlot> emptySlots;

    public PickToStoreCarStashSession(CityEntity carOnEntrance, CarStashWarehouse warehouse) : base(warehouse)
    {
        slotsToSwapWithCar = stashSlots.Where(s => s.Occupant != null).ToList();
        emptySlots = stashSlots.Where(s => s.Occupant == null).ToList();
        CarOnEntrance = carOnEntrance;
    }

    public override StashSessionDisplayer GetDisplayer(Action<StashSlot> slotSelectedCallback)
    {
        List<StashSlot> selection = new List<StashSlot>();
        selection.AddRange(slotsToSwapWithCar);
        selection.AddRange(emptySlots);

        return new StoreStashSessionDisplayer(selection, slotSelectedCallback);
    }

    public override CarStashSession MakeChoice(StashSlot choice)
    {
        Debug.Assert(slotsToSwapWithCar.Contains(choice) || emptySlots.Contains(choice), "Attempted to make a pick-to-store choice with a stash slot that is not available.");
        if (slotsToSwapWithCar.Contains(choice))
        {
            CityEntity swappedCar = choice.Deploy(CarOnEntrance.Position);
            choice.Hide(CarOnEntrance);
            G.VehicleController.DriveWorldVehicle(swappedCar);
            return CreatePickToStoreSession(Warehouse, swappedCar);
        }
        else
        {
            choice.Hide(CarOnEntrance);
            G.VehicleController.ExitWorldVehicle();
            return null;
        }
    }
}

public abstract class CarStashSession
{
    public readonly CarStashWarehouse Warehouse;
    public readonly IReadOnlyList<StashSlot> stashSlots;
    public CarStashSession(CarStashWarehouse warehouse)
    {
        Warehouse = warehouse;
        stashSlots = warehouse.Stashes;
    }

    public static PickToDeployCarStashSession CreatePickToDeploySession(CarStashWarehouse warehouse, CityPosition entrancePosition)
    {
        return new PickToDeployCarStashSession(entrancePosition, warehouse);
    }

    public static PickToStoreCarStashSession CreatePickToStoreSession(CarStashWarehouse warehouse, CityEntity carOnEntrance)
    {
        return new PickToStoreCarStashSession(carOnEntrance, warehouse);
    }

    public abstract StashSessionDisplayer GetDisplayer(Action<StashSlot> slotSelectedCallback);

    public abstract CarStashSession MakeChoice(StashSlot choice);

}

public class CarStashWarehouse : ILocationsHolder,ILocatable
{
    public CarStashWarehouseConfig Config;

    public IReadOnlyList<StashSlot> Stashes => stashes.AsReadOnly();

    List<StashSlot> stashes;
    public int MaxCount { get; private set; }
    public static string DisplayName { get; internal set; } = "Stolen cars warehouse";

    public CarStashWarehouse(CarStashWarehouseConfig config)
    {
        Debug.Assert(config != null, "Attempted to create a CarStashWarehouse with null config.");

        MaxCount = config.maxCount;
        Debug.Assert(config.initiallyFilledCars.Count <= MaxCount,
            $"Attempted to create a CarStashWarehouse with {config.initiallyFilledCars.Count} filled stashes, but the max count is only {MaxCount}.");
        
        stashes = new List<StashSlot>();
        InitializeFilledStashes(config.initiallyFilledCars);
        InitializeEmptyStashes(MaxCount - stashes.Count);
        Config = config;
    }

    private void InitializeFilledStashes(List<SimpleCarSpawnConfig> filledCars)
    {
        foreach(var carConfig in filledCars)
        {
            stashes.Add(StashSlot.CreateFilledStash(this, carConfig));
        }
    }

    private void InitializeEmptyStashes(int count)
    {
        for(int i = 0; i < count; i++)
        {
            stashes.Add(StashSlot.CreateEmptyStash(this));
        }
    }

    public ILocation[] GetLocations()
    {
        return stashes.ToArray();
    }

}

public class StashSlot : ILocation
{
    public ILocatable Occupant => Car;

    public ILocationsHolder Holder => Warehouse;

    public Car Car { get; private set; }
    public CarStashWarehouse Warehouse { get; private set; }


    private StashSlot(CarStashWarehouse warehouse)
    {
        Warehouse = warehouse;
    }
    public static StashSlot CreateFilledStash(CarStashWarehouse carStash, SimpleCarSpawnConfig carConfig)
    {
        var location = new StashSlot(carStash);
        var car = carConfig.GenerateCar(location);
        return location;
    }

    public static StashSlot CreateEmptyStash(CarStashWarehouse carStash)
    {
        var location = new StashSlot(carStash);
        return location;
    }

    public CityEntity Deploy(CityPosition position)
    {
        CityEntity enity = CityEntitiesCreationHelper.MoveInExistingCar(Car, position);
        return enity;
    }

    public bool Hide(CityEntity entity)
    {
        Debug.Log($"Hiding car {entity} in stash slot at warehouse {Warehouse.Config.name} with subject of {entity.Subject}.");
        return G.ProductLifetimeService.MoveProduct(entity.Subject as Car, this);
    }

    public bool Attach(ILocatable product)
    {
        Debug.Assert(product is Car);
        if (Car != null)
        {
            Debug.LogError("Attempted to attach a car to a stash location that already has a car.");
            return false;
        }
        Car = (Car)product;
        return true;
    }

    public void Detach()
    {
        Car = null;
    }
}