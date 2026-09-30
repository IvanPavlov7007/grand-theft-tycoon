public interface IInteractionManager
{
    void OnProductViewClick(Interactable interactable);
    void OnDragStart(Interactable interactable);
    void OnDragEnd(Interactable interactable);
    void OnTriggerEntered(ModelProvider trigger, ModelProvider triggerCause);
}