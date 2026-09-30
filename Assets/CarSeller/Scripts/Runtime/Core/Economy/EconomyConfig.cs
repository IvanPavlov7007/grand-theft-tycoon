using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "EconomyConfig", menuName = "Configs/Economy/EconomyConfig")]
public class  EconomyConfig : SerializedScriptableObject
{
    public WarehouseOffersConfig WarehouseOffersConfig;
    public PlayerStartState PlayerStartState;
    public CarSpawnConfig CarSpawnConfig;
    public CarShopOffersConfig CarShopOffersConfig;
}