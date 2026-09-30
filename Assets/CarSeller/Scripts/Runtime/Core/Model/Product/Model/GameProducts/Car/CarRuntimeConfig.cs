using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CarRuntimeConfig : IRuntimeConfig, ISimplifiedCarModel
{
    public string Name;
    public float BasePrice;
    public float Speed;
    public float Acceleration;
    public Sprite TopView;
    public Sprite SideView;
    public CarFrameRuntimeConfig CarFrameRuntimeConfig;
    public List<PartSlotRuntimeConfig> SlotConfigs;

    List<CarModifier> _modifiers;
    Dictionary<Type, CarModifier> modifiersByType;

    public IReadOnlyList<CarModifier> Modifiers => _modifiers;

    public CarKind Kind;

    CarKind ISimplifiedCarModel.Kind => Kind;

    public void InitializeModifiers(List<CarModifier> modifiers)
    {
        this._modifiers = modifiers;
        modifiersByType = new Dictionary<Type, CarModifier>();
        foreach (var modifier in modifiers)
        {
            if(modifiersByType.ContainsKey(modifier.GetType()))
            {
                Debug.LogError($"CarRuntimeConfig {Name} has multiple modifiers of type {modifier.GetType()}");
                continue;
            }
            modifiersByType[modifier.GetType()] = modifier;
        }
    }

    internal T GetModifier<T>() where T : CarModifier
    {
        if(modifiersByType.TryGetValue(typeof(T), out var modifier))
        {
            return (T)modifier;
        }
        return null;
    }
}

public abstract class CarModifier { }

public class CanTurnAround : CarModifier
{
    
}

public class CanNarrowStreet : CarModifier
{
    
}