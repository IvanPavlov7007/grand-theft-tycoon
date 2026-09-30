using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonStateController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    Button button;
    UIElement uIElement;
    public bool Initialized { get; private set; }
    public bool Interactable => Initialized && uIElement.IsInteractable;

    // Track pointer for distinguishing tap vs scroll/drag
    private bool _pointerDown;
    private Vector2 _pointerDownPos;
    private const float DragThreshold = 10f; // pixels

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
            G.GlobalHintManager.ShowHint(uIElement.UnavailabilityReason);
    }

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(onClick);
    }

    public void Initialize(UIElement uIElement)
    {
        this.uIElement = uIElement;
        Initialized = true;
        button.interactable = uIElement.IsInteractable;
    }

    void onClick()
    {
        if (Interactable)
        {
            if (uIElement.OnClick == null)
                Debug.LogWarning("No onClick action defined for this ui element " + uIElement);
            else
                uIElement.OnClick.Invoke();

            if (uIElement.closePopupOnClick)
            {
                var popup = GetComponentInParent<IClosable>();
                if (popup != null)
                    popup.Close();
                else
                    Debug.LogWarning("No popup context menu found in parents for this ui element " + uIElement);
            }
        }
    }
}

public interface IClosable
{
    void Close();
}