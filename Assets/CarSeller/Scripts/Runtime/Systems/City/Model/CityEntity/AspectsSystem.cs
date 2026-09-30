using UnityEngine;

public class AspectsSystem : IDisposable
{
    public readonly CityEntityAspectsService AspectsService;
    public CityVisionCentersSystem centersSystem { get; private set; }
    public CityVisibleSystem visibleSystem { get; private set; }

    TinyMonoBehaviourHelper monoBehaviourHelper;

    public AspectsSystem()
    {
        monoBehaviourHelper = TinyMonoBehaviourHelper.Create("Aspects System's MonoBehaviour");
        GameObject.DontDestroyOnLoad(monoBehaviourHelper.gameObject);
        AspectsService = new CityEntityAspectsService();
        initializeSystems();
    }

    private void initializeSystems()
    {
        centersSystem = new CityVisionCentersSystem(AspectsService);
        visibleSystem = new CityVisibleSystem(AspectsService, centersSystem);

        monoBehaviourHelper.OnUpdateEvent += x => Update();
    }

    public void Dispose()
    {
        centersSystem?.Dispose();
        visibleSystem?.Dispose();
        if(monoBehaviourHelper != null)
            GameObject.Destroy(monoBehaviourHelper.gameObject);
    }

    public void Update()
    {
        visibleSystem.Update();
    }

}