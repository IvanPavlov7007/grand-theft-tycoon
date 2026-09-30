using System;
using UnityEngine;

public class PauseMenuManager : GlobalSingletonBehaviour<PauseMenuManager>, IClosable
{
    protected override PauseMenuManager GlobalInstance { get => G.PauseMenuManager; set => G.PauseMenuManager = value; }

    CanvasGroup canvasGroup;
    Canvas canvas;

    protected override void Awake()
    {
        base.Awake();
        if (!IsActiveSingleton) return;

        canvasGroup = GetComponent<CanvasGroup>();
        canvas = GetComponent<Canvas>();
    }

    private void OnEnable()
    {
        GameEvents.Instance.OnGamePaused += onPaused;
        GameEvents.Instance.OnGameUnpaused += onResumed;
        onResumed();
    }

    private void OnDisable()
    {
        GameEvents.Instance.OnGamePaused -= onPaused;
        GameEvents.Instance.OnGameUnpaused -= onResumed;
    }

    public void Close()
    {
        G.GameManager.Pause(false);
    }

    void onPaused()
    {
        G.BlockUIManager.Block(canvas,()=> G.GameManager.Pause(false));
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
    }

    void onResumed()
    {
        G.BlockUIManager.Unblock(canvas);
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
    }
    
}