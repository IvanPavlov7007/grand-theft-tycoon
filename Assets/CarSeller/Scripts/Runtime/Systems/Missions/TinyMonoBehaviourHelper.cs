using System;
using UnityEngine;

public class TinyMonoBehaviourHelper : MonoBehaviour
{
    public event Action<float> OnUpdateEvent;

    private void Update()
    {
        OnUpdateEvent?.Invoke(Time.deltaTime);
    }

    public static TinyMonoBehaviourHelper Create(string name)
    {
        GameObject go = new GameObject(name);
        return go.AddComponent<TinyMonoBehaviourHelper>();
    }
}