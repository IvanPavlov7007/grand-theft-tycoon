using System;
using UnityEngine;

[CreateAssetMenu(fileName = "CarShopOffersConfig", menuName = "Configs/Economy/CarShopOffersConfig", order = 1)]
public class CarShopOffersConfig : ScriptableObject
{
    public CarShopOfferEntry[] offerConfigs;
}

[Serializable]
public class CarShopOfferEntry
{
    public SimpleCarSpawnConfig carSpawnConfig;
}