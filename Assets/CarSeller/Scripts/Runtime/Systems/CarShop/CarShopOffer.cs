using UnityEngine;

public class CarShopOffer : IOffer
{
    public Car fromPlayer { get; private set; }
    public Car toPlayer { get; private set; }

    public float MoneyDelta;

    public bool Accepted { get; private set; } = false;

    public CarShopOffer(
        Car fromPlayer,
        Car toPlayer,
        float moneyDelta
    )
    {
        this.fromPlayer = fromPlayer;
        this.toPlayer = toPlayer;
        MoneyDelta = moneyDelta;
    }
    //TODO in the future add context to check if the offer can be accepted
    //and modify price based on that context
    public Transaction Accept()
    {
        Debug.Assert(!Accepted);
        Debug.Assert(CanAccept(), "Cannot accept offer: car not available.");
        Transaction transaction = new ExchangeTransaction(MoneyDelta,fromPlayer, toPlayer);
        Accepted = true;
        return transaction;
    }
    public bool CanAccept()
    {
        return !Accepted && G.Player.Money + MoneyDelta >= 0f;
    }
}