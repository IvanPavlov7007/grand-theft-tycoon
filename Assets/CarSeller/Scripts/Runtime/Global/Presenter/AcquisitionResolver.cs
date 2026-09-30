
using System.Collections.Generic;
using UnityEngine;

public class AcquisitionResolver
{
    //public void Resolve(IMutableOwnable[] purchasedItems)
    //{
    //    foreach (var item in purchasedItems)
    //    {
    //        Resolve(item);
    //    }
    //}

    public void Resolve(IReadOnlyList<IPurchasable> purchasables)
    {
        foreach(var purchasable in purchasables)
        {
            purchasable.CompletePurchase();
        }
    }

    public void Resolve(IMutableOwnable purchasedItem)
    {
        switch(purchasedItem)
        {
            case Warehouse warehouse:
                // When purchasing a warehouse, set its owner to the player.
                G.OwnershipService.TransferOwnership(warehouse, G.Player);
                break;

            case Car car:
            case Product product:
            default:
                Debug.LogWarning($"PurchaseResolver: No ownership resolution defined for purchased item of type {purchasedItem.GetType().Name}");
                break;
        }
    }
}