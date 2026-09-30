using UnityEngine;

public class DragSortingOrderChanger : MonoBehaviour
{
    private Interactable directDragInteractable;
    private SpriteRenderer spriteRenderer;
    private int originalSortingOrder;
    private void Awake()
    {
        directDragInteractable = GetComponent<DirectDragInteractable>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalSortingOrder = spriteRenderer.sortingOrder;
        }
    }
    private void OnEnable()
    {
        directDragInteractable.CursorDragStarted += HandleDragStart;
        directDragInteractable.CursorDragEnded += HandleDragEnd;
    }
    private void OnDisable()
    {
        directDragInteractable.CursorDragStarted -= HandleDragStart;
        directDragInteractable.CursorDragEnded -= HandleDragEnd;
    }
    private void HandleDragStart(Interactable draggedObject)
    {
        if (draggedObject.gameObject == this.gameObject)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.sortingOrder = 1000; // Set to a high value to render on top
            }
        }
    }
    private void HandleDragEnd(Interactable draggedObject)
    {
        if (draggedObject.gameObject == this.gameObject)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.sortingOrder = originalSortingOrder; // Restore original sorting order
            }
        }
    }
}