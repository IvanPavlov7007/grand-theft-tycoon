public class SellHandler : TransactionHandler<SellTransaction>
{
    public override TransactionResult Handle(SellTransaction transaction)
    {
        G.PlayerManager.AddPlayerMoney(transaction.UnitPrice);
        G.AreaProgressionManager.ProgressCarSale(transaction);
        City.EntityLifetimeService.Destroy(transaction.Car);
        City.EntityLifetimeService.Destroy(transaction.Buyer);

        return TransactionResult.Success();

        //if (result == null && sellData == null)
        //{
        //    result = TransactionResult.InvalidTransaction("Invalid data: expected SellTransactionData.");
        //}

        //var player = World.Instance.Economy.Player;
        //if (result == null && player == null)
        //{
        //    result = TransactionResult.InvalidTransaction("Player not found.");
        //}

        //var car = sellData?.Car;

        //if (result == null && car == null)
        //{
        //    result = TransactionResult.InvalidTransaction("No car specified for sale.");
        //}

        //if (result == null && !player.Owns(car))
        //{
        //    result = TransactionResult.InvalidTransaction("Player does not own the car being sold.");
        //}

        //if (result == null)
        //{
        //    result = TransactionResult.Success();
        //}

        //if (result.Type == TransactionResultType.Success)
        //{
        //    G.PlayerManager.AddPlayerMoney(sellData.Price);
        //    G.ProductLifetimeService.DestroyProduct(car);
        //}
        //return result;
    }
}