using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CarVariantConfig", menuName = "Configs/Products/Car/Car Variant Config")]
public class CarVariantConfig : SerializedScriptableObject, IVariantConfig
{
    public CarBaseConfig baseFallbackConfig;
    public bool OverrideBasePrice;
    [ShowIf("OverrideBasePrice")]
    public float BasePrice;
    public bool OverrideSpeed;
    [ShowIf("OverrideSpeed")]
    public float Speed;
    public bool OverrideAcceleration;
    [ShowIf("OverrideAcceleration")]
    public float Acceleration;

    public CarFrameVariantConfig carFrameRuntimeConfig;
    [OdinSerialize]
    public List<PartSlotVariantConfig> slotConfigs = new List<PartSlotVariantConfig>();
}