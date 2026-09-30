using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public abstract class ButtonView<T> : WidgetView<T> , IPointerDownHandler, IPointerUpHandler where T : ButtonWidget
{
    protected Button button;
    protected T widget;

    public bool Initialized { get; private set; }
    public bool Interactable => Initialized && widget.IsInteractable;

    // Track pointer for distinguishing tap vs scroll/drag
    private bool _pointerDown;
    private Vector2 _pointerDownPos;
    private const float DragThreshold = 10f; // pixels

    protected override void Bind(T widget)
    {
        Initialize(widget);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _pointerDown = true;
        _pointerDownPos = eventData.position;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // If this pointer-up is part of a drag/scroll, ignore it
        if (!_pointerDown)
            return;

        _pointerDown = false;

        // Check if the finger moved too much (scroll/drag)
        if (Vector2.Distance(_pointerDownPos, eventData.position) > DragThreshold)
            return;

        // Now treat it as a real tap
        if (!Interactable)
            G.GlobalHintManager.ShowHint(widget.UnavailabilityReason);
    }

    protected virtual void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(onClick);
    }

    public void Initialize(T widget)
    {
        this.widget = widget;
        Initialized = true;
        button.interactable = widget.IsInteractable;
    }

    void onClick()
    {
        if (Interactable)
        {
            if (widget.OnClick == null)
                Debug.LogWarning("No onClick action defined for this button widget " + widget);
            else
                widget.OnClick.Invoke();

            if (widget.CloseParentMenuOnClick)
            {
                var popup = GetComponentInParent<IClosable>();
                if (popup != null)
                    popup.Close();
                else
                    Debug.LogWarning("No parent menu found in parents for this button widget " + widget);
            }
        }
    }
}