using System.Collections.Generic;
using UnityEngine;

public class StripCarHandler : TransactionHandler<StripCarTransaction>
{
    public override TransactionResult Handle(StripCarTransaction transaction)
    {
        Debug.Assert(transaction != null, "Transaction is null.");
        Debug.Assert(transaction.Car != null, "Car is null.");
        Debug.Assert(transaction.TargetProductsHolder != null, "TargetProductsHolder is null.");
        Debug.Assert(transaction.StrippingProcess != null, "StrippingProcess is null.");


        var process = transaction.StrippingProcess;
        process.Strip();

        var resultData = new StripResultData(addStrippedParts(process.StrippedParts, transaction.TargetProductsHolder));

        // Build result first
        var result = TransactionResult.Success(resultData);
        return result;
    }

    private ProductsPutInsideResult addStrippedParts(IReadOnlyList<Product> strippedParts, ITargetProductsHolder targetProductsHolder)
    {
        return targetProductsHolder.PutProducts(strippedParts);
    }
}

public class StripResultData : ITransactionResultData
{
    public readonly ProductsPutInsideResult PutInsideResult;
    public StripResultData(ProductsPutInsideResult putInsideResult)
    {
        PutInsideResult = putInsideResult;
    }
}