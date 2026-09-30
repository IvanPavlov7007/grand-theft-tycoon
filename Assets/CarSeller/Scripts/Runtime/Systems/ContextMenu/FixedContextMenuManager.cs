using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Might be used in the future for fixed context menus
// TODO don't duplicate code with ContextMenuManager
public class FixedContextMenuManager : GlobalSingletonBehaviour<FixedContextMenuManager>
{
    protected override FixedContextMenuManager GlobalInstance { get => G.FixedContextMenuManager; set => G.FixedContextMenuManager = value; }

    public GameObject fixedPopUpMenuPrefab;
    public SimpleUIBuilder UIBuilder;
    public HashSet<PopUpContextMenu> activeMenus = new HashSet<PopUpContextMenu>();

    [Header("Context Menu Visuals")]
    [SerializeField]
    private Color UsedContextMenuColor = Color.white;

    public PopUpContextMenu CreateContextMenu(UIElement content)
    {
        GameObject panel = Instantiate(fixedPopUpMenuPrefab, G.FixedContextMenuCanvas.Canvas.transform);
        RectTransform panelTransform = panel.GetComponent<RectTransform>();

        // Set panel color
        var image = panelTransform.GetComponent<Image>();
        if (image != null)
        {
            image.color = UsedContextMenuColor;
        }

        var contentTransform = getContentTransform(panel);
        UIBuilder.Build(content, contentTransform);

        var menu = createContextMenu(panel, content.blockingInput);
        menu.Initialize(null, contentTransform, OnMenuClose);

        G.GameCursor.CancelCurrentInteraction(invokeDragEnd: false);
        return menu;
    }

    public PopUpContextMenu CreateContextMenu(Widget widget)
    {
        RectTransform view = UIBuilder.Build(widget, G.FixedContextMenuCanvas.Canvas.transform);

        var menu = createContextMenu(view.gameObject, widget.BlockingInput);
        menu.Initialize(null, view, OnMenuClose);

        G.GameCursor.CancelCurrentInteraction(invokeDragEnd: false);
        return menu;
    }

    private PopUpContextMenu createContextMenu(GameObject panel, bool blocking)
    {
        var ctxMenu = panel.AddComponent<PopUpContextMenu>();
        activeMenus.Add(ctxMenu);
        if (blocking)
            G.BlockUIManager.Block(G.FixedContextMenuCanvas.Canvas, ctxMenu.Close);
        return ctxMenu;
    }

    private void OnMenuClose(PopUpContextMenu menu)
    {
        if (activeMenus.Contains(menu))
        {
            activeMenus.Remove(menu);
            G.BlockUIManager.Unblock(G.FixedContextMenuCanvas.Canvas);
            Destroy(menu.gameObject);
        }
    }

    private RectTransform getContentTransform(GameObject panel)
    {
        // Using a LayoutGroup child as Content (VerticalLayoutGroup present)
        return panel.GetComponentInChildren<LayoutGroup>().GetComponent<RectTransform>();
    }
}