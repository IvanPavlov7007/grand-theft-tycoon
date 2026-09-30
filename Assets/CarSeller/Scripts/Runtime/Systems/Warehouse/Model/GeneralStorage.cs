using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GeneralStorage : TypeExcludedStorage
{
    public Dictionary<Product, FreePositionProductLocation> productLocations = new Dictionary<Product, FreePositionProductLocation>();
    public DimensionalPositionData emptyProductLocation { get; set; }

    public GeneralStorage(Warehouse warehouse) : base(warehouse, typeof(Car)) { }

    public override int FreeLocationsCount => int.MaxValue;

    public override ILocation GetEmptyLocation()
    {
        return new FreePositionProductLocation(this, emptyProductLocation, null);
    }

    public override ILocation[] GetLocations()
    {
        return productLocations.Values.ToArray();
    }

    public class FreePositionProductLocation : ILocation
    {
        public GeneralStorage Storage { get; private set; }
        public Product Product { get; private set; }
        public DimensionalPositionData Position { get; set; }

        public ILocationsHolder Holder => Storage;
        public ILocatable Occupant => Product;

        public FreePositionProductLocation(GeneralStorage storage, DimensionalPositionData position, Product product)
        {
            Product = product;
            Position = position;
            Storage = storage;
        }

        public bool Attach(ILocatable locatable)
        {
            var product = locatable as Product;
            if (product == null)  
            {
                Debug.LogWarning("FreePositionProductLocation: Attempt to attach a non-product locatable to a product location");
                return false;
            }
            if (Product != null)
            { 
                Debug.LogWarning("FreePositionProductLocation: Attempt to attach a product to an occupied product location");
                return false;
            }
            Product = product;
            Storage.productLocations[product] = this;
            return true;
        }
        public void Detach()
        {
            Storage.productLocations.Remove(Product);
            Product = null;
        }
    }

    public struct DimensionalPositionData
    {
        public Vector3 LocalPosition;
        public Vector3 LocalRotation;
    }
}