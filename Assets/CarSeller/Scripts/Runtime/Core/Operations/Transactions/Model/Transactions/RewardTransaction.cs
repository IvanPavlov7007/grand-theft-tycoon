public class RewardTransaction : Transaction
{
    public RewardTransaction(float price, IPurchasable[] items)
    {
        Price = price;
        Items = items;
    }
    public float Price { get; private set; }
    public IPurchasable[] Items { get; private set; }
}