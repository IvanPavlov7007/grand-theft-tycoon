using UnityEngine;

public class RewardHandler : TransactionHandler<RewardTransaction>
{

    public override TransactionResult Handle(RewardTransaction transaction)
    {
        Debug.Assert(transaction != null, "RewardHandler.Handle: transaction is null.");
        Debug.Assert(transaction.Price > 0, "RewardHandler.Handle: transaction price is not positive.");
        // Build result first
        var result = TransactionResult.Success();

        G.PlayerManager.AddPlayerMoney(transaction.Price);

        if (transaction.Items != null && transaction.Items.Length > 0)
            G.AcquisitionResolver.Resolve(transaction.Items);

        return TransactionResult.Success();
    }
}