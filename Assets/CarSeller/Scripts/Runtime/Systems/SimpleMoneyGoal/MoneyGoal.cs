using UnityEngine;

[CreateAssetMenu(fileName = "NewMoneyGoal", menuName = "Configs/Economy/Simple Money Goal/Money Goal")]
public class MoneyGoal : ScriptableObject
{
    public string GoalID;
    public float TargetAmount;
    public string Description;
    public bool IsCompleted(float currentAmount)
    {
        return currentAmount >= TargetAmount;
    }
}