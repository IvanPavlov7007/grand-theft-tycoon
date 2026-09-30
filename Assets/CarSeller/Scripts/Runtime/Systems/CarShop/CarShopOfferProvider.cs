using Sirenix.Utilities;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Car Shop offers depend on the given car as well,
/// so offers created on demand when requested.
/// </summary>
public class CarShopOfferProvider
{
    List<Car> carOptions;

    public CarShopOfferProvider(List<Car> carOptions)
    {
        this.carOptions = carOptions;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="fromPlayer"></param>
    /// <param name="carOptionsToExchangeTo"></param>
    /// <param name="commissionMoney"> money player pays additionally for transaction </param>
    /// <returns></returns>
    public CarShopOffer[] GetOffers(Car fromPlayer, float commissionMoney)
    {
        CarShopOffer[] offers = new CarShopOffer[carOptions.Count];
        for(int i = 0; i < carOptions.Count; i++)
        {
            Car toPlayer = carOptions[i];
            float moneyDelta = G.Economy.ProductPriceCalculator.Calculate(fromPlayer) - G.Economy.ProductPriceCalculator.Calculate(toPlayer) - commissionMoney;
            offers[i] = new CarShopOffer(fromPlayer, toPlayer, moneyDelta);
     
        }

        offers.Sort((a, b) => b.MoneyDelta.CompareTo(a.MoneyDelta));

        return offers;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="fromPlayer"> car the player gives into pool</param>
    /// <param name="toPlayer">car the player gets from the pool</param>
    public void SwapCar(Car fromPlayer, Car toPlayer)
    {
        Debug.Assert(carOptions.Contains(toPlayer));
        Debug.Assert(!carOptions.Contains(fromPlayer));

        carOptions.Remove(toPlayer);
        carOptions.Add(fromPlayer);
    }
}
