using UnityEngine;

/// <summary>
/// General service to manage product locations. Should be used as the only tool to move products between locations.
/// </summary>
public class ProductLifetimeService
{
    private readonly ProductLocationService locationService = new ProductLocationService();
    OwnershipResolutionService ownershipService => G.OwnershipService;

    public void RegisterProduct(Product product, ILocation location)
    {
        locationService.RegisterProductLocation(product, location);
        ownershipService.RegisterOwnable(product);
        ownershipService.tryResolveOwnership(product, location);
    }

    public bool MoveProduct(Product product, ILocation newLocation)
    {
        bool result = locationService.MoveProduct(product, newLocation);

        if (result)
        {
            result = ownershipService.tryResolveOwnership(product, newLocation);
        }

        return result;
    }

    public void TransferOwnership(IMutableOwnable item, IOwnable newOwner)
    {
        ownershipService.TransferOwnership(item, newOwner);
    }

    public ILocation GetProductLocation(Product product) => locationService.GetProductLocation(product);

    public bool SwapProducts(Product productA, Product productB)
    {
        // Keep location swap but ownership must also be resolved after swap based on new holders.
        var locA = GetProductLocation(productA);
        var locB = GetProductLocation(productB);

        var ok = locationService.SwapProducts(productA, productB);
        if (!ok)
            return false;

        ownershipService.tryResolveOwnership(productA, locB);
        ownershipService.tryResolveOwnership(productB, locA);
        return true;
    }

    public void DestroyProduct(Product product)
    {
        var location = GetProductLocation(product);
        Debug.Assert(location != null, $"Trying to delete product {product?.Name} that has no location.");

        ownershipService.ResolveOnRemoval(product);
        locationService.RemoveProduct(product);

        Debug.Assert(product != null, "Trying to delete a null product.");
        GameEvents.Instance.OnProductDestroyed?.Invoke(new ProductDestroyedEventData(product));
        Debug.Assert(product is ILocatable);
        GameEvents.Instance.OnLocatableDestroyed?.Invoke(new LocatableDestroyedEventData(product));
    }
}