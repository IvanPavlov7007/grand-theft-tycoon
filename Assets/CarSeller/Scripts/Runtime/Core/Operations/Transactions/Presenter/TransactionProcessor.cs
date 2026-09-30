using System;
using System.Collections.Generic;
using UnityEngine;

public class TransactionProcessor
{
    Dictionary<Type, ITransactionHandler> Handlers { get; set; }

    public TransactionProcessor(Dictionary<Type, ITransactionHandler> handlers)
    {
        Handlers = handlers;
    }

    /// <summary>
    /// Finds handler for the transaction, executes, saves result, and triggers transaction complete event.
    /// </summary>
    /// <param name="transaction"></param>
    /// <param name="location"></param>
    /// <returns></returns>
    public TransactionResult Process(Transaction transaction, TransactionFeedbackLocation location)
    {
        Debug.Assert(Handlers != null, "TransactionProcessor handlers dictionary is null.");

        if (transaction == null)
            return TransactionResult.InvalidTransaction("Transaction is null");

        if (location == null)
        {
            Debug.LogWarning("TransactionProcessor.Process called with null location. Defaulting to OmniDirectional.");
            location = TransactionFeedbackLocation.OmniDirectional;
        }

        TransactionResult result;

        if (Handlers != null &&
            Handlers.TryGetValue(transaction.GetType(), out var handler))
        {
            result = handle(transaction, handler);
        }
        else
        {
            result = TransactionResult.InvalidTransaction(
                $"No handler found for transaction type {transaction.GetType()}");
        }

        transaction.FinalizeResult(result);
        GameEvents.Instance.OnTransactionComplete(new TransactionEventData(transaction, location));

        return result;
    }

    private TransactionResult handle(Transaction transaction, ITransactionHandler handler)
    {
        try
        {
            return handler.Handle(transaction)
                   ?? TransactionResult.InvalidTransaction("Handler returned null result.");
        }
        catch(Exception ex)
        {
            Debug.LogError($"Exception while processing transaction of type {transaction.GetType()}: {ex}");
            return TransactionResult.InvalidTransaction("An error occurred while processing the transaction.");
        }
    }

    public TransactionResult Process(Transaction transaction)
    {
        return Process(transaction, TransactionFeedbackLocation.OmniDirectional);
    }
}