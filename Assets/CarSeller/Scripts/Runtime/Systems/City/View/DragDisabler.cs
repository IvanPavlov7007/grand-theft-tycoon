using UnityEngine;

public class DragDisabler : MonoBehaviour
{
    DragInteractable dragInteractable;
    CityViewObjectController cityViewObjectController;
    private void Awake()
    {
        dragInteractable = GetComponent<DragInteractable>();
        cityViewObjectController = GetComponent<CityViewObjectController>();
    }
    private void OnEnable()
    {
        cityViewObjectController.OnDraggableStateChanged += onDraggableStateChanged;
    }
    

    private void OnDisable()
    {
        cityViewObjectController.OnDraggableStateChanged -= onDraggableStateChanged;
    }

    private void onDraggableStateChanged(bool draggable)
    {
        dragInteractable.SetActive(draggable);
    }

}