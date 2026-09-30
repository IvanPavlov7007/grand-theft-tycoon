using System.Collections.Generic;
using UnityEngine;

public class ViewVisualStateChanger : MonoBehaviour
{
    [SerializeField, Range(0.2f, 1f)]
    private float disabledBrightnessMultiplier = 0.6f;

    [Header("Sorting")]
    [SerializeField]
    private int selectedSortingOrderIncrease = 5;

    private CityViewObjectController cityViewObjectController;

    private readonly Dictionary<SpriteRenderer, Color> originalColors = new Dictionary<SpriteRenderer, Color>();
    private readonly Dictionary<SpriteRenderer, int> originalSortingOrders = new Dictionary<SpriteRenderer, int>();

    private SpriteRenderer[] spriteRenderers;

    private void Awake()
    {
        cityViewObjectController = GetComponent<CityViewObjectController>();
        CacheRenderersAndColors();
    }

    private void OnEnable()
    {
        if (cityViewObjectController != null)
        {
            cityViewObjectController.OnVisualStateChanged += onVisualStateChanged;
            onVisualStateChanged(cityViewObjectController.CurrentVisualState);
        }
    }

    private void OnDisable()
    {
        if (cityViewObjectController != null)
        {
            cityViewObjectController.OnVisualStateChanged -= onVisualStateChanged;
        }
    }

    private void OnTransformChildrenChanged()
    {
        // Re-cache and re-apply when hierarchy changes at runtime.
        var current = cityViewObjectController != null ? cityViewObjectController.CurrentVisualState : ViewObjectVisualState.Normal;
        CacheRenderersAndColors();
        onVisualStateChanged(current);
    }

    private void onVisualStateChanged(ViewObjectVisualState state)
    {
        if (spriteRenderers == null || spriteRenderers.Length == 0)
            CacheRenderersAndColors();

        if (state == ViewObjectVisualState.Disabled)
        {
            ApplyDisabledColors();
        }
        else
        {
            RestoreOriginalColors();
        }

        if (state == ViewObjectVisualState.Selected)
        {
            ApplySelectedSortingOrder();
        }
        else
        {
            RestoreOriginalSortingOrder();
        }
    }

    private void CacheRenderersAndColors()
    {
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);

        originalColors.Clear();
        originalSortingOrders.Clear();

        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            var sr = spriteRenderers[i];
            if (sr == null) continue;

            originalColors[sr] = sr.color;
            originalSortingOrders[sr] = sr.sortingOrder;
        }
    }

    private void RestoreOriginalColors()
    {
        foreach (var kvp in originalColors)
        {
            if (kvp.Key == null) continue;
            kvp.Key.color = kvp.Value;
        }
    }

    private void ApplyDisabledColors()
    {
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            var sr = spriteRenderers[i];
            if (sr == null) continue;

            // Ensure we have the original saved (covers late-added renderers).
            if (!originalColors.ContainsKey(sr))
                originalColors[sr] = sr.color;

            var orig = originalColors[sr];
            sr.color = ToGrayscale(orig, disabledBrightnessMultiplier);
        }
    }

    private void ApplySelectedSortingOrder()
    {
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            var sr = spriteRenderers[i];
            if (sr == null) continue;

            // Ensure we have the original saved (covers late-added renderers).
            if (!originalSortingOrders.ContainsKey(sr))
                originalSortingOrders[sr] = sr.sortingOrder;

            sr.sortingOrder = originalSortingOrders[sr] + selectedSortingOrderIncrease;
        }
    }

    private void RestoreOriginalSortingOrder()
    {
        foreach (var kvp in originalSortingOrders)
        {
            if (kvp.Key == null) continue;
            kvp.Key.sortingOrder = kvp.Value;
        }
    }

    private static Color ToGrayscale(Color c, float brightnessMultiplier)
    {
        // Rec. 709 luminance coefficients
        float gray = (0.2126f * c.r) + (0.7152f * c.g) + (0.0722f * c.b);
        gray *= brightnessMultiplier;
        return new Color(gray, gray, gray, c.a);
    }
}