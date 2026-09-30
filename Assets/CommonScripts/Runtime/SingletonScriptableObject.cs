using UnityEngine;
public class SingletonScriptableObject<T> : ScriptableObject where T : SingletonScriptableObject<T>
{
    private static T _instance;
    public static T Instance
    {
        get
        {
            return Load();
        }
    }

    public static T Load()
    {
        if (_instance == null)
        {
            T[] assets = Resources.LoadAll<T>("");
            if (assets == null || assets.Length < 1)
            {
                throw new System.Exception($"SingletonScriptableObject: No instance of {typeof(T).Name} found in Resources.");
            }
            else if (assets.Length > 1)
            {
                Debug.LogWarning($"SingletonScriptableObject: Multiple instances of {typeof(T).Name} found in Resources. Using the first one.");
            }
            _instance = assets[0];
        }
        return _instance;
    }

}