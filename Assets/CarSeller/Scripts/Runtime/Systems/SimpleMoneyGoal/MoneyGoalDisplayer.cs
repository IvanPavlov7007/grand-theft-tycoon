using TMPro;
using UnityEngine;

public class MoneyGoalDisplayer : MonoBehaviour
{
    public UnityEngine.UI.Slider progressBar;
    public TextMeshProUGUI text;

    public void UpdateDisplay(float currentAmount, float goalAmount)
    {
        float progress = Mathf.Clamp01(currentAmount / goalAmount);
        progressBar.value = progress;
        text.text = $"{currentAmount.ToString("C0")} / {goalAmount.ToString("C0")}";
    }
}