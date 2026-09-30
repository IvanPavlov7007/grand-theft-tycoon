using System.Collections.Generic;

public class SuppliesList : ILocationsHolder
{
    public List<SupplyProductLocation> supplies;

    public ILocation[] GetLocations()
    {
        return supplies.ToArray();
    }

    public class SupplyProductLocation : ILocation
    {
        public SupplyProductLocation(SuppliesList suppliesList, Supply supply, Product product)
        {
            SuppliesList = suppliesList;
            Supply = supply;
            Product = product;
        }

        public SuppliesList SuppliesList { get; private set; }
        public Supply Supply { get; private set; }
        public Product Product { get; private set; }
        public ILocatable Occupant => Product;

        public ILocationsHolder Holder => SuppliesList;

        public bool Attach(ILocatable Occupant)
        {
            var product = Occupant as Product;

            if (Product != null)
                return false;
            Product = product;
            SuppliesList.supplies.Add(this);
            return true;
        }

        public void Detach()
        {
            Product = null;
            SuppliesList.supplies.Remove(this);
        }
    }
}