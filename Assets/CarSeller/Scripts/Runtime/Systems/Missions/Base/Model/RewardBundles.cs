using System;

[Serializable]
public class CurrencyRewardBundle : RewardBundle
{
    public int amount;
    public override Transaction CreateTransaction()
    {
        return new RewardTransaction(amount, null);
    }

    public override string GetRewardDescription()
    {
        return TextConventionsHelper.FormatPrice(amount);
    }
}