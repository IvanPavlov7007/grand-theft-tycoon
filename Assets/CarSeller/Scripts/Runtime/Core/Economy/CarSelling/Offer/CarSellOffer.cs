using UnityEngine;

/// <summary>
/// On-demand created offer to sell a car.
/// </summary>
public class CarSellOffer : IOffer
{
    public Car Car { get; private set; }
    public float InitialOfferPrice { get; private set; }
    public float FinalRewardPrice { get; private set; }

    public bool Accepted { get; private set; } = false;


    public CarSellOffer(
        Car car
    )
    {
        Car = car;
        InitialOfferPrice = CalculatePrice();
    }

    private float CalculatePrice()
    {
        return G.Economy.ProductPriceCalculator.Calculate(Car);
    }

    //TODO in the future add context to check if the offer can be accepted
    //and modify price based on that context

    public Transaction Accept()
    {
        Debug.Assert(!Accepted);
        Debug.Assert(CanAccept(), "Cannot accept offer: car not delivered.");

        FinalRewardPrice = CalculatePrice();
        throw new System.NotImplementedException();
        //Transaction transaction = new SellTransaction(Car, FinalRewardPrice);

        Accepted = true;

        //return transaction;
    }

    public bool CanAccept()
    {
        return !Accepted;
    }
}