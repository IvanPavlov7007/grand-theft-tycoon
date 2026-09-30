using UnityEngine;
using UnityEngine.EventSystems;
using System;

public class CityUIPinDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public event Action<PointerEventData> OnCustomDrag;
    public event Action<PointerEventData> OnCustomBeginDrag;
    public event Action<PointerEventData> OnCustomEndDrag;

    public void OnBeginDrag(PointerEventData eventData)
    {
        OnCustomBeginDrag?.Invoke(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        OnCustomDrag?.Invoke(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        OnCustomEndDrag?.Invoke(eventData);
    }
}