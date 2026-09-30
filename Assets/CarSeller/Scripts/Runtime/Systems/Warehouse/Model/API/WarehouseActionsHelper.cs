using System.Collections.Generic;
using System.Linq;

//Question is, is it more useful to have use this helper, which is currently called by TransactionHandlers
// or should the helper be using TransactionHandlers instead?

public static class WarehouseActionsHelper
{
    public static ProductsPutInsideResult TryPutProductsInsideWarehouse(Warehouse warehouse, params Product[] products)
    {
        List<Product> skippedProducts = new List<Product>();
        List<Product> putInsideProducts = new List<Product>();
        skippedProducts.AddRange(products);

        foreach (var product in products)
        {
            //first check if there is space in the warehouse
            //for now only for cars
            if (product is Car)
            {
                if (warehouse.AvailableCarParkingSpots <= 0)
                {
                    continue;
                }
            }

            if (G.ProductLifetimeService.MoveProduct(product, warehouse.GetEmptyLocation()))
            {
                putInsideProducts.Add(product);
                skippedProducts.Remove(product);
            }
        }
        return new ProductsPutInsideResult(skippedProducts,putInsideProducts);
    }
}

public class ProductsPutInsideResult
{
    public IReadOnlyList<Product> skippedProducts = new List<Product>();
    public IReadOnlyList<Product> putInsideProducts = new List<Product>();

    public ProductsPutInsideResult(IReadOnlyList<Product> skippedProducts, IReadOnlyList<Product> putInsideProducts)
    {
        this.skippedProducts = skippedProducts;
        this.putInsideProducts = putInsideProducts;
    }

    public bool AllProductsPutInside => skippedProducts.Count == 0;

    public bool CarsSkipped => skippedProducts.Select(p => p is Car).Count() > 0;
}