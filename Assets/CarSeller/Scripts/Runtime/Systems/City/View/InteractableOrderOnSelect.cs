using UnityEngine;
    
public class InteractableOrderOnSelect : MonoBehaviour
{
    CityViewObjectController controller;
    Interactable interactable;

    int initialSortingOrder;

    const int SelectedSortingOrder = 5;

    private void Awake()
    {
        controller = GetComponent<CityViewObjectController>();
        interactable = GetComponent<Interactable>();
        Debug.Assert(controller != null, $"Missing {nameof(CityViewObjectController)} on {gameObject.name}");
        Debug.Assert(interactable != null, $"Missing {nameof(Interactable)} on {gameObject.name}");

        initialSortingOrder = interactable.sortingOrder;
    }

    private void OnEnable()
    {
        controller.OnVisualStateChanged += handleVisualStateChanged;
    }
  
    private void OnDisable()
    {
        controller.OnVisualStateChanged -= handleVisualStateChanged;
    }
  
    private void handleVisualStateChanged(ViewObjectVisualState newState)
    {
        switch (newState)
        {
            case ViewObjectVisualState.Selected:
                interactable.sortingOrder = SelectedSortingOrder;
                break;
            default:
                interactable.sortingOrder = initialSortingOrder;
                break;
        }
    }
}