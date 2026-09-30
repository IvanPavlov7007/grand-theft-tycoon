using System;
using System.Collections.Generic;
using UnityEngine;
public class Warehouse : OwnableBase, IOwnershipContainer, ILocationsHolder, ILocatable, IRegisterable
{
    public WarehouseConfig Config;
    public DimensionalPositionData emptyProductLocation { get; set; }

    private readonly Guid _id = Guid.NewGuid();
    public Guid Id => _id;
    public string Name => Config.Name;

    public string DistrictName => G.City.MarkersById[Config.Marker.MarkerId].RegionId;
    public string SizeCategory => Config.SizeCategory;

    public List<WarehouseProductLocation> productLocations = new List<WarehouseProductLocation>();
    public SuppliesList suppliesList;
    public int OverallCarParkingSpots => Config.CarParkingSpots;
    public int AvailableCarParkingSpots => OverallCarParkingSpots - productLocations.FindAll(p => p.Product is Car).Count;

    public OwnershipResolution OwnershipResolution => OwnershipResolution.OwnerOfContainerIfNull;
    public IOwnable GetOwnerOfContainer() => this.Owner;


    public Warehouse(WarehouseConfig config)
    {
        this.Config = config;
    }

    public ILocation GetEmptyLocation()
    {
        return new WarehouseProductLocation(this,emptyProductLocation, null);
    }

    public ILocation[] GetLocations()
    {
        return productLocations.ToArray();
    }

    public List<Car> GetCars()
    {
        List<Car> cars = new List<Car>();
        foreach (var location in productLocations)
        {
            if (location.Occupant is Car car)
            {
                cars.Add(car);
            }
        }
        return cars;
    }

    public class WarehouseProductLocation : ILocation
    {
        public Warehouse Warehouse { get; private set; }
        public Product Product { get; private set; }
        public DimensionalPositionData Position { get; set; }

        public ILocationsHolder Holder => Warehouse;
        public ILocatable Occupant => Product;

        public WarehouseProductLocation(Warehouse warehouse, DimensionalPositionData position, Product product)
        {
            Product = product;
            Position = position;
            Warehouse = warehouse;
        }

        public bool Attach(ILocatable locatable)
        {
            var product = locatable as Product;
            if(product == null)
            {
                Debug.LogWarning("WarehouseProductLocation: Attempt to attach a non-product locatable to a product location");
                return false;
            }
            if (Product != null)
            {
                 Debug.LogWarning("WarehouseProductLocation: Attempt to attach a product to an occupied product location");
                return false;
            }
            if(ifCarAndNoSpotsAvailable(product))
            {
                return false;
            }

            Product = product;
            Warehouse.productLocations.Add(this);
            return true;
        }

        private bool ifCarAndNoSpotsAvailable(Product product)
        {
            if (product is Car && Warehouse.AvailableCarParkingSpots <= 0)
            {
                Debug.LogWarning("WarehouseProductLocation: Attempt to attach a car product when there are no available parking spots");
                return true;
            }
            return false;
        }
        public void Detach()
        {
            Product = null;
            Warehouse.productLocations.Remove(this);
        }
    }

    public struct DimensionalPositionData
    {
        public Vector3 LocalPosition;
        public Vector3 LocalRotation;
    }
}


