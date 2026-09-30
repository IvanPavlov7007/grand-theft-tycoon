using UnityEngine;

[CreateAssetMenu(fileName = "CarSelectionSettings", menuName = "Configs/View/CarSelectionSettings")]
public class CarSelectionSettings : SingletonScriptableObject<CarSelectionSettings>
{
    public Color selectionRingColor = Color.green;
    public float selectionRingScaleMultiplier = 1.2f;
    public float selectionRingHeightOffset = 0.1f;
    public int sortingOrder = 4;
    public GameObject selectionRingPrefab;
}