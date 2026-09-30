using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "GameConfig", menuName = "Configs/Game Config")]
public class GameConfig : SerializedScriptableObject
{
    public CityConfig CityConfig;
    public EconomyConfig EconomyConfig;
    public WorldMissionsConfig WorldMissionsConfig;
    public VehicleControllerConfig VehicleControllerConfig;
    public GameDatabaseContainer GameDatabaseContainer;
    public ColorPalette ColorPalette;
    [Header("Game Modes Data")]
    public ScriptableObject GameModeData;
}