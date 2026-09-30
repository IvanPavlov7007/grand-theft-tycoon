#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.Splines;
using System.Linq;
using System.Collections.Generic;
using System.IO;

// ChatGPT generated code
public static class CityGraphBaker
{
    private const string LastFolderPrefKey = "CityGraphBaker.LastFolder";

    [MenuItem("City/Bake Graph From Selection")]
    public static void BakeSelected()
    {
        var root = Selection.activeGameObject;
        if (root == null)
        {
            Debug.Log("Select a root GameObject containing nodes and edges.");
            return;
        }

        var nodes = root.GetComponentsInChildren<RoadNodeAuthor>(true);
        var edges = root.GetComponentsInChildren<RoadEdgeAuthor>(true);
        var markers = root.GetComponentsInChildren<CityMarkerAuthor>(false);
        var areas = root.GetComponentsInChildren<CityAreaAuthor>(false);
        var trafficLights = root.GetComponentsInChildren<TrafficLightAuthor>(false);

        foreach (var n in nodes)
        {
            n.EnsureId();
            // Re-check uniqueness at baking time
            var dupes = nodes.Where(x => x != n && x.Id == n.Id).ToList();
            if (dupes.Count > 0)
            {
                // Regenerate this node's Id to avoid conflicts in the asset
                n.GetType().GetMethod("EnsureUniqueIdInScene", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.Invoke(n, null);
            }
        }

        foreach (var e in edges) e.EnsureId();

        foreach (var m in markers)
        {
            m.EnsureId();
            var dupes = markers.Where(x => x != m && x.Id == m.Id).ToList();
            if (dupes.Count > 0)
            {
                m.GetType().GetMethod("EnsureUniqueIdInScene", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.Invoke(m, null);
            }
        }

        foreach (var a in areas)
        {
            a.EnsureId();
            var dupes = areas.Where(x => x != a && x.Id == a.Id).ToList();
            if (dupes.Count > 0)
            {
                a.GetType().GetMethod("EnsureUniqueIdInScene", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.Invoke(a, null);
            }
        }

        foreach (var t in trafficLights)
        {
            t.EnsureId();
            var dupes = trafficLights.Where(x => x != t && x.Id == t.Id).ToList();
            if (dupes.Count > 0)
            {
                t.GetType().GetMethod("EnsureUniqueIdInScene", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.Invoke(t, null);
            }
        }

        // Load last-used folder (default to Assets)
        var lastFolder = EditorPrefs.GetString(LastFolderPrefKey, "Assets");
        if (string.IsNullOrEmpty(lastFolder) || !lastFolder.StartsWith("Assets"))
            lastFolder = "Assets";

        string path;
        // Prefer the overload with directory if available
        try
        {
            path = EditorUtility.SaveFilePanelInProject(
                "Save CityGraph",
                "CityGraph",
                "asset",
                "Choose asset location",
                lastFolder);
        }
        catch // fallback for older Unity versions without the directory overload
        {
            path = EditorUtility.SaveFilePanelInProject(
                "Save CityGraph",
                "CityGraph",
                "asset",
                "Choose asset location");
        }

        if (string.IsNullOrEmpty(path)) return;

        // Persist the chosen folder for next time
        var chosenFolder = Path.GetDirectoryName(path)?.Replace('\\', '/');
        if (!string.IsNullOrEmpty(chosenFolder) && chosenFolder.StartsWith("Assets"))
        {
            EditorPrefs.SetString(LastFolderPrefKey, chosenFolder);
        }

        var graph = AssetDatabase.LoadAssetAtPath<CityGraphAsset>(path);
        if (graph == null)
        {
            graph = ScriptableObject.CreateInstance<CityGraphAsset>();
            AssetDatabase.CreateAsset(graph, path);
        }

        graph.Nodes = nodes.Select(n => new CityGraphAsset.NodeData
        {
            Id = n.Id,
            Position = new Vector2(n.transform.position.x, n.transform.position.y)
        }).ToList();

        graph.Edges = edges.Select(e => new CityGraphAsset.EdgeData
        {
            Id = e.Id,
            FromNodeId = e.From ? e.From.Id : null,
            ToNodeId = e.To ? e.To.Id : null,
            Bidirectional = e.Bidirectional,
            Tags = e.Tags,
            // We no longer store scene references here for persistence hygiene
            EdgeAuthorId = e.Id,
            EdgeAuthorPath = GetHierarchyPath(e.gameObject),
            SplineIndex = 0
        }).ToList();

        // New: bake markers
        graph.Markers = markers.Select(m =>
        {
            var md = new CityGraphAsset.MarkerData
            {
                Id = m.Id,
                DisplayName = m.DisplayName,
                Tags = m.Tags,
                Radius = m.Radius,
                AuthorId = m.Id,
                AuthorPath = GetHierarchyPath(m.gameObject),
                Anchor = new CityGraphAsset.MarkerAnchorData()
            };

#pragma warning disable CS0618
            md.RegionId = m.RegionId;
#pragma warning restore CS0618

            // Always store the marker's world position for snapping fallback
            var wp = m.transform.position;
            md.Anchor.WorldPoint = new Vector2(wp.x, wp.y);

            switch (m.Kind)
            {
                case CityMarkerAuthor.AnchorKind.WorldPoint:
                    md.Anchor.Kind = CityGraphAsset.MarkerAnchorKind.WorldPoint;
                    break;

                case CityMarkerAuthor.AnchorKind.Node:
                    md.Anchor.Kind = CityGraphAsset.MarkerAnchorKind.Node;
                    md.Anchor.NodeId = m.Node ? m.Node.Id : null;
                    break;

                case CityMarkerAuthor.AnchorKind.Edge:
                    md.Anchor.Kind = CityGraphAsset.MarkerAnchorKind.Edge;
                    md.Anchor.EdgeId = m.Edge ? m.Edge.Id : TryFindNearestEdgeAuthorId(edges, m.transform.position);
                    md.Anchor.T = Mathf.Clamp01(m.T);
                    md.Anchor.Forward = m.Forward;
                    break;
            }

            return md;
        }).ToList();

        // New: bake areas
        graph.Areas = areas.Select(a =>
        {
            var poly = a.PolygonCollider;

            Vector2[] points = System.Array.Empty<Vector2>();
            if (poly != null && poly.pathCount > 0)
            {
                var offset = poly.offset;
                var splinePath = poly.GetPath(0);
                points = new Vector2[splinePath.Length];
                for (int i = 0; i < splinePath.Length; i++)
                {
                    var world = a.transform.TransformPoint(splinePath[i] + offset);
                    var rootLocal = root.transform.InverseTransformPoint(world);
                    points[i] = new Vector2(rootLocal.x, rootLocal.y);
                }
            }

            return new CityGraphAsset.AreaData
            {
                Id = a.Id,
                DisplayName = a.DisplayName,
                Tags = a.Tags,
                Polygon = points,
                AuthorId = a.Id,
                AuthorPath = GetHierarchyPath(a.gameObject)
            };
        }).ToList();

        // New: bake traffic lights
        graph.TrafficLights = trafficLights.Select(t =>
        {
            var data = new CityGraphAsset.TrafficLightData
            {
                Id = t.Id,
                NodeId = t.Node ? t.Node.Id : null,
                AuthorId = t.Id,
                AuthorPath = GetHierarchyPath(t.gameObject),
                PreparationTimeSeconds = t.PreparationTimeSeconds,
                InitialTimeOffsetSeconds = t.InitialTimeOffsetSeconds,
                EdgeSlots = new List<CityGraphAsset.TrafficLightEdgeSlotData>(),
                Program = new List<CityGraphAsset.TrafficLightProgramStepData>()
            };

            var slots = t.EdgeSlots;
            if (slots != null)
            {
                for (int i = 0; i < slots.Count; i++)
                {
                    var s = slots[i];
                    data.EdgeSlots.Add(new CityGraphAsset.TrafficLightEdgeSlotData
                    {
                        Key = s.Key,
                        EdgeId = s.Edge != null ? s.Edge.Id : null
                    });
                }
            }

            if (t.Program != null)
            {
                for (int i = 0; i < t.Program.Count; i++)
                {
                    var p = t.Program[i];
                    data.Program.Add(new CityGraphAsset.TrafficLightProgramStepData
                    {
                        DurationSeconds = p.DurationSeconds,
                        GoEdgeKeys = p.GoEdgeKeys != null ? p.GoEdgeKeys.ToArray() : System.Array.Empty<string>()
                    });
                }
            }

            return data;
        }).ToList();

        // Warn for multiple splines between same node pair
        var duplicates = new Dictionary<(string from, string to), int>();
        foreach (var ed in graph.Edges)
        {
            if (string.IsNullOrEmpty(ed.FromNodeId) || string.IsNullOrEmpty(ed.ToNodeId)) continue;
            var key = (ed.FromNodeId, ed.ToNodeId);
            if (!duplicates.ContainsKey(key)) duplicates[key] = 0;
            duplicates[key]++;
        }

        foreach (var kv in duplicates)
        {
            if (kv.Value > 1)
            {
                Debug.LogWarning($"Multiple edges detected between nodes '{kv.Key.from}' and '{kv.Key.to}' ({kv.Value}).");
            }
        }

        if (graph.Markers != null)
        {
            foreach (var marker in graph.Markers)
            {
                if (marker == null || marker.Anchor == null) continue;

                if (marker.Anchor.Kind == CityGraphAsset.MarkerAnchorKind.Edge && string.IsNullOrEmpty(marker.Anchor.EdgeId))
                {
                    Debug.LogWarning($"[CityGraphBaker] Marker '{marker.Id}' is edge-anchored but has no associated edge. Re-bake after assigning an edge or adjust the marker position.");
                }
            }
        }

        EditorUtility.SetDirty(graph);
        AssetDatabase.SaveAssets();
        Debug.Log($"Baked {graph.Nodes.Count} nodes, {graph.Edges.Count} edges, {graph.Markers.Count} markers, {graph.Areas.Count} areas, {graph.TrafficLights.Count} traffic lights to '{path}'.");
    }

    private static string TryFindNearestEdgeAuthorId(RoadEdgeAuthor[] edges, Vector3 worldPoint)
    {
        if (edges == null || edges.Length == 0)
        {
            return null;
        }

        RoadEdgeAuthor bestEdge = null;
        float bestD2 = float.PositiveInfinity;
        var worldPoint2 = new Vector2(worldPoint.x, worldPoint.y);

        for (int i = 0; i < edges.Length; i++)
        {
            var edge = edges[i];
            if (edge == null || edge.Container == null)
            {
                continue;
            }

            var spline = edge.Container.Splines.Count > 0 ? edge.Container.Splines[edge.SplineIndex] : null;
            if (spline == null)
            {
                continue;
            }

            const int divisions = 32;
            for (int j = 0; j <= divisions; j++)
            {
                float t = j / (float)divisions;
                var localPos = SplineUtility.EvaluatePosition(spline, t);
                var worldPos3 = edge.Container.transform.TransformPoint(localPos);
                float d2 = (new Vector2(worldPos3.x, worldPos3.y) - worldPoint2).sqrMagnitude;
                if (d2 < bestD2)
                {
                    bestD2 = d2;
                    bestEdge = edge;
                }
            }
        }

        return bestEdge != null ? bestEdge.Id : null;
    }

    private static string GetHierarchyPath(GameObject go)
    {
        var path = go.name;
        var t = go.transform;
        while (t.parent != null)
        {
            t = t.parent;
            path = $"{t.name}/{path}";
        }
        return path;
    }
}
#endif