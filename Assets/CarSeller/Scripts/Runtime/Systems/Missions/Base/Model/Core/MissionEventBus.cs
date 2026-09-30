using System;
using System.Collections.Generic;

//ChatGPT Generated
//This is internal event bus for mission-related events.
public class MissionEventBus
{
    private readonly Dictionary<Type, Action<MissionInternalEvent>> listeners
        = new Dictionary<Type, Action<MissionInternalEvent>>();

    // Maps: event type -> (user listener -> wrapped listener)
    private readonly Dictionary<Type, Dictionary<Delegate, Action<MissionInternalEvent>>> listenerWrappers
        = new Dictionary<Type, Dictionary<Delegate, Action<MissionInternalEvent>>>();

    private void OnEvent<T>(T requestEvent)
        where T : MissionInternalEvent
    {
        var eventType = typeof(T);
        if (listeners.TryGetValue(eventType, out var action) && action != null)
        {
            action.Invoke(requestEvent);
        }
    }

    public void Subscribe<T>(Action<T> listener)
        where T : MissionInternalEvent
    {
        if (listener == null) throw new ArgumentNullException(nameof(listener));

        var eventType = typeof(T);

        // Create wrapper once and store it
        Action<MissionInternalEvent> wrapper = e => listener((T)e);

        if (!listeners.TryGetValue(eventType, out var existing))
        {
            existing = null;
        }

        existing += wrapper;
        listeners[eventType] = existing;

        if (!listenerWrappers.TryGetValue(eventType, out var wrapperMap))
        {
            wrapperMap = new Dictionary<Delegate, Action<MissionInternalEvent>>();
            listenerWrappers[eventType] = wrapperMap;
        }

        // Overwrite if same listener is subscribed again; last wrapper wins
        wrapperMap[listener] = wrapper;
    }

    public void Unsubscribe<T>(Action<T> listener)
        where T : MissionInternalEvent
    {
        if (listener == null) throw new ArgumentNullException(nameof(listener));

        var eventType = typeof(T);

        if (!listeners.TryGetValue(eventType, out var existing) || existing == null)
            return;

        if (!listenerWrappers.TryGetValue(eventType, out var wrapperMap))
            return;

        if (!wrapperMap.TryGetValue(listener, out var wrapper))
            return;

        // Remove the stored wrapper delegate
        existing -= wrapper;

        if (existing == null)
        {
            listeners.Remove(eventType);
        }
        else
        {
            listeners[eventType] = existing;
        }

        wrapperMap.Remove(listener);
        if (wrapperMap.Count == 0)
        {
            listenerWrappers.Remove(eventType);
        }
    }

    public void Emit<T>(T requestEvent)
        where T : MissionInternalEvent
    {
        if (requestEvent == null) throw new ArgumentNullException(nameof(requestEvent));
        OnEvent(requestEvent);
    }
}