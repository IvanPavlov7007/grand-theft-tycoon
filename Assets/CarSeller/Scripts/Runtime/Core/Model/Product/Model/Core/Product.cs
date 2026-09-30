using System;

/// <summary>
/// A data representation of a product instance in the game
/// </summary>
public abstract class Product : OwnableBase, IProductRepresentationProvider, ILocatable, IRegisterable
{
    private readonly Guid _id = Guid.NewGuid();
    public Guid Id => _id;

    public abstract string Name { get; }
    public abstract float BasePrice { get; }

    public string UniqueName => Name + "_" + Id.ToString();

    public abstract T GetRepresentation<T>(IProductViewBuilder<T> builder);
}

public interface IProductRepresentationProvider
{
    public abstract T GetRepresentation<T>(IProductViewBuilder<T> builder);
}

