using UnityEngine;

// remove later – now unused; can be deleted once nothing references it
public class ContextMenuContext : IInteractionContext
{
    public GameState GameState;
    public ContextMenuContext(GameState gameState)
    {
        GameState = gameState;
    }
}

public class WarehouseInteractionManager : IInteractionManager
{
    // when dragging starts -> select target objects
    // when dragging ends -> get the drop target and perform action
    // when clicking -> open popup menu

    private Product draggedProduct;

    private readonly WarehouseContextMenuProfileRegistry _contextMenuRegistry =
        new WarehouseContextMenuProfileRegistry();

    public void OnProductViewClick(Interactable interactable)
    {
        var gameState = G.GameState;
        var profile = _contextMenuRegistry.Get(gameState);
        if (profile == null)
        {
            Debug.LogWarning("WarehouseInteractionManager: No context menu profile for current game state");
            return;
        }

        var productView = interactable.GetComponent<ProductView>();
        if (productView == null)
        {
            Debug.LogWarning("WarehouseInteractionManager: Interactable has no ProductView");
            return;
        }

        var model = productView.Product;
        if (model == null)
        {
            Debug.LogWarning("WarehouseInteractionManager: ProductView.Product is null");
            return;
        }

        var ctxMenuContent = profile.GenerateContent(model, gameState);
        if (ctxMenuContent != null)
        {
            G.ContextMenuManager.CreateContextMenu(interactable.gameObject, ctxMenuContent);
        }
    }

    public void OnDragEnd(Interactable interactable)
    {
        if (draggedProduct == null)
        {
            Debug.LogWarning("Unknown product has been stopped dragging");
            return;
        }

        // casting all products on drop location
        var positionInteractables = InteractionPhysics.RaycastForInteractables(
            G.GameCursor.interactableLayersMask,
            interactable.transform.position,
            G.GameCursor.use3dPhysics);

        tryDropProduct(positionInteractables);

        draggedProduct = null;
    }

    private void tryDropProduct(Interactable[] interactables)
    {
        // trying to attach dragged product to any car on the position
        foreach (var targetInteractable in interactables)
        {
            var productView = targetInteractable.GetComponent<ProductView>();
            var targetProduct = productView?.Product;
            if (targetProduct == draggedProduct)
            {
                continue;
            }

            var car = targetProduct as Car;
            Car.CarPartLocation slot = car?.AvailableSlot(draggedProduct);
            if (slot != null)
            {
                attachProductToCar(draggedProduct, slot);
                break;
            }
        }
    }

    private void attachProductToCar(Product product, Car.CarPartLocation slot)
    {
        if (!G.ProductLifetimeService.MoveProduct(product, slot))
        {
            Debug.LogWarning(
                $"Failed to attach product {draggedProduct.Name} to car slot {slot.PartSlotRuntimeConfig}");
        }
    }

    public void OnDragStart(Interactable interactable)
    {
        // getting dragged product
        var productView = interactable.GetComponent<ProductView>();
        if (productView != null)
        {
            draggedProduct = productView.Product;
        }
    }

    public void OnTriggerEntered(ModelProvider trigger, ModelProvider triggerCause)
    {
        Debug.LogWarning($"WarehouseInteractionManager: OnTriggerEntered is not implemented, objects {trigger?.CityEntity} and {triggerCause?.CityEntity}");
    }

    // NOTE: the old inner WarehouseContextMenuContentProfile is now moved
    // out as NeutralWarehouseContextMenuProfile and wired via registry.
}

