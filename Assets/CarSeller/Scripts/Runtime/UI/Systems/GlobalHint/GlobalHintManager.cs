using System.Collections;
using UnityEngine;
using Pixelplacement;

public class GlobalHintManager : GlobalSingletonBehaviour<GlobalHintManager>
{
    protected override GlobalHintManager GlobalInstance { get => G.GlobalHintManager; set => G.GlobalHintManager = value; }

    CanvasGroup canvasGroup;
    TMPro.TextMeshProUGUI hintText;
    Coroutine currentHintCoroutine;
    float fadeDuration = 0.25f;
    float displayDuration = 2f;

    ITransparencyController transparencyController;
    protected override void Awake()
    {
        base.Awake();
        if (!IsActiveSingleton) return;

        canvasGroup = GetComponent<CanvasGroup>();
        ITransparencyController transparencyController = Transparency.GetController(canvasGroup);
        hintText = GetComponentInChildren<TMPro.TextMeshProUGUI>();
        canvasGroup.alpha = 0;
    }

    public void ShowHint(string message)
    {
        if (currentHintCoroutine != null)
        {
            StopCoroutine(currentHintCoroutine);
        }
        currentHintCoroutine = StartCoroutine(ShowHintCoroutine(message));
    }

    IEnumerator ShowHintCoroutine(string message)
    {
        hintText.text = message;
        Tween.Value(0f, 1f, f => canvasGroup.alpha = f, fadeDuration, 0f);
        yield return new WaitForSeconds(fadeDuration + displayDuration);
        Tween.Value(1f,0f, f => canvasGroup.alpha = f, fadeDuration, 0f);
        yield return new WaitForSeconds(fadeDuration);
        currentHintCoroutine = null;
    }
}