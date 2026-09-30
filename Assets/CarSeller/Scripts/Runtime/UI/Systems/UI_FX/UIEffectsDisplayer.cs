using Pixelplacement;
using TMPro;
using UnityEngine;

public class UIEffectsDisplayer : GlobalSingletonBehaviour<UIEffectsDisplayer>
{
    protected override UIEffectsDisplayer GlobalInstance { get => G.UIEffectsDisplayer; set => G.UIEffectsDisplayer = value; }

    [Header("World")]
    public float MoneyEffectHeightOffset = 2f;
    public GameObject MoneyWorldEffectPrefab;

    [Header("UI")]
    public GameObject MoneyUIEffectPrefab;
    public float MoneyUIEffectHeightOffset = 100f;

    Canvas _canvas;
    Camera Camera => Camera.main;

    protected override void Awake()
    {
        base.Awake();
        if (!IsActiveSingleton) return;

        _canvas = GetComponent<Canvas>();
    }

    public void PlayMoneyEffectWorld(float price, Vector3 position)
    {
        if (MoneyWorldEffectPrefab == null)
            return;

        var go = GameObject.Instantiate(MoneyWorldEffectPrefab, position, Quaternion.identity);
        var textMesh = go.GetComponentInChildren<TextMeshProUGUI>();
        SetupMoneyText(textMesh, price);

        Tween.Position(go.transform,
            position + Vector3.up * MoneyEffectHeightOffset,
            3f, 0f, Tween.EaseInOut);

        if (textMesh != null)
        {
            Tween.Color(textMesh, Color.clear, 3f, 0f, Tween.EaseInOut);
        }

        Object.Destroy(go, 3f);
    }

    public void PlayMonetEffectScreen(float price, Vector3 worldPosition)
    {
        if (MoneyUIEffectPrefab == null || _canvas == null || Camera == null)
            return;

        var screenPosition = Camera.WorldToScreenPoint(worldPosition);
        var uiPosition = _canvas.transform.InverseTransformPoint(screenPosition);

        var go = GameObject.Instantiate(MoneyUIEffectPrefab, _canvas.transform);
        var rectTransform = go.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = uiPosition;

        var textMesh = go.GetComponentInChildren<TextMeshProUGUI>();
        SetupMoneyText(textMesh, price);

        Tween.Position(go.transform,
            uiPosition + Vector3.up * MoneyUIEffectHeightOffset,
            3f, 0f, Tween.EaseInOut);

        if (textMesh != null)
        {
            Tween.Color(textMesh, Color.clear, 3f, 0f, Tween.EaseInOut);
        }

        Object.Destroy(go, 3f);
    }

    void SetupMoneyText(TextMeshProUGUI textMesh, float price)
    {
        if (textMesh == null)
            return;

        textMesh.text = $"+${price:N0}";
        textMesh.color = Color.yellow;
    }
}