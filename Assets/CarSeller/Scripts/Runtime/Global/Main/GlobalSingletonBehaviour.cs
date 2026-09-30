using UnityEngine;

public abstract class GlobalSingletonBehaviour<T> : MonoBehaviour where T : MonoBehaviour
{
    [SerializeField] private bool dontDestroyOnLoad;

    protected bool IsActiveSingleton { get; private set; }

    protected abstract T GlobalInstance { get; set; }

    protected virtual void Awake()
    {
        var self = this as T;
        if (self == null)
        {
            Debug.LogError($"{GetType().Name} must inherit GlobalSingletonBehaviour with its own type.");
            return;
        }

        var existing = FindObjectsOfType<T>(true);
        var keeper = self;
        var bestId = self.GetInstanceID();

        for (var i = 0; i < existing.Length; i++)
        {
            var candidate = existing[i];
            if (candidate == null) continue;

            var id = candidate.GetInstanceID();
            if (id < bestId)
            {
                bestId = id;
                keeper = candidate;
            }
        }

        if (!ReferenceEquals(keeper, self))
        {
            IsActiveSingleton = false;
            Destroy(gameObject);
            return;
        }

        IsActiveSingleton = true;

        if (dontDestroyOnLoad)
        {
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
        }

        GlobalInstance = self;
    }

    protected virtual void OnDestroy()
    {
        var self = this as T;
        if (!IsActiveSingleton || self == null) return;

        if (ReferenceEquals(GlobalInstance, self))
        {
            GlobalInstance = null;
        }
    }
}

