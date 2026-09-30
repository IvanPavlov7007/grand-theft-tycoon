using UnityEngine;

public interface IProductViewComponentBuilder<TProductView> where TProductView : ProductView
{
    public TProductView BuildViewComponent(GameObject gameObject, Product product);
}

/// <summary>
/// Since products can be located in deffent locations (warehouse, city, etc), this view represents a product in a specific location.
/// It destroys itself when the product is moved to a different location or destroyed.
/// Handles its own destruction
/// </summary>
public class ProductView : MonoBehaviour, ModelProvider
{
    public Product Product { get; private set; }
    public ILocation RepresentedProductLocation { get; private set; }

    public bool Initialized { get; private set; } = false;

    public CityEntity CityEntity => null;
    public ILocatable Locatable => Product;
    public GameObject ViewGameObject => gameObject;

    public virtual void Initialize(Product product, ILocation representedProductLocation)
    {
        this.Product = product;
        this.RepresentedProductLocation = representedProductLocation;
        Initialized = true;
    }

    protected virtual void OnEnable()
    {
        GameEvents.Instance.OnProductLocationChanged += productLocationChanged;
        GameEvents.Instance.OnProductDestroyed += productDestroyed;
    }

    protected virtual void OnDisable()
    {
        GameEvents.Instance.OnProductLocationChanged -= productLocationChanged;
        GameEvents.Instance.OnProductDestroyed -= productDestroyed;
    }

    private void productLocationChanged(ProductLocationChangedEventData data)
    {
        if(data.NewLocation?.Occupant == Product && RepresentedProductLocation != data.NewLocation)
        {
            OnProductLocationChanged();
        }
    }

    private void productDestroyed(ProductDestroyedEventData data)
    {
        if(data.Product == Product)
        {
            OnProductDestroyed();
        }
    }

    protected virtual void OnProductDestroyed()
    {
        Destroy(gameObject);
    }

    protected virtual void OnProductLocationChanged()
    {
        Destroy(gameObject);
    }
}