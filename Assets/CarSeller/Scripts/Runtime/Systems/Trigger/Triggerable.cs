using System;
using UnityEngine;

public class Triggerable : MonoBehaviour, ITriggerable
{
    public event Action<ModelProvider, ModelProvider> OnTriggerEntered;
    CustomTrigger2D CustomTrigger2D;
    ModelProvider contentProvider;

    private void Awake()
    {
        CustomTrigger2D = GetComponentInChildren<CustomTrigger2D>();
        contentProvider = GetComponent<ModelProvider>();
        if (CustomTrigger2D == null)
            Debug.LogError("No CustomTrigger2D Provided to this " + gameObject.name);
    }

    private void OnEnable()
    {
        CustomTrigger2D?.onEnter.AddListener(OnTriggered);
        CustomTrigger2D?.onExit.AddListener(OnTriggerExited);
        CustomTrigger2D.onStay.AddListener(OnTriggerStayed);
        G.InteractionController.RegisterTriggerable(this);
    }

    private void OnDisable()
    {
        G.InteractionController.UnregisterTriggerable(this);
        CustomTrigger2D?.onEnter.RemoveListener(OnTriggered);
        CustomTrigger2D?.onExit.RemoveListener(OnTriggerExited);
        CustomTrigger2D.onStay.RemoveListener(OnTriggerStayed);
    }

    private void OnTriggerStayed(Collider2D collider)
    {
        TryRegisterTriggerCausable(collider);
    }

    private void OnTriggered(Collider2D collision)
    {
        TryRegisterTriggerCausable(collision);

        ModelProvider triggerCause = collision.GetComponentInParent<ModelProvider>();
        if (triggerCause != null)
            OnTriggerEntered?.Invoke(contentProvider, triggerCause);
    }

    private void TryRegisterTriggerCausable(Collider2D collision)
    {
        TriggerCausable triggerCause = collision.GetComponentInParent<TriggerCausable>();
        if (triggerCause != null)
            triggerCause.AddTriggerable(this);
    }

    private void OnTriggerExited(Collider2D other)
    {
        TriggerCausable triggerCause = other.GetComponentInParent<TriggerCausable>();
        if (triggerCause != null)
            triggerCause.RemoveTriggerable(this);
    }
}


public interface ITriggerable
{
    public event Action<ModelProvider, ModelProvider> OnTriggerEntered;
}