using System;
using System.Collections.Generic;

public interface IStorageModule : ILocationsHolder
{
    bool CanStore(Product product);
    ILocation GetEmptyLocation();
    int FreeLocationsCount { get; }
}

public abstract class StorageModuleBase : IStorageModule
{
    public Warehouse Warehouse { get; private set; }
    public StorageModuleBase(Warehouse warehouse)
    {
        Warehouse = warehouse;
    }
    public abstract ILocation GetEmptyLocation();
    public abstract ILocation[] GetLocations();
    public abstract bool CanStore(Product product);
    public abstract int FreeLocationsCount { get; }
}

public abstract class TypeExcludedStorage : StorageModuleBase
{
    private readonly HashSet<Type> excluded;
    public TypeExcludedStorage(Warehouse warehouse,params Type[] excludedTypes) : base(warehouse)
        => excluded = new(excludedTypes);
    public override bool CanStore(Product p)
        => !excluded.Contains(p.GetType());
}

public abstract class TypeRestrictedStorage : StorageModuleBase
{
    private readonly HashSet<Type> allowed;

    public TypeRestrictedStorage(Warehouse warehouse, params Type[] allowedTypes) : base(warehouse)
        => allowed = new(allowedTypes);
    public override bool CanStore(Product p)
        => allowed.Contains(p.GetType());
}