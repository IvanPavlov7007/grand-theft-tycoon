using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//Should I also implement it as a system, like with ownership?

public class WorldRegistry
{
    private Dictionary<(Type runtimeType, ScriptableObject config), List<object>> configsMap
        = new();

    private Dictionary<string, object> nameMap
        = new();

    public void Register<T>(T instance, ScriptableObject config)
        where T : IRegisterable
    {
        var key = (typeof(T), config);

        if (!configsMap.TryGetValue(key, out var list))
        {
            list = new List<object>();
            configsMap[key] = list;
        }

        list.Add(instance);
        nameMap[instance.Name] = instance;
    }

    public IReadOnlyList<T> GetByConfig<T>(ScriptableObject config)
    {
        var key = (typeof(T), config);

        return configsMap.TryGetValue(key, out var list)
            ? list.Cast<T>().ToList()
            : Array.Empty<T>();
    }

    public T GetByName<T>(string name) where T : class
    {
        if (nameMap.TryGetValue(name, out var instance))
        {
            if(instance is not T)
                throw new InvalidOperationException($"WorldRegistry.GetByName: Instance with name {name} is not of type {typeof(T).Name}.");
            return instance as T;
        }
        return null;
    }

}