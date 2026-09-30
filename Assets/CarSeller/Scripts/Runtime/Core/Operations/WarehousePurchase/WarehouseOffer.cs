using UnityEngine;

public class WarehouseOffer : IOffer
{
    WarehouseOfferProvider provider;

    public WarehouseOffer(WarehouseOfferProvider provider, Warehouse warehouse, float price)
    {
        this.provider = provider;
        Warehouse = warehouse;
        Price = price;
    }

    public Warehouse Warehouse { get; private set; }
    public float Price { get; private set; }

    public bool IsAccepted { get; private set; } = false;

    public Transaction Accept()
    {
        Debug.Assert(!IsAccepted);
        if(!CanAccept())
        {
            Debug.LogError("Cannot accept offer: insufficient funds.");
            return null;
        }

        IsAccepted = true;

        throw new System.NotImplementedException("Transaction creation not implemented yet.");
        //var purchaseData = new PurchaseTransactionData(Price, new IMutableOwnable[1] { Warehouse});
        //Transaction transaction = new Transaction(TransactionType.Purchase, purchaseData);

        provider.OfferAccepted(this);
    }

    public bool CanAccept()
    {
        Debug.Assert(!IsAccepted);
        return World.Instance.Economy.Player.Money >= Price;
    }
}