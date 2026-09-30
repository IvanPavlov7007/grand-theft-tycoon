using Sirenix.OdinInspector;
using System;
using System.Linq;
using UnityEngine;

[Serializable, InlineProperty]
public class CityMarkerRef
{
    // Optional explicit override. Usually left null to use auto-resolved or default graph.
    [HideLabel, AssetsOnly]
    [HorizontalGroup("Row", Width = 80)]
    [LabelText("Graph")]
    public CityGraphAsset Graph;

    [HorizontalGroup("Row")]
#if UNITY_EDITOR
    [ShowInInspector, ValueDropdown(nameof(GetMarkerOptions))]
#else
    [ShowInInspector]
#endif
    [LabelText("Marker"), LabelWidth(60)]
    public string MarkerId;

    [HideInInspector]
    public bool IsValid => !string.IsNullOrEmpty(MarkerId) && HasGraph;

    // --------- Runtime helpers ---------

    public bool HasGraph => ResolveGraph(null) != null;

    public CityGraphAsset.MarkerData GetMarkerData(UnityEngine.Object owner = null)
    {
        var graph = ResolveGraph(owner);
        if (graph == null || string.IsNullOrEmpty(MarkerId)) return null;
        return graph.Markers?.FirstOrDefault(m => m.Id == MarkerId);
    }

    // --------- Shared logic (editor + runtime) ---------

    /// <summary>
    /// Resolve graph from explicit Graph, from owner, or from global fallback.
    /// </summary>
    public CityGraphAsset ResolveGraph(UnityEngine.Object owner)
    {
        // 1) Explicit override on this ref
        if (Graph != null)
            return Graph;

#if UNITY_EDITOR
        // 2) Owner-based resolution (e.g. via CityConfig)
        if (owner != null)
        {
            var cityGraphFromOwner = CityMarkerRefGraphResolver.TryResolveGraphFromOwner(owner);
            if (cityGraphFromOwner != null)
                return cityGraphFromOwner;
        }
#endif

        // 3) Global fallback (resources or first asset in project)
        return CityMarkerRefDefaults.GetDefaultGraph();
    }

#if UNITY_EDITOR
    // Odin dropdown source – uses whichever graph ResolveGraph gives it (via default/override).
    private ValueDropdownList<string> GetMarkerOptions()
    {
        var list = new ValueDropdownList<string>();

        // For dropdown, we don't know the owner here, so pass null.
        // ResolveGraph(null) will use: Graph override OR global default.
        var graph = ResolveGraph(null);
        if (graph == null || graph.Markers == null)
            return list;

        foreach (var m in graph.Markers)
        {
            var tags = m.Tags != null && m.Tags.Length > 0
                ? $" [{string.Join(",", m.Tags)}]"
                : "";
            var label = $"{m.DisplayName}{tags} ({m.Id})";
            list.Add(label, m.Id);
        }

        return list;
    }
#endif

    public City.CityMarker GetMarker()
    {
        Debug.Assert(IsValid, "CityMarkerRef is not valid.");
        if (G.City.TryGetMarker(MarkerId, out var marker))
        {
            return marker;
        }
        Debug.LogError($"CityMarkerRef: Marker with ID '{MarkerId}' not found in graph.");
        return default;
    }

    public CityPosition GetCityPosition()
    {
        Debug.Assert(IsValid, "CityMarkerRef is not valid.");
        var pos = GetMarker().PositionOnGraph;
        if(pos == null)
        {
            Debug.LogError($"CityMarkerRef: Marker with ID '{MarkerId}' does not have a valid position on graph.");
            return default;
        }
        return pos.Value;
    }
}
public static class CityMarkerRefDefaults
{
    private static CityGraphAsset _cachedDefault;

    /// <summary>
    /// Returns a default CityGraphAsset to use when no explicit/owner graph is present.
    /// Editor: tries AssetDatabase first, then Resources.
    /// Runtime: uses Resources only.
    /// </summary>
    public static CityGraphAsset GetDefaultGraph()
    {
        if (_cachedDefault != null)
            return _cachedDefault;

        // 0) Prefer the graph referenced by the active game config (GameMainConfig is a Resources singleton).
        try
        {
            var fromGameMainConfig = GameMainConfig.Instance != null
                ? GameMainConfig.Instance.GameConfig != null
                    ? GameMainConfig.Instance.GameConfig.CityConfig != null
                        ? GameMainConfig.Instance.GameConfig.CityConfig.CityGraph
                        : null
                    : null
                : null;

            if (fromGameMainConfig != null)
            {
                _cachedDefault = fromGameMainConfig;
                return _cachedDefault;
            }
        }
        catch (Exception)
        {
            // GameMainConfig.Instance throws if it's not present in Resources; ignore and fall back.
        }

        // 1. Try Resources at runtime (and in editor as fallback).
        //    If you keep your graphs under Resources/CityGraphs, this can be:
        //    Resources.LoadAll<CityGraphAsset>("CityGraphs");
        var fromResources = Resources.LoadAll<CityGraphAsset>(string.Empty);
        if (fromResources != null && fromResources.Length > 0)
        {
            _cachedDefault = fromResources[0];
            return _cachedDefault;
        }

#if UNITY_EDITOR
        // 2. Editor-only: search all CityGraphAsset assets in the project.
        string[] guids = UnityEditor.AssetDatabase.FindAssets("t:CityGraphAsset");
        foreach (var guid in guids)
        {
            var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<CityGraphAsset>(path);
            if (asset != null)
            {
                _cachedDefault = asset;
                return _cachedDefault;
            }
        }
#endif

        return null;
    }
}