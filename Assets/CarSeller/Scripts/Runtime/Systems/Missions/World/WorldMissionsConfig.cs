using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WorldMissionsConfig", menuName = "Configs/Missions/World Missions Config")]
public class WorldMissionsConfig : ScriptableObject
{
    public List<MissionConfig> allMissions = new List<MissionConfig>();
    // Check that all starting missions are in allMissions
    public List<MissionConfig> startingMissions = new List<MissionConfig>();

    [Header("Shared Data")]
    public PinStyle finishPinStyle;
}