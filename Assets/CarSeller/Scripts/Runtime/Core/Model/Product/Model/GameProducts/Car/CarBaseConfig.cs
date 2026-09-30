using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CarBaseConfig", menuName = "Configs/Products/Car/Car Base Config")]
public class CarBaseConfig : SerializedScriptableObject, IBaseConfig
{
    public string Name = "New Car";
    public float BasePrice = 1000f;
    public float Speed = 2f;
    public float Acceleration = 0.3f;
    public CarFrameBaseConfig CarFrameBaseConfig;
    public Sprite TopView;
    public Sprite SideView;
    [ShowInInspector]
    [OdinSerialize]
    public List<PartSlotBaseConfig> SlotConfigs = new List<PartSlotBaseConfig>();

    public CarKind Kind;
}