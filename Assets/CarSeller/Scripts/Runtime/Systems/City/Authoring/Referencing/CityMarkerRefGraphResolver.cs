#if UNITY_EDITOR
using System;
using UnityEditor;

public static class CityMarkerRefGraphResolver
{
    /// <summary>
    /// Try to resolve a CityGraphAsset based on where the owner ScriptableObject lives.
    /// Currently: if owner is a WarehouseConfig that is referenced by some CityConfig,
    /// return that CityConfig.CityGraph.
    /// </summary>
    public static CityGraphAsset TryResolveGraphFromOwner(UnityEngine.Object owner)
    {
        if (owner == null) return null;

        // 1. If the owner IS a CityConfig, use its CityGraph directly.
        if (owner is CityConfig directCityConfig && directCityConfig.CityGraph != null)
            return directCityConfig.CityGraph;

        var ownerPath = AssetDatabase.GetAssetPath(owner);
        if (string.IsNullOrEmpty(ownerPath))
            return null;

        // 2. Scan CityConfig assets to see which one references this owner (e.g. in warehouseConfigs)
        var guids = AssetDatabase.FindAssets("t:CityConfig");
        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var cityConfig = AssetDatabase.LoadAssetAtPath<CityConfig>(path);
            if (cityConfig == null) continue;

            if (cityConfig.warehouseConfigs != null &&
                Array.IndexOf(cityConfig.warehouseConfigs, owner) >= 0)
            {
                return cityConfig.CityGraph;
            }
        }

        return null;
    }
}
#endif