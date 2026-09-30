public class TransactionEventData : GameEventData
{
    public TransactionEventData(Transaction transaction, TransactionFeedbackLocation transactionFeedbackLocation)
    {
        this.Transaction = transaction;
        TransactionFeedbackLocation = transactionFeedbackLocation;
    }
    public Transaction Transaction{ get; private set; }
    public TransactionFeedbackLocation TransactionFeedbackLocation { get; private set; }
}
