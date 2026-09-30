using UnityEngine;

public class Transaction
{
    internal void FinalizeResult(TransactionResult result)
    {
        Result = result;
    }
    public TransactionResult Result { get; private set; }
}
public class TransactionResult
{
    public TransactionResultType Type { get; private set; }
    public ITransactionResultData Data { get; private set; }
    public TransactionResult(TransactionResultType type, ITransactionResultData data = null)
    {
        Type = type;
        Data = data;
    }

    public static TransactionResult Success(ITransactionResultData data = null)
    {
        return new TransactionResult(TransactionResultType.Success, data);
    }

    public static TransactionResult InvalidTransaction(string text)
    {
        return new TransactionResult(TransactionResultType.InvalidTransaction, data: new MessageTransactionResultData(text));
    }

}

public class TransactionFeedbackLocation
{
    public static readonly TransactionFeedbackLocation OmniDirectional = new TransactionFeedbackLocation(TransactionLocationType.OmniDirectional, Vector3.zero);

    public TransactionFeedbackLocation(TransactionLocationType type, Vector3 position)
    {
        Type = type;
        Position = position;
    }
    public TransactionLocationType Type { get; private set; }
    public Vector3 Position { get; private set; }
}

public enum TransactionLocationType
{
    OmniDirectional,
    ScreenSpace,
    WorldSpace
}

public enum TransactionResultType
{
    Success,
    Failure,
    InvalidTransaction,
} 

public interface ITransactionResultData { }

public class MessageTransactionResultData : ITransactionResultData
{
    public string Message { get; private set; }
    public MessageTransactionResultData(string message)
    {
        Message = message;
    }
}