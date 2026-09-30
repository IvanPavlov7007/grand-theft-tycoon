using Sirenix.OdinInspector;
using UnityEngine;


/// <summary>
/// Interactable that can be dragged and provides drag direction
/// </summary>
///
public class DragInteractable : Interactable, IDirectionProvider, IActivatable
{

    bool active = true;
    bool dragging = false;

    [ShowInInspector]
    public Vector2 ProvidedDirection { get; private set; }

    private void Update()
    {
        // Security check that dragging is still valid
        // fast fix for issue when switching windows while dragging
        if (dragging && G.GameCursor.draggedInteractable == null)
        {
            OnCursorDragEnd();
        }
    }

    private void LateUpdate()
    {
        if (dragging)
        {
            ProvidedDirection = (G.GameCursor.transform.position - transform.position);
        }
    }

    protected override void OnCursorDragStart()
    {
        if (!active)
            return;
        base.OnCursorDragStart();
        ProvidedDirection = Vector2.zero;
        dragging = true;

    }
    protected override void OnCursorDragEnd()
    {
        base.OnCursorDragEnd();
        dragging = false;
        ProvidedDirection = Vector2.zero;
    }

    public void Activate()
    {
        active = true;
    }

    public void Deactivate()
    {
        active = false;
        OnCursorDragEnd();
    }

    public void SetActive(bool active)
    {
        if (active)
        {
            Activate();
        }
        else
        {
            Deactivate();
        }
    }
}