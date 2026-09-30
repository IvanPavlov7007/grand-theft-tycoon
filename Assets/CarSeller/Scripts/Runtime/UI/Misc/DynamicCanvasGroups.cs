using UnityEngine;

public class DynamicCanvasGroups : GlobalSingletonBehaviour<DynamicCanvasGroups>
{
    protected override DynamicCanvasGroups GlobalInstance { get => G.DynamicCanvasGroups; set => G.DynamicCanvasGroups = value; }

    [SerializeField]
    CanvasGroup[] canvasGroups;
    protected override void Awake()
    {
        base.Awake();
        if (!IsActiveSingleton) return;

        if(canvasGroups == null || canvasGroups.Length == 0)
            canvasGroups = GetComponentsInChildren<CanvasGroup>();
    }
    public void SetAlpha(float alpha)
    {
        foreach (var cg in canvasGroups)
        {
            cg.alpha = alpha;
        }
    }
    public void SetInteractable(bool interactable)
    {
        foreach (var cg in canvasGroups)
        {
            cg.interactable = interactable;
        }
    }
    public void SetBlocksRaycasts(bool blocksRaycasts)
    {
        Debug.Log($"Setting blocks raycasts to {blocksRaycasts} for {canvasGroups.Length} canvas groups.");
        foreach (var cg in canvasGroups)
        {
            cg.blocksRaycasts = blocksRaycasts;
        }
    }
}