using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CarsDefinitionLibrary", menuName = "Configs/CarsDefinitionLibrary", order = 0)]
public class CarsDefinitionLibrary : SerializedScriptableObject
{
    [OdinSerialize] List<CarBaseConfig> mainBaseConfigs = new List<CarBaseConfig>();
    [OdinSerialize] List<CarBaseConfig> additionalBaseConfigs = new List<CarBaseConfig>();

    NonDuplicatesIdentifiedCarList<List<CarRuntimeConfig>> runtimeConfigs;
    Dictionary<string, CarRuntimeConfig> runtimeConfigsById = new Dictionary<string, CarRuntimeConfig>();
    public IReadOnlyNonDuplicatesIdentifiedCarList<List<CarRuntimeConfig>> RuntimeConfigs => runtimeConfigs;
    public IReadOnlyDictionary<string, CarRuntimeConfig> RuntimeConfigsById => runtimeConfigsById;


    public void Initialize()
    {
        initializeRuntimesMatrix();
        initializeAdditionalConfigs();
    }

    private void initializeAdditionalConfigs()
    {
        CarConfigResolver resolver = new CarConfigResolver();
        foreach (var additionalBaseConfig in additionalBaseConfigs)
        {
            var runtime = resolver.Resolve(additionalBaseConfig,null);
            if (runtimeConfigsById.ContainsKey(runtime.Name))
                {
                    Debug.LogWarning($"CarsDefinitionLibrary: additional config {additionalBaseConfig.name} resolved to runtime {runtime.Name} which is already defined by main configs. Skipping.");
                    continue;
            }
            runtimeConfigsById[runtime.Name] = runtime;
        }
    }

    private void initializeRuntimesMatrix()
    {
        CarConfigResolver resolver = new CarConfigResolver();
        runtimeConfigs = new NonDuplicatesIdentifiedCarList<List<CarRuntimeConfig>>();

        foreach (var mainBaseConfig in mainBaseConfigs)
        {
            insertRuntimeIntoAllConfigs(resolver.Resolve(mainBaseConfig,null));
        }
    }

    private void insertRuntimeIntoAllConfigs(CarRuntimeConfig runtimeConfig)
    {
        var identifier = runtimeConfig.Kind;
        if (runtimeConfigs.TryGetValue(identifier, out var runtimesList))
        {
            runtimesList.Add(runtimeConfig);
        }
        else
        {
            runtimeConfigs.Add(identifier, new List<CarRuntimeConfig>() { runtimeConfig });
        }
        runtimeConfigsById[runtimeConfig.Name] = runtimeConfig;
    }
}