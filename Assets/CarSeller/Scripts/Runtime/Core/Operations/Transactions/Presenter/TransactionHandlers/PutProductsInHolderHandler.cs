using UnityEngine;

public class PutProductsInHolderHandler : TransactionHandler<PutProductsInHolderTransaction>
{
    public override TransactionResult Handle(PutProductsInHolderTransaction transaction)
    {
        Debug.Assert(transaction != null, "transaction is null");
        var products = transaction.Products;
        var targetHolder = transaction.TargetHolder;
        
        var resultData = new ProductsPlacingResultData(targetHolder.PutProducts(products));
        TransactionResult result;
        if (resultData.PuttingResult.putInsideProducts.Count > 0)
        {
            result = TransactionResult.Success(resultData);
        }
        else // couldn't find space in the warehouse
        {
            result = new TransactionResult(TransactionResultType.Failure,resultData);
        }
        return result;
    }
}

public class ProductsPlacingResultData : ITransactionResultData
{
    public readonly ProductsPutInsideResult PuttingResult;

    public ProductsPlacingResultData(ProductsPutInsideResult puttingResult)
    {
        PuttingResult = puttingResult;
    }
}