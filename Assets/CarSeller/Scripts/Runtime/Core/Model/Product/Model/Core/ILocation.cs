using System.Linq;

public interface ILocatable { }

public interface ILocation
{
    // Usual structure:
    // 1) location
    // 2) position in the local spacial system
    // 3) product
    ILocatable Occupant { get; }
    ILocationsHolder Holder { get; }

    bool Attach(ILocatable product);
    void Detach();
}

public interface ILocationsHolder
{
    ILocation[] GetLocations();
}

public static class ProductsHolderExtensions
{
    public static ILocation[] GetNonEmptyProductLocations(this ILocationsHolder holder)
        => holder.GetLocations().Where(l => l.Occupant != null).ToArray();
}