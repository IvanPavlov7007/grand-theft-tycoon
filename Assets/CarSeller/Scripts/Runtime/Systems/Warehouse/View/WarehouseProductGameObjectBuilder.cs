using UnityEngine;

public abstract class WarehouseProductGameObjectBuilder : ScriptableObject, IProductViewBuilder<GameObject>
{
    [Header("Common product view dependencies")]
    public CarPartGameObjectBuilder carPartViewBuilder;

    [Tooltip("Generic rectangular prefab with SpriteRenderer + collider for basic products.")]
    public GameObject rectanglePrefab;

    // Shared view component builder for all warehouse product views
    protected IProductViewComponentBuilder<WarehouseProductView> productViewComponentBuilder =
        new WarehouseProductViewComponentBuilder();

    #region Abstracts

    public abstract GameObject BuildCar(Car car);
    public abstract GameObject BuildCarFrame(CarFrame carFrame);

    #endregion

    #region Default implementations for simple products

    public virtual GameObject BuildEngine(Engine engine)
    {
        var go = Instantiate(rectanglePrefab);
        var sr = go.GetComponent<SpriteRenderer>();
        go.name = engine.Name;
        sr.sprite = engine.runtimeConfig.Sprite;
        CollisionBuilder.InitializeCollision(sr);

        productViewComponentBuilder.BuildViewComponent(go, engine);
        return go;
    }

    public virtual GameObject BuildSpoiler(Spoiler spoiler)
    {
        var go = Instantiate(rectanglePrefab);
        var sr = go.GetComponent<SpriteRenderer>();
        go.name = spoiler.Name;
        sr.sprite = spoiler.runtimeConfig.Sprite;
        sr.color = spoiler.runtimeConfig.Color;
        go.transform.localScale = Vector3.one * spoiler.runtimeConfig.Size;
        CollisionBuilder.InitializeCollision(sr);

        productViewComponentBuilder.BuildViewComponent(go, spoiler);
        return go;
    }

    public virtual GameObject BuildWheel(Wheel wheel)
    {
        var go = Instantiate(rectanglePrefab);
        var sr = go.GetComponent<SpriteRenderer>();
        go.name = wheel.Name;
        sr.sprite = wheel.runtimeConfig.TopViewSprite;
        sr.color = wheel.runtimeConfig.Color;
        go.transform.localScale = Vector3.one * wheel.runtimeConfig.TopViewSize;
        CollisionBuilder.InitializeCollision(sr);

        productViewComponentBuilder.BuildViewComponent(go, wheel);
        return go;
    }

    #endregion

    #region Shared Warehouse view component builder

    public class WarehouseProductViewComponentBuilder : IProductViewComponentBuilder<WarehouseProductView>
    {
        public WarehouseProductView BuildViewComponent(GameObject gameObject, Product product)
        {
            Debug.Assert(gameObject != null, "gameObject != null");
            Debug.Assert(product != null, "product != null");

            WarehouseProductView view;
            if (product is Car)
            {
                view = gameObject.AddComponent<WarehouseCarView>();
            }
            else
            {
                view = gameObject.AddComponent<WarehouseProductView>();
            }

            view.Initialize(product, G.ProductLifetimeService.GetProductLocation(product));
            InitializeAdditionalComponents(gameObject, product);
            return view;
        }

        // Add any additional components based on product type if needed
        private void InitializeAdditionalComponents(GameObject gameObject, Product product)
        {
            gameObject.AddComponent<DirectDragInteractable>();

            if (product is not Car)
            {
                gameObject.AddComponent<DragCollisionDisabler>();
                gameObject.AddComponent<DragSortingOrderChanger>();
            }
        }
    }

    #endregion
}