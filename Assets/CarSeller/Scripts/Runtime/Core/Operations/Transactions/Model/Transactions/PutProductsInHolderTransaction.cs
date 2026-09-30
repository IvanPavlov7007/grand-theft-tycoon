public class PutProductsInHolderTransaction : Transaction
{
    public readonly Product[] Products;
    public readonly ITargetProductsHolder TargetHolder;

    public PutProductsInHolderTransaction(ITargetProductsHolder targetProductsHolder, params Product[] products)
    {
        Products = products;
        TargetHolder = targetProductsHolder;
    }
}