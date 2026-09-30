using System;

public class ExchangeHandler : TransactionHandler<ExchangeTransaction>
{
    public override TransactionResult Handle(ExchangeTransaction transaction)
    {
        throw new NotImplementedException();

        if (transaction.FromPlayer == null)
            return TransactionResult.InvalidTransaction("No car specified to give in exchange.");

        if (transaction.ToPlayer == null)
            return TransactionResult.InvalidTransaction("No car specified to receive in exchange.");

        // Build result first
        var result = TransactionResult.Success();

        // Side effects only if successful
        if (result.Type == TransactionResultType.Success)
        {
            G.PlayerManager.DeltaPlayerMoney(transaction.DeltaMoney);
            G.ProductLifetimeService.SwapProducts(transaction.FromPlayer,
                transaction.ToPlayer);
        }

        return result;
    }
}