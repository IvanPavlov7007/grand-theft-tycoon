using UnityEngine.InputSystem;

public class UIInputController : GlobalSingletonBehaviour<UIInputController>
{
    protected override UIInputController GlobalInstance { get => G.UIInputController; set => G.UIInputController = value; }

    public event System.Action onPaused;
    public event System.Action onResumed;

    public void OnPause(InputValue value)
    {
        onPaused?.Invoke();
    }

    public void OnResume(InputValue value)
    {

        onResumed?.Invoke();
    }
}
