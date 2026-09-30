using UnityEngine;

[CreateAssetMenu(fileName = "CarShopGameModeData", menuName = "Configs/GameMode/Car Shop Game Mode Data")]
public sealed class CarShopGameModeData : ScriptableObject
{
    public Vector2 carSpawnPoint;
    public SimpleCarSpawnConfig carSpawnConfig;
    public WarehouseConfig carShopWarehouseConfig;
}