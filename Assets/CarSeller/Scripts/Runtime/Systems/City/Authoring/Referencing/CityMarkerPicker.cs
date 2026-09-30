using System.Collections.Generic;
using UnityEngine;

public static class CityMarkerPicker
{
    /// <summary>
    /// Resolve which graph to use for this marker reference.
    /// </summary>
    public static CityGraphAsset ResolveGraph(CityMarkerRef markerRef, Object owner)
    {
        // 1. Field-specific override
        if (markerRef.Graph != null)
            return markerRef.Graph;

        // 2. Try owner-level shared graph (e.g., WarehouseConfig.Graph)
        if (owner is IHasCityGraph ownerWithGraph && ownerWithGraph.CityGraph != null)
            return ownerWithGraph.CityGraph;

        // 3. Fallback: your own global/default mechanism (e.g., a singleton config)
        return GetGlobalDefaultGraph();
    }

    /// <summary>
    /// Build dropdown label/value pairs for a graph's markers.
    /// </summary>
    public static List<(string label, string id)> BuildMarkerOptions(CityGraphAsset graph)
    {
        var result = new List<(string label, string id)>();
        if (graph == null || graph.Markers == null)
            return result;

        foreach (var m in graph.Markers)
        {
            var tags = m.Tags != null && m.Tags.Length > 0
                ? $" [{string.Join(",", m.Tags)}]"
                : string.Empty;

            var label = $"{m.DisplayName}{tags} ({m.Id})";
            result.Add((label, m.Id));
        }

        return result;
    }

    private static CityGraphAsset GetGlobalDefaultGraph()
    {
        // You can implement whatever you like here, for example:
        // - Resources.Load<CityGraphAsset>("DefaultCityGraph");
        // - An addressables lookup
        // - A ScriptableObject singleton
        var graphs = Resources.LoadAll<CityGraphAsset>("ScriptableObjects");
        if (graphs != null && graphs.Length > 0)
            return graphs[0];
        else
            Debug.LogError("CityMarkerPicker: No global default CityGraphAsset found in Resources/ScriptableObjects.");
        return null;
    }
}

/// <summary>
/// Optional interface for ScriptableObjects/MonoBehaviours that expose a shared CityGraph.
/// </summary>
public interface IHasCityGraph
{
    CityGraphAsset CityGraph { get; }
}