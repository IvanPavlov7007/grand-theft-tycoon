using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using System;

[CreateAssetMenu(fileName = "UIBuilder", menuName = "GameUI/UIBuilder")]
public class SimpleUIBuilder : SingletonScriptableObject<SimpleUIBuilder>, IUIElementBuilder<RectTransform>
{
    [Header("Prefabs")]
    public GameObject RowHolderPrefab;
    public GameObject PushButtonPrefab;
    public GameObject ContentButtonPrefab;
    public GameObject TextMeshProPrefab;
    public GameObject ImagePrefab;
    public GameObject VLayoutPrefab;
    public GameObject HLayoutPrefab;
    public GameObject GridLayoutPrefab;

    [Header("Touch Sizing")]
    [Tooltip("Minimum row height to provide a comfortable touch target.")]
    [ShowInInspector]
    public const float MinRowHeight = 32f;
    [Tooltip("Minimum button height to provide a comfortable touch target.")]
    [ShowInInspector]
    public const float MinButtonHeight = 60f;
    [ShowInInspector]
    public const float MinImageHeight = 200f;
    [ShowInInspector]
    public const float PrefferedButtonHeight = 80f;

    public RectTransform Build(UIElement content, RectTransform container)
    {
        switch (content.Type)
        {
            case UIElementType.Container:
                var subContainer = container;//BuildContainer(content, container);
                foreach (var child in content.Children)
                {
                    Debug.Log($"Building child of type {child.Type} in container with style {content.Style}");
                    Build(child, subContainer);
                }
                return subContainer;
            case UIElementType.Button:
                return BuildButton(content, container);
            case UIElementType.ButtonContainer:
                var button = BuildContentButton(content, container);
                foreach(var child in content.Children)
                {
                    Build(child, button);
                }
                return button;
            case UIElementType.Text:
                return BuildText(content, container);
            case UIElementType.Image:
                return BuildImage(content, container);
            default:
                return null;
        }
    }
    

    private RectTransform BuildText(UIElement item, RectTransform container)
    {
        var headerTMPObj = GameObject.Instantiate(TextMeshProPrefab, container);
        var headerTMP = headerTMPObj.GetComponent<TextMeshProUGUI>();
        headerTMP.text = item.Text;
        switch (item.Style)
        {
            case "header":
                headerTMP.fontSize = 56;
                headerTMP.fontStyle = FontStyles.Bold;
                headerTMP.alignment = TextAlignmentOptions.Top;
                break;
            case "description":
                headerTMP.fontSize = 48;
                headerTMP.fontStyle = FontStyles.Italic;
                headerTMP.alignment = TextAlignmentOptions.MidlineLeft;
                break;
            case "hint":
                headerTMP.fontSize = 48;
                headerTMP.alignment = TextAlignmentOptions.MidlineRight;
                break;
            case "price":
                headerTMP.fontSize = 56;
                headerTMP.fontStyle = FontStyles.Bold;
                headerTMP.alignment = TextAlignmentOptions.Midline;
                headerTMP.color = Color.white;
                break;
            default:
                headerTMP.fontSize = 48; // bump default for readability on touch
                break;
        }

        // Make TMP auto-size to use available vertical space
        //headerTMP.enableAutoSizing = true;
        headerTMP.textWrappingMode = TextWrappingModes.Normal;

        RectTransform recT = headerTMP.GetComponent<RectTransform>();

        // Ensure row has at least MinRowHeight, but expand to fit text if needed
        LayoutRebuilder.ForceRebuildLayoutImmediate(recT);
        var preferredHeight = headerTMP.preferredHeight;

        var le = recT.GetComponent<LayoutElement>();
        if (le == null) le = recT.gameObject.AddComponent<LayoutElement>();

        le.minHeight = Mathf.Max(MinRowHeight, preferredHeight);
        //le.preferredHeight = Mathf.Max(le.preferredHeight, preferredHeight);
        le.flexibleHeight = 0f;

        return recT;
    }

    private RectTransform BuildButton(UIElement item, RectTransform container)
    {
        var buttonObj = GameObject.Instantiate(PushButtonPrefab, container);
        var buttonTMP = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
        if (buttonTMP != null)
        {
            buttonTMP.text = item.Text;
            buttonTMP.fontSize = Mathf.Max(buttonTMP.fontSize, 20); // comfortable default
        }

        buttonObj.AddComponent<ButtonStateController>().
             Initialize(item);

        RectTransform recT = buttonObj.GetComponent<RectTransform>();
        // Ensure button itself has adequate hit size via LayoutElement
        EnsureMinHeight(recT, MinButtonHeight, PrefferedButtonHeight);
        return recT;
    }

    // Build container
    private RectTransform BuildContainer(UIElement item, RectTransform container)
    {
        GameObject layoutObj;
        switch (item.Style)
        {
            case "vertical":
                layoutObj = GameObject.Instantiate(VLayoutPrefab, container);
                break;
            case "horizontal":
                layoutObj = GameObject.Instantiate(HLayoutPrefab, container);
                break;
            case "grid":
                layoutObj = new GameObject();
                layoutObj.AddComponent<RectTransform>().parent = container;
                var el = layoutObj.AddComponent<LayoutElement>();

                var grid = layoutObj.AddComponent<GridLayoutGroup>();
                grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                grid.constraintCount = 2;
                break;
            default:
                layoutObj = GameObject.Instantiate(VLayoutPrefab, container);
                break;
        }
        return layoutObj.GetComponent<RectTransform>();
    }

    private RectTransform BuildContentButton(UIElement item, RectTransform container)
    {
        var buttonObj = GameObject.Instantiate(ContentButtonPrefab, container);

        buttonObj.AddComponent<ButtonStateController>().
             Initialize(item);

        VerticalLayoutGroup group = buttonObj.AddComponent<VerticalLayoutGroup>();

        group.childAlignment = TextAnchor.MiddleCenter;
        group.padding = new RectOffset(8, 8, 20, 8);

        RectTransform recT = buttonObj.GetComponent<RectTransform>();
        // Ensure button itself has adequate hit size via LayoutElement
        //EnsureMinHeight(recT, MinButtonHeight, PrefferedButtonHeight);
        return recT;
    }

    private RectTransform BuildImage(UIElement item, RectTransform container)
    {
        
        var imageObj = GameObject.Instantiate(ImagePrefab, container);
        var imageRect = imageObj.GetComponent<RectTransform>();
        var image = imageObj.GetComponent<Image>();
        image.sprite = item.Image;
        image.type = Image.Type.Simple;
        image.preserveAspect = true;
        // Ensure minimum height for touch
        EnsureMinHeight(imageRect, MinImageHeight, image.sprite.rect.height);
        return imageRect;
    }

    private void EnsureMinHeight(RectTransform rowHolder, float minHeight = MinRowHeight, float prefferedHeight = -1f)
    {
        var le = rowHolder.GetComponent<LayoutElement>();
        if (le == null) le = rowHolder.gameObject.AddComponent<LayoutElement>();
        if (le.minHeight < minHeight) le.minHeight = minHeight;
        // Disallow flexible shrinking to preserve touch targets
        le.flexibleHeight = 0f;
        if(prefferedHeight > 0f)
            le.preferredHeight = prefferedHeight;
    }

    private Dictionary<Type, WidgetView> registry = new();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
    public static void Initialize()
    {
        Instance.RegisterPrefabs();
    }

    public void RegisterPrefabs()
    {
        var prefabs = Resources.LoadAll<GameObject>("Prefabs/UI/Widgets");
        registry.Clear();

        foreach (var prefab in prefabs)
        {
            var view = prefab.GetComponent<WidgetView>();
            if (view == null)
                continue;
            registry[view.WidgetType] = view;
        }
    }

    public RectTransform Build(Widget widget, Transform parent)
    {
        var prefabView = registry[widget.GetType()];

        var view = Instantiate(prefabView, parent);

        view.Bind(widget);
        view.BuildChildren(this, widget);

        return view.GetComponent<RectTransform>();
    }
}