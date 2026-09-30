public interface ITransactionHandler
{
    TransactionResult Handle(Transaction transaction);
}

public abstract class TransactionHandler<T> : ITransactionHandler where T : Transaction
{
    public abstract TransactionResult Handle(T transaction);

    TransactionResult ITransactionHandler.Handle(Transaction transaction)
    {
        return Handle((T)transaction);
    }
}