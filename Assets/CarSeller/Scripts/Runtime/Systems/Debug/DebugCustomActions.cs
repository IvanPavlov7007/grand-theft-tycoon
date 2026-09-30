public class DebugCustomActions : GlobalSingletonBehaviour<DebugCustomActions>
{
    protected override DebugCustomActions GlobalInstance { get => G.DebugCustomActions; set => G.DebugCustomActions = value; }

#if DEBUG

    private void Start()
    {
        G.PlayerInputController.crouched += onActionOne;
        G.PlayerInputController.jumped += onActionTwo;
    }

    private void OnDisable()
    {
        if (G.PlayerInputController == null)
            return;
        G.PlayerInputController.crouched -= onActionOne;
        G.PlayerInputController.jumped -= onActionTwo;
    }

    void onActionOne()
    {
        FindAnyObjectByType<Interactable>()?.CursorClick();
    }

    void onActionTwo()
    {

    }
#endif
}