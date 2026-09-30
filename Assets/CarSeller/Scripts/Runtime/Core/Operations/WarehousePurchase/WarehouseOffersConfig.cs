using System;
using UnityEngine;

[CreateAssetMenu(fileName = "WarehouseOffersConfig", menuName = "Configs/Economy/Warehouse Offers Config")]
public class WarehouseOffersConfig : ScriptableObject
{
    public WarehouseOfferConfig[] Offers;
}

[Serializable]
public struct WarehouseOfferConfig
{
    public WarehouseConfig Warehouse;
    public float Price;
}