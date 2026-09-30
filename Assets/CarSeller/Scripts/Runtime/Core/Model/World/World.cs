using System.Collections.Generic;

public class World : IDisposable
{
    public WorldRegistry WorldRegistry{ get; private set; } = new WorldRegistry();

    public HiddenSpace HiddenSpace = new HiddenSpace();

    // TODO is it also kind of a registry? Should it be moved to WorldRegistry?
    public Dictionary<Product, ILocation> productLocations = new Dictionary<Product, ILocation>();
    public Dictionary<ILocatable, ILocation> allLocations = new Dictionary<ILocatable, ILocation>();
    internal Dictionary<IOwnable, HashSet<IOwnable>> ownerships = new Dictionary<IOwnable, HashSet<IOwnable>>();

    public City City;
    public Economy Economy;

    public static World Instance = new World();
    internal PersonalVehicleShop VehicleShop;

    public static World Reset()
    {
        Instance?.Dispose();
        Instance = new World();
        return Instance;
    }

    public void Dispose()
    {
        City?.Dispose();
    }
}

public interface IDisposable
{
    public void Dispose();
}


/// <summary>
/// A place for things that exist in the world, but are not stored in a city/warehouse/etc.
/// </summary>
public class HiddenSpace : ILocationsHolder
{
    public List<HiddenSpaceLocation> hiddenLocations = new List<HiddenSpaceLocation>();

    public HiddenSpaceLocation GetEmptyLocation()
    {
        var location = new HiddenSpaceLocation();
        return location;
    }

    public ILocation[] GetLocations()
    {
        return hiddenLocations.ToArray();
    }

    public class HiddenSpaceLocation : ILocation
    {
        public ILocatable Occupant { get; private set; }

        public ILocationsHolder Holder => World.Instance.HiddenSpace;

        public bool Attach(ILocatable locatable)
        {
            if (locatable == null)
                return false;
            Occupant = locatable;
            World.Instance.HiddenSpace.hiddenLocations.Add(this);
            return true;
        }

        public void Detach()
        {
            if (Occupant == null)
                return;
            Occupant = null;
            World.Instance.HiddenSpace.hiddenLocations.Remove(this);
        }
    }
}