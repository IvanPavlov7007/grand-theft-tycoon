using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerStartStateConfig", menuName = "Configs/Player/PlayerStartStateConfig")]
public class PlayerStartState : ScriptableObject
{
    public float initialMoney = 0f;
    public List<WarehouseConfig> ownWarehouses;
}
