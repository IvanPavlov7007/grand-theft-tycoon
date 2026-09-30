using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputLocator : GlobalSingletonBehaviour<PlayerInputLocator>
{
    protected override PlayerInputLocator GlobalInstance { get => G.PlayerInputLocator; set => G.PlayerInputLocator = value; }

    [SerializeField]
    PlayerInput playerInput;
    public PlayerInput PlayerInput => playerInput;
}