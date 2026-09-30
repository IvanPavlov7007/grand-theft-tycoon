public class SellTransaction : Transaction
{
    public SellTransaction(Car car, Buyer buyer, SellPriceWrapper sellPriceWrapper)
    {
        Car = car;
        Buyer = buyer;

        UnitPrice = sellPriceWrapper.CalculateUnitSellPrice(car, buyer);
        AbsolutePrice = SellPriceWrapper.CalculateAbsolutePrice(UnitPrice);
    }

    public readonly Buyer Buyer;
    public readonly Car Car;
    public readonly float UnitPrice;
    public readonly float AbsolutePrice;
}