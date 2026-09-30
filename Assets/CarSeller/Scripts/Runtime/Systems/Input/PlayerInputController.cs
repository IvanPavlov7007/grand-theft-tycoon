using UnityEngine.InputSystem;
using System;

public class PlayerInputController : GlobalSingletonBehaviour<PlayerInputController>
{
    protected override PlayerInputController GlobalInstance { get => G.PlayerInputController; set => G.PlayerInputController = value; }

    public event Action crouched;
    public event Action jumped;
    public event Action restarted;

    //Debugging purpose
    public void OnCrouch(InputValue inputValue)
    {
        crouched?.Invoke();
    }

    public void OnJump(InputValue inputValue)
    {
        jumped?.Invoke();
    }

    public void OnRestart(InputValue inputValue)
    {
        restarted?.Invoke();
    }
}