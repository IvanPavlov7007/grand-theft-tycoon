using UnityEngine;
public class ContextMenuCanvas : GlobalSingletonBehaviour<ContextMenuCanvas>
{
    protected override ContextMenuCanvas GlobalInstance { get => G.ContextMenuCanvas; set => G.ContextMenuCanvas = value; }

    public RectTransform RectTransform { get; private set; }
    public Canvas Canvas { get; private set; }
    public CanvasGroup CanvasGroup { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        if (!IsActiveSingleton) return;

        Canvas = GetComponent<Canvas>();
        CanvasGroup = GetComponent<CanvasGroup>();
        RectTransform = GetComponent<RectTransform>();
    }
}