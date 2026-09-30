using UnityEngine;

/// <summary>
/// Simple provider that creates one-time car sell offers on demand.
/// </summary>
public class CarSellOneTimeOfferProvider
{
    public CarSellOffer GetOfferByCar(Car car)
    {
        Debug.Assert(car != null, "GetOfferByCar called with null car.");
        Debug.Assert(G.Player.Owns(car));

        return new CarSellOffer(car);
    }
}