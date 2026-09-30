using System;

[Obsolete("Since it's uses WarehouseActionsHelper directly, which is job of PutProductsInHolderHandler")]
public class StealHandler
{

    //public TransactionResult Handle(StealTransaction transaction)
    //{
    //    var data = transaction.Data as StealTransaction;
    //    if (data == null)
    //    {
    //        return TransactionResult.InvalidTransaction("Invalid data for steal transaction.");
    //    }
    //    var car = data.Car;
    //    var targetWarehouse = data.TargetWarehouse;

    //    // Logic to add the stolen car to the target warehouse
        
    //    TransactionResult result;

    //    var putInsideResult = WarehouseActionsHelper.TryPutProductsInsideWarehouse(targetWarehouse, car);

    //    if (putInsideResult.putInsideProducts.Count > 0)
    //    {
    //        result = TransactionResult.Success();
    //    }
    //    else // couldn't find space in the warehouse
    //    {
    //        result = new TransactionResult(TransactionResultType.Failure, 
    //            data: new ProductsPlacingResultData(putInsideResult));
    //    }
    //    return result;
    //}
}