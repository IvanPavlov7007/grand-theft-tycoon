using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// General service to manage product locations. Should be used as the only tool to move products between locations.
/// </summary>
public class ProductLocationService
{
    Dictionary<Product, ILocation> productLocations => World.Instance.productLocations;

    public void RegisterProductLocation(Product product, ILocation location)
    {
        Debug.Assert(product != null, "Product cannot be null when registering a location.");
        Debug.Assert(location != null, "Location cannot be null when registering a product.");
        Debug.Assert(!productLocations.ContainsKey(product), $"Product {product.Name} is already registered with a location.");

        productLocations[product] = location;
    }

    public bool MoveProduct(Product product, ILocation newLocation)
    {
        var previousLocation = GetProductLocation(product);

        if (newLocation.Attach(product))
        {
            if (previousLocation != null)
            {
                previousLocation.Detach();
            }
            updateSystems(product, newLocation, previousLocation);
            return true;
        }

        return false;
    }

    public bool SwapProducts(Product productA, Product productB)
    {
        Debug.Assert(productA != null);
        Debug.Assert(productB != null);

        var locationA = GetProductLocation(productA);
        var locationB = GetProductLocation(productB);

        Debug.Assert(locationA != null);
        Debug.Assert(locationB != null);

        locationA.Detach();
        locationB.Detach();

        if (!locationB.Attach(productA))
        {
            Debug.LogError($"Couldn't attach {productA} to {locationB}");
            return false;
        }
        if (!locationA.Attach(productB))
        {
            Debug.LogError($"Couldn't attach {productB} to {locationA}");
            return false;
        }

        updateSystems(productA, locationB, locationA);
        updateSystems(productB, locationA, locationB);
        return true;
    }

    private void updateSystems(Product product, ILocation newLocation, ILocation previousLocation)
    {
        productLocations[product] = newLocation;
        GameEvents.Instance.OnProductLocationChanged?.Invoke(new ProductLocationChangedEventData(product, newLocation, previousLocation));
        Debug.Assert(product is ILocatable);
        GameEvents.Instance.OnLocatableLocationChanged?.Invoke(new LocatableLocationChangedEventData(product, newLocation, previousLocation));
    }

    public void RemoveProduct(Product product)
    {
        if (productLocations.TryGetValue(product, out ILocation location))
        {
            location.Detach();
            productLocations.Remove(product);
            GameEvents.Instance.OnProductLocationChanged?.Invoke(new ProductLocationChangedEventData(product, null, location));
            Debug.Assert(product is ILocatable);
            GameEvents.Instance.OnLocatableLocationChanged?.Invoke(new LocatableLocationChangedEventData(product, null, location));
        }
    }

    public ILocation GetProductLocation(Product product)
    {
        if (productLocations.TryGetValue(product, out ILocation location))
        {
            return location;
        }
        return null;
    }
}