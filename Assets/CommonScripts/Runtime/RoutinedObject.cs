using System.Collections;
using UnityEngine;

// A tiny host MonoBehaviour to run coroutines. You cannot AddComponent<MonoBehaviour> directly.
public class RoutineHost : MonoBehaviour {
}

/// <summary>
/// A base class for objects that need to run coroutines.
/// </summary>
public class RoutinedObject
{
    private MonoBehaviour routineHolder;
    private Coroutine currentRoutine;

    public RoutinedObject()
    {
        var go = new GameObject(this.GetType().Name + "'s routine object");
        routineHolder = go.AddComponent<RoutineHost>();
        Object.DontDestroyOnLoad(go);
    }

    /// <summary>
    /// Starts a routine only if no routine is currently running on this object.
    /// </summary>
    protected bool TryStartRoutine(IEnumerator routine)
    {
        if (currentRoutine != null)
        {
            return false;
        }
        currentRoutine = routineHolder.StartCoroutine(RunAndClear(routine));
        return true;
    }

    /// <summary>
    /// Starts a routine immediately and returns its Coroutine handle.
    /// Useful for child classes that need multiple concurrent routines.
    /// </summary>
    protected Coroutine StartRoutine(IEnumerator routine)
    {
        return routineHolder.StartCoroutine(routine);
    }

    private IEnumerator RunAndClear(IEnumerator routine)
    {
        yield return routine;
        currentRoutine = null;
    }
}