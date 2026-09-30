public class ExchangeTransaction : Transaction
{
    public ExchangeTransaction(float deltaMoney, Car fromPlayer, Car toPlayer)
    {
        DeltaMoney = deltaMoney;
        FromPlayer = fromPlayer;
        ToPlayer = toPlayer;
    }

    public float DeltaMoney { get; private set; }
    public Car FromPlayer { get; private set; }
    public Car ToPlayer { get; private set; }
}