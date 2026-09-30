using UnityEngine;

public class SelectableVisuals : MonoBehaviour
{
    CityViewObjectController carViewController;
    GameObject selectionRing;

    void Awake()
    {
        carViewController = GetComponent<CityViewObjectController>();
        createSelection();
    }

    private void OnEnable()
    {
        carViewController.OnVisualStateChanged += HandleVisualStateChanged;
    }

    private void OnDisable()
    {
        carViewController.OnVisualStateChanged -= HandleVisualStateChanged;
    }

    private void HandleVisualStateChanged(ViewObjectVisualState state)
    {
        activateSelection(state == ViewObjectVisualState.Selected);
    }

    void createSelection()
    {
        CarSelectionSettings settings = CarSelectionSettings.Instance;
        selectionRing = Instantiate(settings.selectionRingPrefab, transform);
        selectionRing.transform.localScale *= settings.selectionRingScaleMultiplier;
        var sprite = selectionRing.GetComponent<SpriteRenderer>();
        sprite.color = settings.selectionRingColor;
        sprite.sortingOrder = settings.sortingOrder;
    }

    void activateSelection(bool active)
    {
        selectionRing.SetActive(active);
    }
}