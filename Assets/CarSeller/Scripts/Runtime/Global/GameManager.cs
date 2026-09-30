using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Currently in the background of GameFlowManager, probably should merge
/// </summary>
public class GameManager : MonoBehaviour
{
    public enum TimeState
    {
        Normal, Paused, FastForward
    }

    TimeState currentTimeState = TimeState.Normal;
    public TimeState CurrentTimeState { get { return currentTimeState; } }

    private void Awake()
    {
        G.GameManager = this;
    }

    private void OnEnable()
    {
        if (G.UIInputController != null)
        {
            G.UIInputController.onPaused += onPausedInput;
            G.UIInputController.onResumed += onResumedInput;
        }
#if UNITY_EDITOR
        if (G.PlayerInputController != null)
        {
            G.PlayerInputController.restarted += onRestartInput;
        }
#endif
    }

    private void OnDisable()
    {
        if (G.UIInputController != null)
        {
            G.UIInputController.onPaused -= onPausedInput;
            G.UIInputController.onResumed -= onResumedInput;
        }
#if UNITY_EDITOR
        if (G.PlayerInputController != null)
        {
            G.PlayerInputController.restarted -= onRestartInput;
        }
#endif
    }

    private void OnDestroy()
    {
        if (ReferenceEquals(G.GameManager, this))
        {
            G.GameManager = null;
        }
    }

    private void onPausedInput()
    {
        Pause(true);
    }

    private void onResumedInput()
    {
        Pause(false);
    }

    private void onRestartInput()
    {
        ResetGame();
    }

    public void Pause(bool pause)
    {
        bool pausing = pause && currentTimeState != TimeState.Paused;
        bool unpausing = !pause && currentTimeState == TimeState.Paused;

        currentTimeState = pause ? TimeState.Paused : TimeState.Normal;
        changeTime();

        //if there was a change
        if (pausing || unpausing)
        {
            if(pause)
                G.PlayerInputLocator.PlayerInput.SwitchCurrentActionMap("UI");
            else
                G.PlayerInputLocator.PlayerInput.SwitchCurrentActionMap("Player");
        }

            // change notification
        if (pausing)
            GameEvents.Instance.OnGamePaused?.Invoke();
        else if (unpausing)
            GameEvents.Instance.OnGameUnpaused?.Invoke();
    }

    public void switchFastForward()
    {
        bool currentFastForward = currentTimeState == TimeState.FastForward;
        FastForward(!currentFastForward);
    }

    public void FastForward(bool fastForward)
    {
        if(currentTimeState != TimeState.Paused)
            currentTimeState = fastForward ? TimeState.FastForward : TimeState.Normal;
        changeTime();
    }

    public void ResetGame()
    {
        Pause(false);
        Debug.Log("Resetting game...");
        SceneManager.LoadScene(0);
        ServicedMain.GameReset();
    }

    void changeTime()
    {
        switch(currentTimeState)
        {
            case TimeState.Normal:
                Time.timeScale = 1f;
                break;
            case TimeState.Paused:
                Time.timeScale = 0f;
                break;
            case TimeState.FastForward:
                Time.timeScale = 10f;
                break;
        }
    }

}