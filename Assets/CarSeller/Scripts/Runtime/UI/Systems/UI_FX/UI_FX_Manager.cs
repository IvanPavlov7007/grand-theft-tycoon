using UnityEngine;

public class UI_FX_Manager : GlobalSingletonBehaviour<UI_FX_Manager>
{
    protected override UI_FX_Manager GlobalInstance { get => G.UIFXManager; set => G.UIFXManager = value; }

    private void OnEnable() { GameEvents.Instance.OnTransactionComplete += onTransactionComplete; }
    private void OnDisable()
    {
        GameEvents.Instance.OnTransactionComplete -= onTransactionComplete;
    }

    void onTransactionComplete(TransactionEventData data)
    {
        var transaction = data.Transaction;
        if (transaction == null || transaction.Result == null)
            return;

        if (transaction.Result.Type != TransactionResultType.Success)
        {
            Debug.LogWarning($"Transaction failed as {transaction.Result.Type} with {transaction.Result.Data}");
            return;
        }

        switch (transaction)
        {
            case PurchaseTransaction:
                // TODO: add purchase-specific FX if needed
                break;

            case StealTransaction:
                // TODO: add steal-specific FX if needed
                break;

            case SellTransaction sellData:
                {
                    playMoneyEffect(sellData.AbsolutePrice, data.TransactionFeedbackLocation);
                    break;
                }

            case RewardTransaction rewardData:
                {
                    playMoneyEffect(SellPriceWrapper.CalculateAbsolutePrice(rewardData.Price), data.TransactionFeedbackLocation);
                    break;
                }

            default:
                break;
        }
    }

    void playMoneyEffect(float amount, TransactionFeedbackLocation location)
    {
        if (location == null)
        {
            Debug.LogWarning("Transaction location is null, defaulting to OmniDirectional.");
            location = TransactionFeedbackLocation.OmniDirectional;
        }

        if (location.Type == TransactionLocationType.WorldSpace)
        {
            G.UIEffectsDisplayer.PlayMoneyEffectWorld(amount, location.Position);
        }
        else
        {
            G.UIEffectsDisplayer.PlayMonetEffectScreen(amount, location.Position);
        }
    }
}
