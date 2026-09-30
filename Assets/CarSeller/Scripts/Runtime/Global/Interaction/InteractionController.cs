using System;
using Pixelplacement;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Lives in the scene and manages all interactable objects
/// </summary>
public class InteractionController : MonoBehaviour
{
    public List<Interactable> interactables = new List<Interactable>();
    public List<ITriggerable> triggerables = new List<ITriggerable>();

    private IInteractionManager interactionManager => G.InteractionManager;

    public void RegisterInteractable(Interactable interactable)
    {
        if (!interactables.Contains(interactable))
        {
            interactables.Add(interactable);

            interactable.CursorClicked += OnClick;
            interactable.CursorDragStarted += OnDragStart;
            interactable.CursorDragEnded += OnDragEnd;
        }
    }

    public void RegisterTriggerable(ITriggerable triggerable)
    {
        if (!triggerables.Contains(triggerable))
        {
            triggerables.Add(triggerable);
            triggerable.OnTriggerEntered += OnTriggerEntered;
        }
    }
    

    public void UnregisterInteractable(Interactable interactable)
    {
        if (interactables.Contains(interactable))
        {
            interactables.Remove(interactable);
            interactable.CursorClicked -= OnClick;
            interactable.CursorDragStarted -= OnDragStart;
            interactable.CursorDragEnded -= OnDragEnd;
        }
    }

    public void UnregisterTriggerable(ITriggerable triggerable)
    {
        if (triggerables.Contains(triggerable))
        {
            triggerables.Remove(triggerable);
            triggerable.OnTriggerEntered -= OnTriggerEntered;
        }
    }

    void OnClick(Interactable interactable)
    {
        interactionManager.OnProductViewClick(interactable);
    }

    void OnDragStart(Interactable interactable)
    {
        interactionManager.OnDragStart(interactable);
    }

    void OnDragEnd(Interactable interactable)
    {
        interactionManager.OnDragEnd(interactable);
    }

    void OnTriggerEntered(ModelProvider trigger, ModelProvider triggerCause)
    {
        interactionManager.OnTriggerEntered(trigger, triggerCause);
    }
}
