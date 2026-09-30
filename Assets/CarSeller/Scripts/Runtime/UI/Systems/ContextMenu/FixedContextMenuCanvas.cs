using UnityEngine;

public class FixedContextMenuCanvas : GlobalSingletonBehaviour<FixedContextMenuCanvas>
{
    protected override FixedContextMenuCanvas GlobalInstance { get => G.FixedContextMenuCanvas; set => G.FixedContextMenuCanvas = value; }

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