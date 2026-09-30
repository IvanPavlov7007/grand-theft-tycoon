using System;
using UnityEngine;

public class PopUpContextMenu : MonoBehaviour, IClosable
{
    public RectTransform RectTransform { get; private set; }
    public RectTransform ContentTransform { get; private set; }
    public Transform TargetTransform { get; private set; }

    public event Action<PopUpContextMenu> Closed;

    private void Awake()
    {
        RectTransform = GetComponent<RectTransform>();
    }

    public void Initialize(Transform targetTransform, RectTransform contentTransform, Action<PopUpContextMenu> onClose)
    {
        TargetTransform = targetTransform;
        ContentTransform = contentTransform;

        if (onClose != null)
            Closed += onClose;
    }

    public void Close()
    {
        Closed?.Invoke(this);
    }
}