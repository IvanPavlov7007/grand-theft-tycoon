using UnityEngine;

public class DragCollisionDisabler : MonoBehaviour
{
    private Collider2D objectCollider;
    private Interactable directDragInteractable;
    private void Awake()
    {
        objectCollider = GetComponent<Collider2D>();
        directDragInteractable = GetComponent<DirectDragInteractable>();
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
            if (objectCollider != null)
            {
                objectCollider.enabled = false;
            }
        }
    }
    private void HandleDragEnd(Interactable draggedObject)
    {
        if (draggedObject.gameObject == this.gameObject)
        {
            if (objectCollider != null)
            {
                objectCollider.enabled = true;
            }
        }
    }
}