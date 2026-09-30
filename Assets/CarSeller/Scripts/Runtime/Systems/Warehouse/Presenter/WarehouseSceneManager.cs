using UnityEngine;


/// <summary>
/// Loads and manages warehouse in the warehouse scene
/// Listens when to create product views and creates them in the scene
/// </summary>
public class WarehouseSceneManager : GlobalSingletonBehaviour<WarehouseSceneManager>
{
    protected override WarehouseSceneManager GlobalInstance { get => G.WarehouseSceneManager; set => G.WarehouseSceneManager = value; }

    static Warehouse CurrentWarehouse => G.GameFlowController.CurrentWarehouse;

    public Transform emptyPosition;

    private void OnEnable()
    {
        GameEvents.Instance.OnProductCreated += onNewProductCreated;
        GameEvents.Instance.OnProductLocationChanged += onProductLocationChanged;
    }

    private void OnDisable()
    {
        GameEvents.Instance.OnProductCreated -= onNewProductCreated;
        GameEvents.Instance.OnProductLocationChanged -= onProductLocationChanged;
    }

    private void Update()
    {
        if (emptyPosition != null)
        {
            CurrentWarehouse.emptyProductLocation = new Warehouse.DimensionalPositionData
            { LocalPosition = emptyPosition.localPosition, LocalRotation = emptyPosition.localEulerAngles };
        }
        else
        {
            CurrentWarehouse.emptyProductLocation = new Warehouse.DimensionalPositionData
            { LocalPosition = Vector3.up * 4f, LocalRotation = Vector3.zero };
        }
        
    }

    public void InitializeWarehouse()
    {
        if (CurrentWarehouse == null)
        {
            Debug.LogWarning("Warehouse instance is not set");
            return;
        }
        foreach (var location in CurrentWarehouse.productLocations)
        {
            Debug.Assert(location.Product != null, "Product at the location is null");

            buildProductView(location.Product, location);
        }
    }

    void onNewProductCreated(ProductCreatedEventData data)
    {
        var location = G.ProductLifetimeService.GetProductLocation(data.Product);
        if (CurrentWarehouse == null)
        {
            Debug.LogError("Warehouse instance is not set");
            return;
        }
        if (location.Holder == CurrentWarehouse)
        {
            buildProductView(data.Product, location);
        }
    }

    void onProductLocationChanged(ProductLocationChangedEventData data)
    {
        Debug.Assert(CurrentWarehouse != null, "Warehouse instance is not set");

        if (data.NewLocation?.Holder == CurrentWarehouse)
        {
            buildProductView(data.Product, data.NewLocation);
        }
    }


    void buildProductView(Product product, ILocation location)
    {
        var productViewGO = product.GetRepresentation(G.warehouseProductViewBuilder);
        var transform = productViewGO.transform;

        transform.SetParent(this.transform);
        var dimensionalData = (location as Warehouse.WarehouseProductLocation).Position;
        transform.localPosition = dimensionalData.LocalPosition;
        transform.localEulerAngles = dimensionalData.LocalRotation;
    }
}
