using UnityEngine;

public class PullCarFromWarehouseHandler : TransactionHandler<PullCarFromWarehouseTransaction>
{
    public override TransactionResult Handle(PullCarFromWarehouseTransaction transaction)
    {
        Debug.Assert(transaction != null, "PullCarFromWarehouseTransaction is null");
        Debug.Assert(transaction.Car != null, "Car in PullCarFromWarehouseTransaction is null");
        Debug.Assert(transaction.SourceWarehouse != null, "SourceWarehouse in PullCarFromWarehouseTransaction is null");

        TransactionResult result;

        if (G.CityActionService.PutCarOutsideWarehouse(transaction.Car, transaction.SourceWarehouse))
        {
            result = TransactionResult.Success();
        }
        else // couldn't put out of the warehouse
        {
            result = new TransactionResult(TransactionResultType.Failure, 
                data: new PuttingCarOutsideFailureData(transaction.Car));
        }
        return result;

    }
}

public class PuttingCarOutsideFailureData : ITransactionResultData
{
    Car car;

    public PuttingCarOutsideFailureData(Car car)
    {
        this.car = car;
    }
}