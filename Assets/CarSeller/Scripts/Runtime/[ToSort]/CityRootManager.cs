using System;
using UnityEngine;

public class CityRootManager : MonoBehaviour
{
    private void Awake()
    {
        // Instantiate(GameMainConfig.Instance.GameConfig.CityConfig.CityGraph.PrefabRoot,transform);
        G.CityRoot = gameObject;
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        G.CityRoot = null;
    }
}