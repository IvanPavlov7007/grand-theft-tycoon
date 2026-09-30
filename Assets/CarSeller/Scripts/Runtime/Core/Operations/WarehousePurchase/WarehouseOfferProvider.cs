using System.Collections.Generic;
using System.Linq;

public class WarehouseOfferProvider : IOfferProvider<WarehouseOffer>
{
    Dictionary<Warehouse, WarehouseOffer> offers;

    public void Initialize(Dictionary<Warehouse, WarehouseOffer> offers)
    {
        this.offers = offers;
    }

    public void OfferAccepted(WarehouseOffer offer)
    {
        offers.Remove(offer.Warehouse);
    }

    public WarehouseOffer GetOfferForWarehouse(Warehouse warehouse)
    {
        if (offers.TryGetValue(warehouse, out var offer))
        {
            return offer;
        }
        return null;
    }

    List<WarehouseOffer> IOfferProvider<WarehouseOffer>.GetOffers()
    {
        return offers.Values.ToList<WarehouseOffer>();
    }
}