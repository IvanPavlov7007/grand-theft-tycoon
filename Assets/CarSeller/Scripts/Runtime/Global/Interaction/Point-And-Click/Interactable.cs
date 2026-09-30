using Sirenix.OdinInspector;
using System;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    public int sortingOrder = 0;

    public event Action<Interactable> CursorEntered;
    public event Action<Interactable> CursorExited;
    public event Action<Interactable> CursorSelectStarted;
    public event Action<Interactable> CursorSelectEnded;
    public event Action<Interactable> CursorDragStarted;
    public event Action<Interactable> CursorDragEnded;

    // New: higher-level gestures
    public event Action<Interactable> CursorClicked;
    public event Action<Interactable> CursorHoldStarted;
    public event Action<Interactable> CursorHoldEnded;

    protected bool CursorHovering { get; private set; }

    [Sirenix.OdinInspector.BoxGroup("Debug")]
    [ShowInInspector]
    protected bool logEvents = false;

    protected virtual void OnEnable()
    {
        G.InteractionController.RegisterInteractable(this);
    }

    protected virtual void OnDisable()
    {
        G.InteractionController.UnregisterInteractable(this);
    }

    public void CursorSelectStart()
    {
        CursorSelectStarted?.Invoke(this);
        OnCursorSelectStart();
    }

    public void CursorSelectEnd()
    {
        CursorSelectEnded?.Invoke(this);
        OnCursorSelectEnd();
    }

    public void CursorEnter()
    {
        CursorEntered?.Invoke(this);
        CursorHovering = true;
        OnCursorEnter();
    }

    public void CursorExit()
    {
        CursorExited?.Invoke(this);
        CursorHovering = false;
        OnCursorExit();
    }

    public void DragStart()
    {
        CursorDragStarted?.Invoke(this);
        OnCursorDragStart();
    }

    public void DragEnd()
    {
        CursorDragEnded?.Invoke(this);
        OnCursorDragEnd();
    }

    public void Drag()
    {
        OnCursorDrag();
    }

    // New: Click + Hold APIs
    public void CursorClick()
    {
        CursorClicked?.Invoke(this);
        OnCursorClick();
    }

    public void CursorHoldStart()
    {
        CursorHoldStarted?.Invoke(this);
        OnCursorHoldStart();
    }

    public void CursorHold()
    {
        OnCursorHold();
    }

    public void CursorHoldEnd()
    {
        CursorHoldEnded?.Invoke(this);
        OnCursorHoldEnd();
    }

    protected virtual void OnCursorEnter()
    {
        if(logEvents)
        Debug.Log("OnCursorEnter");
    }
    protected virtual void OnCursorStay() 
    {
        if (logEvents)
            Debug.Log("OnCursorStay");
    }
    protected virtual void OnCursorExit() 
    {
        if (logEvents)
            Debug.Log("OnCursorExit");
    }
    protected virtual void OnCursorSelectEnd() 
    {
        if (logEvents)
            Debug.Log("OnCursorSelectEnd");
    }
    protected virtual void OnCursorSelectStart() 
    {
        if (logEvents)
            Debug.Log("OnCursorSelectStart");
    }
    protected virtual void OnCursorDrag() 
    {
        if (logEvents)
            Debug.Log("OnCursorDrag");
    }
    protected virtual void OnCursorDragStart() 
    {
        if (logEvents)
            Debug.Log("OnCursorDragStart");
    }
    protected virtual void OnCursorDragEnd() 
    {
        if (logEvents)
            Debug.Log("OnCursorDragEnd");
    }

    // New virtuals
    protected virtual void OnCursorClick() 
    {
        if (logEvents)
            Debug.Log("OnCursorClick");
    }
    protected virtual void OnCursorHoldStart() 
    {
        if (logEvents)
            Debug.Log("OnCursorHoldStart");
    }
    protected virtual void OnCursorHold() 
    {
        if (logEvents)
            Debug.Log("OnCursorHold");
    }
    protected virtual void OnCursorHoldEnd() 
    {
        if (logEvents)
            Debug.Log("OnCursorHoldEnd");
    }
}