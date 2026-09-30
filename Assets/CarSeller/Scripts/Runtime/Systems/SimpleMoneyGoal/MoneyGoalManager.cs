using UnityEngine;

public class MoneyGoalManager : GlobalSingletonBehaviour<MoneyGoalManager>
{
    protected override MoneyGoalManager GlobalInstance { get => G.MoneyGoalManager; set => G.MoneyGoalManager = value; }

    public MoneyGoal CurrentMoneyGoal;

    MoneyGoalDisplayer moneyGoalDisplayer;

    protected override void Awake()
    {
        base.Awake();
        if (!IsActiveSingleton) return;

        moneyGoalDisplayer = FindAnyObjectByType<MoneyGoalDisplayer>();
    }

    private void Start()
    {
        Debug.Assert(CurrentMoneyGoal
            != null, "No CurrentMoneyGoal assigned to MoneyGoalManager!");
        Debug.Assert(moneyGoalDisplayer
            != null, "No MoneyGoalDisplayer found in scene for MoneyGoalManager!"); 

        moneyGoalDisplayer.UpdateDisplay(
            G.Player.Money, CurrentMoneyGoal.TargetAmount);
    }

    private void OnEnable()
    {
        GameEvents.Instance.OnPlayerMoneyChanged += onPlayerMoneyChanged;
    }

    private void OnDisable()
    {
        GameEvents.Instance.OnPlayerMoneyChanged -= onPlayerMoneyChanged;
    }

    private void onPlayerMoneyChanged(PlayerMoneyChangeEventData data)
    {
        if(CurrentMoneyGoal == null)
            return;
        if(moneyGoalDisplayer != null)
        {
            moneyGoalDisplayer.UpdateDisplay(data.NewMoney, CurrentMoneyGoal.TargetAmount);
        }

        if (CurrentMoneyGoal.IsCompleted(data.NewMoney))
        {
            G.CongradulationsMenu.Show();
        }
    }
}