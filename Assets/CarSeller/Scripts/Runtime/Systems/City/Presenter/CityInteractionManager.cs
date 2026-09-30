using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manager that handles logic of interactions in the city scene,
/// using registry-based profiles per GameState (similar to CitySceneManager).
/// </summary>
public class CityInteractionManager : IInteractionManager
{
    readonly CityContextMenuProfileRegistry _contextMenuRegistry = new CityContextMenuProfileRegistry();
    readonly CityTriggerProfileRegistry _triggerRegistry = new CityTriggerProfileRegistry();

    public void OnDragEnd(Interactable interactable)
    {
        if (interactable == null)
            return;

        var gameState = G.GameState;
        var profile = _triggerRegistry.Get(gameState);
        if (profile == null)
        {
            Debug.LogError("CityInteractionManager: No trigger profile available for current game state");
            return;
        }

        // We need a ModelProvider for the dragged object to act as triggerCause.
        var triggerCause = interactable.GetComponent<ModelProvider>();
        if (triggerCause == null)
        {
            // Not all interactables are necessarily city entities.
            return;
        }

        var triggerables = collidingTriggerables(interactable);

        for (int i = 0; i < triggerables.Count; i++)
        {
            var triggerable = triggerables[i];
            if (triggerable == null)
                continue;

            // Try to resolve the trigger's ModelProvider from the collider that was hit.
            var trigger = triggerable.GetComponentInParent<ModelProvider>();
            if (trigger == null)
                continue;

            // Ignore self-drop.
            if (ReferenceEquals(trigger, triggerCause))
                continue;


            var triggerAction = profile.GenerateTriggerAction(
                new TriggerContext(
                    trigger.CityEntity,
                    triggerCause.CityEntity,
                    gameState,
                    trigger.ViewGameObject,
                    triggerCause.ViewGameObject,
                    TriggerContext.TriggerKind.DragEnd));


            if (triggerAction == null)
                continue;

            if (!triggerAction.CanProceed)
                continue;
            
            triggerAction.Action?.Invoke();
            return; // fire only one trigger on drop
        }
    }

    private IReadOnlyList<Triggerable> collidingTriggerables(Interactable interactable)
    {
        var triggerCausable = interactable.GetComponent<TriggerCausable>();
        if (triggerCausable == null)
            return new List<Triggerable>();
        return triggerCausable.CurrentTriggerables;
    }

    public void OnDragStart(Interactable interactable)
    {
        // Not used in city at the moment.
    }

    public void OnProductViewClick(Interactable interactable)
    {
        var gameState = G.GameState;
        var profile = _contextMenuRegistry.Get(gameState);
        if (profile == null)
        {
            Debug.LogError("CityInteractionManager: No context menu profile available for current game state");
            return;
        }

        var contentProvider = interactable.GetComponent<ModelProvider>();
        if (contentProvider == null)
        {
            Debug.LogError("CityInteractionManager: Interactable has no ContentProvider, cannot build menu");
            return;
        }

        var model = contentProvider.CityEntity;
        if (model == null)
        {
            Debug.LogError("CityInteractionManager: ContentProvider.Model is null");
            return;
        }

        var content = profile.GenerateContent(model, gameState);
        if (content != null)
        {
            G.FixedContextMenuManager.CreateContextMenu(content);
        }
    }

    public void OnTriggerEntered(ModelProvider trigger, ModelProvider triggerCause)
    {
        var gameState = G.GameState;
        var profile = _triggerRegistry.Get(gameState);
        if (profile == null)
        {
            Debug.LogError("CityInteractionManager: No trigger profile available for current game state");
            return;
        }

        if (trigger == null)
        {
            Debug.LogError("CityInteractionManager: trigger is null");
            return;
        }

        var triggerModel = trigger.CityEntity;
        var causeModel = triggerCause != null ? triggerCause.CityEntity : null;

        var triggerAction = profile.GenerateTriggerAction(
            new TriggerContext(
                triggerModel,
                causeModel,
                gameState,
                trigger.ViewGameObject,
                triggerCause != null ? triggerCause.ViewGameObject : null,
                TriggerContext.TriggerKind.Enter));
        if (triggerAction == null)
        {
            Debug.LogError("CityInteractionManager: TriggerAction is null");
            return;
        }

        if (!triggerAction.CanProceed)
            return;

        if (triggerAction.Action == null)
        {
            Debug.LogError("CityInteractionManager: TriggerAction.Action is null");
            return;
        }

        triggerAction.Action.Invoke();
    }
}