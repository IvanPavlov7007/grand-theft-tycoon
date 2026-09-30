using System;
using UnityEngine;

public enum ViewObjectVisualState
{
    Normal,
    Selected,
    Disabled,
}

public class CityViewObjectController : MonoBehaviour, ModelProvider
{
    public CityEntity CityEntity{ get; private set; }

    public event Action<ViewObjectVisualState> OnVisualStateChanged;
    public event Action<bool> OnDraggableStateChanged;
    public event Action OnDestroyed;

    public ViewObjectVisualState CurrentVisualState { get; private set; }
    public bool IsDraggable { get; private set; }

    public ILocatable Locatable => CityEntity.Subject;
    public GameObject ViewGameObject => gameObject;

    public CityViewObjectController Initialize(CityEntity cityEntity, ViewObjectVisualState cityObjectVisualState = ViewObjectVisualState.Normal, bool isDraggable = false)
    {
        CityEntity = cityEntity;
        IsDraggable = isDraggable;
        CurrentVisualState = cityObjectVisualState;
        return this;
    }

    public void SetViewState(CityObjectState cityObjectState)
    {
        SetVisibility(cityObjectState.visualState);
        SetDraggable(cityObjectState.draggable);
    }

    public void SetVisibility(ViewObjectVisualState visualState)
    {
        switch (visualState)
        {
            case ViewObjectVisualState.Normal:
                break;
            case ViewObjectVisualState.Disabled:
                break;
            case ViewObjectVisualState.Selected:
                break;
            default:
                Debug.LogError($"CityViewObject: Unsupported visual state {visualState}");
                return;
        }
        CurrentVisualState = visualState;
        OnVisualStateChanged?.Invoke(visualState);
    }

    public void SetDraggable(bool draggable)
    {
        IsDraggable = draggable;
        OnDraggableStateChanged?.Invoke(draggable);
    }

    private void OnDestroy()
    {
        OnDestroyed?.Invoke();
    }
}