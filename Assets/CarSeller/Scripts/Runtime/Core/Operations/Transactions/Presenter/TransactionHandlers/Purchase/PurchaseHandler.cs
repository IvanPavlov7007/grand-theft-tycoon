using UnityEngine;

public class PurchaseHandler : TransactionHandler<PurchaseTransaction>
{
    public override TransactionResult Handle(PurchaseTransaction transaction)
    {
        Debug.Assert(transaction != null, "PurchaseHandler.Handle: transaction is null.");
        Debug.Assert(transaction.Purchases != null && transaction.Purchases.Count > 0, "PurchaseHandler.Handle: transaction purchases list is null or empty.");
        var player = World.Instance.Economy.Player;
        Debug.Assert(player != null, "PurchaseHandler.Handle: Player is null.");

        G.PlayerManager.SubtractPlayerMoney(transaction.Price);
        G.AcquisitionResolver.Resolve(transaction.Purchases);

        return TransactionResult.Success();
    }
}