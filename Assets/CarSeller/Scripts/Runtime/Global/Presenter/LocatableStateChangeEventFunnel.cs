using Pixelplacement;
using System;
using UnityEngine;

[Obsolete]
/// <summary>
/// Funnel or hopper that listens to locatable state change related events and funnels them into a single event
/// Useful for presenters that need to respond to any locatable state change
/// Such as SceneManagers
/// </summary>
public class LocatableStateChangeEventFunnel : MonoBehaviour
{
    private void OnEnable()
    {
        GameEvents.Instance.OnOwnershipChanged += OnLocatableAcquired;
    }

    private void OnDisable()
    {
        GameEvents.Instance.OnOwnershipChanged -= OnLocatableAcquired;
    }

    private void OnLocatableAcquired(OwnershipChangedEventData data)
    {
        //GameEvents.Instance.OnLocatableStateChanged?.Invoke(new LocatableStateChangedEventData(data.Possession as ILocatable));
    }

    


}