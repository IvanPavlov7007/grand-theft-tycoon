using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public sealed class Car : Product, IOwnershipContainer, ILocationsHolder, ISimplifiedCarModel
{
    public CarRuntimeConfig runtimeConfig { get; private set; }
    public CarKind Kind => runtimeConfig.Kind;
    public T GetModifier<T>() where T : CarModifier
    {
        return runtimeConfig.GetModifier<T>();
    }

    public bool HasModifier<T>() where T : CarModifier
    {
        return GetModifier<T>() != null;
    }

    public Dictionary<CarPartLocation, PartSlotRuntimeConfig> carParts { get; private set; }
    private FrameProductLocation FrameLocation;

    public CarFrame CarFrame { get; private set; }

    public override string Name => runtimeConfig.Name;

    public override float BasePrice => runtimeConfig.BasePrice;

    public OwnershipResolution OwnershipResolution => OwnershipResolution.Container;
    public IOwnable GetOwnerOfContainer() => this;
    public Car(CarRuntimeConfig runtimeConfig)
    {
        this.runtimeConfig = runtimeConfig;
    }

    public void SetCarFrame(CarFrame carFrame)
    {
        Debug.Assert(CarFrame == null, $"Car {UniqueName}: Car frame has already been set.");
        CarFrame = carFrame;
        FrameLocation = new FrameProductLocation(this, carFrame);
    }

    public void addSlots(Dictionary<CarPartLocation, PartSlotRuntimeConfig> slots)
    {
        Debug.Assert(carParts == null, $"Car {UniqueName}: Car parts have already been set.");
        carParts = slots;
    }

    public CarPartLocation AvailableSlot(Product product)
    {
        foreach (var slotLocation in carParts.Keys)
        {
            if (slotLocation.CanAccept(product))
            {
                return slotLocation;
            }
        }
        return null;
    }

    public bool IsComplete()
    {
        Debug.Assert(carParts != null, $"Car {UniqueName}: Car parts have not been set.");
        foreach (var slotLocation in carParts.Keys)
        {
            if (slotLocation.PartSlotRuntimeConfig.partSlotData.Required && slotLocation.Occupant == null)
            {
                return false;
            }
        }
        return true;
    }

    public ILocation[] GetLocations()
    {
        Debug.Assert(carParts != null, $"Car {UniqueName}: Car parts have not been set.");
        Debug.Assert(CarFrame != null, $"Car {UniqueName}: Car frame has not been set.");

        return carParts.Keys.Cast<ILocation>().Concat(new[] { FrameLocation }).ToArray();
    }

    public override T GetRepresentation<T>(IProductViewBuilder<T> builder)
    {
        return builder.BuildCar(this);
    }

    public class FrameProductLocation : ILocation
    {
        public FrameProductLocation(Car car, CarFrame carFrame)
        {
            Car = car;
            CarFrame = carFrame;
        }

        public Car Car { get; private set; }
        public ILocatable Occupant => CarFrame;
        public ILocationsHolder Holder => Car;
        public CarFrame CarFrame { get; private set; }

        public bool Attach(ILocatable locatable)
        {
            Debug.LogWarning($"Car {Car.UniqueName}: Cannot attach { locatable} to FrameLocation: location is fixed to CarFrame {CarFrame.Name}.");
            return false;
        }

        public void Detach()
        {
            Debug.LogWarning($"Car {Car.UniqueName}: Cannot detach product {(Occupant as CarFrame).Name} from FrameLocation: location is fixed to CarFrame {CarFrame.Name}.");
        }
    }

    /// <summary>
    /// Car Slot-representing instances
    /// </summary>
    public class CarPartLocation : ILocation
    {
        public CarPartLocation(Car car, PartSlotRuntimeConfig partSlotRuntimeConfig, Product product)
        {
            Car = car;
            PartSlotRuntimeConfig = partSlotRuntimeConfig;
            Occupant = product;
        }

        public Car Car { get; private set; }
        public ILocationsHolder Holder => Car;
        public PartSlotRuntimeConfig PartSlotRuntimeConfig { get; private set; }
        public ILocatable Occupant { get; private set; }

        public bool CanAccept(Product product)
        {
            return PartSlotRuntimeConfig.CanAccept(product) && Occupant == null;
        }

        public bool Attach(ILocatable locatable)
        {

            Product product = locatable as Product;

            if(Occupant != null)
            {
                Debug.LogWarning($"Car {Car.UniqueName}: Cannot attach product { product.Name} to CarPartLocation: already occupied by product {(Occupant as Product).Name}.");
                return false;
            }

            if(PartSlotRuntimeConfig.TryAccept(product) == false)
            {
                Debug.LogWarning($"Car {Car.UniqueName}: Cannot attach product { product.Name} to CarPartLocation: incompatible product for slot type {PartSlotRuntimeConfig.SlotType}.");
                return false;
            }
            Occupant = product;
            return true;
        }

        public void Detach()
        {
            PartSlotRuntimeConfig.Detach();
            Occupant = null;
        }
    }

    
}