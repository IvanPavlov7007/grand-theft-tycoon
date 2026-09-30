using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Splines;

public static class CityGraphLoader
{
    public static void LoadFromScene(City city, CityGraphAsset graph, Transform root)
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        var totalTimer = Stopwatch.StartNew();
        var phaseTimer = Stopwatch.StartNew();
        double initGraphMs;
        double areasMs;
        double edgesMs;
        double markerPrepMs;
        double markersMs;
        double adjacencyMs;
#endif

        city.InitializeFromGraph(graph);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        initGraphMs = phaseTimer.Elapsed.TotalMilliseconds;
        phaseTimer.Restart();
#endif

        // Areas are purely data-driven (polygons baked in PrefabRoot local space).
        // We transform them to world-space once at load time.
        int areaCount = graph.Areas != null ? graph.Areas.Count : 0;
        var areas = new List<City.CityPolygonArea>(areaCount);
        if (graph.Areas != null)
        {
            foreach (var a in graph.Areas)
            {
                Vector2[] poly = null;
                if (a.Polygon != null && a.Polygon.Length > 0)
                {
                    poly = new Vector2[a.Polygon.Length];
                    for (int i = 0; i < a.Polygon.Length; i++)
                    {
                        var w = root.TransformPoint(new Vector3(a.Polygon[i].x, a.Polygon[i].y, 0f));
                        poly[i] = new Vector2(w.x, w.y);
                    }
                }

                areas.Add(new City.CityPolygonArea
                {
                    Id = a.Id,
                    Name = a.DisplayName,
                    Tags = a.Tags,
                    Polygon = poly
                });
            }
        }

        city.InitializeAreas(areas);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        areasMs = phaseTimer.Elapsed.TotalMilliseconds;
        phaseTimer.Restart();
#endif

        var edgeAuthorComponents = root.GetComponentsInChildren<RoadEdgeAuthor>(true);
        var edgeAuthorsById = new Dictionary<string, RoadEdgeAuthor>(edgeAuthorComponents.Length);
        for (int i = 0; i < edgeAuthorComponents.Length; i++)
        {
            var author = edgeAuthorComponents[i];
            if (author == null || string.IsNullOrEmpty(author.Id)) continue;
            edgeAuthorsById[author.Id] = author;
        }

        var graphEdgeById = new Dictionary<string, CityGraphAsset.EdgeData>(graph.Edges.Count);
        for (int i = 0; i < graph.Edges.Count; i++)
        {
            var edgeData = graph.Edges[i];
            if (edgeData == null || string.IsNullOrEmpty(edgeData.Id)) continue;
            graphEdgeById[edgeData.Id] = edgeData;
        }

        foreach (var edge in city.Edges)
        {
            if (!graphEdgeById.TryGetValue(edge.Id, out var data)) continue;

            if (edgeAuthorsById.TryGetValue(data.EdgeAuthorId, out var author))
            {
                edge.Container = author.Container;
                edge.SplineIndex = 0;

                var spline = edge.GetSpline();
                if (spline != null)
                {
                    // Use world-space matrix for consistent length with scaled containers
                    var m = edge.Container != null
                        ? edge.Container.transform.localToWorldMatrix
                        : Matrix4x4.identity;

                    edge.Length = SplineUtility.CalculateLength(spline, m);
                }
                else
                {
                    edge.Length = 0f;
                }
            }
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        edgesMs = phaseTimer.Elapsed.TotalMilliseconds;
        phaseTimer.Restart();
#endif

        // Build lookup
        var nodeById = new Dictionary<string, RoadNode>(city.Nodes.Count);
        for (int i = 0; i < city.Nodes.Count; i++)
        {
            var node = city.Nodes[i];
            if (node == null || string.IsNullOrEmpty(node.Id)) continue;
            nodeById[node.Id] = node;
        }

        var edgeById = new Dictionary<string, RoadEdge>(city.Edges.Count);
        for (int i = 0; i < city.Edges.Count; i++)
        {
            var edge = city.Edges[i];
            if (edge == null || string.IsNullOrEmpty(edge.Id)) continue;
            edgeById[edge.Id] = edge;
        }

        // Optional: author lookup for world positions when missing
        Dictionary<string, CityMarkerAuthor> markerAuthorsById = null;
        if (graph.Markers != null && graph.Markers.Count > 0)
        {
            var markerAuthorComponents = root.GetComponentsInChildren<CityMarkerAuthor>(true);
            markerAuthorsById = new Dictionary<string, CityMarkerAuthor>(markerAuthorComponents.Length);
            for (int i = 0; i < markerAuthorComponents.Length; i++)
            {
                var author = markerAuthorComponents[i];
                if (author == null || string.IsNullOrEmpty(author.Id)) continue;
                markerAuthorsById[author.Id] = author;
            }
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        markerPrepMs = phaseTimer.Elapsed.TotalMilliseconds;
        phaseTimer.Restart();
#endif

        // Finalize markers (after edges resolved so Edge.GetSpline is valid)
        int markerCount = graph.Markers != null ? graph.Markers.Count : 0;
        var markers = new List<City.CityMarker>(markerCount);
        var areaMatches = new List<string>(8);
        if (graph.Markers != null)
        {
            foreach (var m in graph.Markers)
            {
                var cm = new City.CityMarker
                {
                    Id = m.Id,
                    Name = m.DisplayName,
                    Tags = m.Tags,
                    RegionId = m.RegionId,
                    Radius = m.Radius
                };

                // Determine a reference position for snapping if needed
                Vector2 refWorld = GetMarkerReferenceWorldPoint(m, markerAuthorsById);

                switch (m.Anchor.Kind)
                {
                    case CityGraphAsset.MarkerAnchorKind.WorldPoint:
                        cm.WorldPoint = m.Anchor.WorldPoint;
                        break;

                    case CityGraphAsset.MarkerAnchorKind.Node:
                        if (!string.IsNullOrEmpty(m.Anchor.NodeId) && nodeById.TryGetValue(m.Anchor.NodeId, out var node))
                        {
                            cm.PositionOnGraph = CityPosition.At(node);
                        }
                        else
                        {
                            // Snap to nearest node using reference world point
                            var nearestNode = FindNearestNode(city, refWorld);
                            if (nearestNode != null)
                            {
                                cm.PositionOnGraph = CityPosition.At(nearestNode);
                            }
                            else
                            {
                                // No nodes: keep as world point.
                                cm.WorldPoint = refWorld;
                            }
                        }
                        break;

                    case CityGraphAsset.MarkerAnchorKind.Edge:
                        if (!string.IsNullOrEmpty(m.Anchor.EdgeId) && edgeById.TryGetValue(m.Anchor.EdgeId, out var edgeFromId))
                        {
                            var t = Mathf.Clamp01(m.Anchor.T);
                            bool forward = m.Anchor.Forward || !edgeFromId.Bidirectional;
                            cm.PositionOnGraph = CityPosition.On(edgeFromId, t, forward);
                        }
                        else
                        {
                            // Edge association should be baked. Fall back to world position only for legacy assets.
                            cm.WorldPoint = refWorld;
                        }
                        break;
                }

                if (areas.Count > 0)
                {
                    var wp = cm.WorldPosition;
                    areaMatches.Clear();
                    for (int i = 0; i < areas.Count; i++)
                    {
                        var area = areas[i];
                        if (!area.Contains(wp) || string.IsNullOrEmpty(area.Id)) continue;
                        areaMatches.Add(area.Id);
                    }

                    cm.AreaIds = areaMatches.Count > 0 ? areaMatches.ToArray() : System.Array.Empty<string>();
                }
                else
                {
                    cm.AreaIds = System.Array.Empty<string>();
                }

                markers.Add(cm);
            }
        }

        city.InitializeMarkers(markers);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        markersMs = phaseTimer.Elapsed.TotalMilliseconds;
        phaseTimer.Restart();
#endif

        RoadGraphMaintenance.RebuildAdjacency(city.Nodes, city.Edges);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        adjacencyMs = phaseTimer.Elapsed.TotalMilliseconds;
        totalTimer.Stop();

        string graphName = graph != null ? graph.name : "<null-graph>";
        string rootName = root != null ? root.name : "<null-root>";
        UnityEngine.Debug.Log(
            $"[CityGraphLoader] '{graphName}' on '{rootName}' loaded in {totalTimer.Elapsed.TotalMilliseconds:0.00} ms " +
            $"(init={initGraphMs:0.00}, areas={areasMs:0.00}, edges={edgesMs:0.00}, markerPrep={markerPrepMs:0.00}, markers={markersMs:0.00}, adjacency={adjacencyMs:0.00}) " +
            $"[nodes={city.Nodes.Count}, edges={city.Edges.Count}, markers={markers.Count}, areas={areas.Count}]"
        );
#endif
    }

    private static Vector2 GetMarkerReferenceWorldPoint(CityGraphAsset.MarkerData m, Dictionary<string, CityMarkerAuthor> authorsById)
    {
        // Prefer baked world point
        var wp = m.Anchor.WorldPoint;
        // If an author exists in the instantiated root, use its live transform (handles scaled roots, etc.)
        if (authorsById != null && !string.IsNullOrEmpty(m.AuthorId) && authorsById.TryGetValue(m.AuthorId, out var author) && author != null)
        {
            var p = author.transform.position;
            return new Vector2(p.x, p.y);
        }
        return wp;
    }

    private static RoadNode FindNearestNode(City city, Vector2 worldPoint)
    {
        RoadNode best = null;
        float bestD2 = float.PositiveInfinity;
        foreach (var n in city.Nodes)
        {
            float d2 = (n.Position - worldPoint).sqrMagnitude;
            if (d2 < bestD2)
            {
                bestD2 = d2;
                best = n;
            }
        }
        return best;
    }

}