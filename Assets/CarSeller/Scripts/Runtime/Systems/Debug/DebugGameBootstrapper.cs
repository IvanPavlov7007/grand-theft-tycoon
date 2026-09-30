public class DebugGameBootstrapper : GlobalSingletonBehaviour<DebugGameBootstrapper>
{
    protected override DebugGameBootstrapper GlobalInstance { get => G.DebugGameBootstrapper; set => G.DebugGameBootstrapper = value; }

    protected override void Awake()
    {
        base.Awake();
        if (!IsActiveSingleton)
        {
            return;
        }
        var currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

    }
}