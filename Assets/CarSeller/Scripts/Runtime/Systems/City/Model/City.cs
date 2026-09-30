using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Splines;
using static City;

public class City : ILocationsHolder, IDisposable
{
    public CityConfig Config;
    internal Dictionary<ILocatable, CityEntity> Entities { get; private set; } = new();
    public INodesNetwork nodesNet { get; private set; } // Legacy grid (optional)

    public IReadOnlyList<RoadNode> Nodes => _nodes;
    public IReadOnlyList<RoadEdge> Edges => _edges;

    private readonly List<RoadNode> _nodes = new();
    private readonly List<RoadEdge> _edges = new();

    private CityEntityLifetimeService lifetimeService = new CityEntityLifetimeService();
    public static CityEntityLifetimeService EntityLifetimeService => G.City.lifetimeService;
    public AspectsSystem AspectsSystem { get; private set; }
    public CityEntityAspectsService AspectsService => AspectsSystem.AspectsService;

    public SpatialGridManager SpatialGridManager;

    // MARKERS RUNTIME
    public sealed class CityMarker
    {
        public string Id;
        public string Name;
        public string[] Tags;
        public string RegionId;
        public string[] AreaIds;
        public float Radius;

        public CityPosition? PositionOnGraph; // Node or Edge@T anchor
        public Vector2? WorldPoint;           // WorldPoint anchor fallback

        public Vector2 WorldPosition => PositionOnGraph.HasValue
            ? PositionOnGraph.Value.WorldPosition
            : (WorldPoint ?? Vector2.zero);

        public bool HasTag(string tag)
        {
            if (Tags == null || tag == null) return false;
            for (int i = 0; i < Tags.Length; i++)
            {
                if (string.Equals(Tags[i], tag, StringComparison.OrdinalIgnoreCase)) return true;
            }
            return false;
        }
    }

    private readonly Dictionary<string, CityMarker> _markersById = new();
    public IReadOnlyDictionary<string, CityMarker> MarkersById => _markersById;

    // AREAS RUNTIME
    public sealed class CityPolygonArea
    {
        public string Id;
        public string Name;
        public string[] Tags;

        // World-space polygon points (XY)
        public Vector2[] Polygon;

        public bool HasTag(string tag)
        {
            if (Tags == null || tag == null) return false;
            for (int i = 0; i < Tags.Length; i++)
            {
                if (string.Equals(Tags[i], tag, StringComparison.OrdinalIgnoreCase)) return true;
            }
            return false;
        }

        public bool Contains(CityPosition position)
        {
            return Contains(position.WorldPosition);
        }

        public bool Contains(Vector2 point)
        {
            if (Polygon == null || Polygon.Length < 3)
                return false;

            bool inside = false;
            for (int i = 0, j = Polygon.Length - 1; i < Polygon.Length; j = i++)
            {
                var pi = Polygon[i];
                var pj = Polygon[j];

                bool intersect = ((pi.y > point.y) != (pj.y > point.y))
                                 && (point.x < (pj.x - pi.x) * (point.y - pi.y) / (pj.y - pi.y + float.Epsilon) + pi.x);
                if (intersect)
                    inside = !inside;
            }
            return inside;
        }

        public float ApproxArea()
        {
            if (Polygon == null || Polygon.Length < 3)
                return 0f;

            float sum = 0f;
            for (int i = 0, j = Polygon.Length - 1; i < Polygon.Length; j = i++)
            {
                sum += (Polygon[j].x * Polygon[i].y) - (Polygon[i].x * Polygon[j].y);
            }
            return Mathf.Abs(sum) * 0.5f;
        }
    }

    private readonly Dictionary<string, CityPolygonArea> _areasById = new();
    public IReadOnlyDictionary<string, CityPolygonArea> AreasById => _areasById;

    internal void InitializeMarkers(IEnumerable<CityMarker> markers)
    {
        _markersById.Clear();
        foreach (var m in markers)
        {
            if (!string.IsNullOrEmpty(m.Id))
            {
                _markersById[m.Id] = m;
            }
        }
    }

    internal void InitializeAreas(IEnumerable<CityPolygonArea> areas)
    {
        _areasById.Clear();
        if (areas == null) return;
        foreach (var a in areas)
        {
            if (!string.IsNullOrEmpty(a.Id))
                _areasById[a.Id] = a;
        }
    }

    public bool TryGetArea(string id, out CityPolygonArea area)
    {
        if (string.IsNullOrEmpty(id))
        {
            area = null;
            return false;
        }
        return _areasById.TryGetValue(id, out area);
    }

    public bool TryGetMarker(string id, out CityMarker marker)
    {
        if (string.IsNullOrEmpty(id))
        {
            marker = null;
            return false;
        }
        return _markersById.TryGetValue(id, out marker);
    }

    public IEnumerable<CityPolygonArea> QueryAreasAt(CityPosition position, string tag = null, Predicate<CityPolygonArea> predicate = null, bool recomputePosition = false)
    {
        foreach (var area in _areasById.Values)
        {
            if (area == null)
                continue;

            if (!string.IsNullOrEmpty(tag) && !area.HasTag(tag))
                continue;

            if (predicate != null && !predicate(area))
                continue;

            if (area.Contains(position))
                yield return area;
        }
    }

    public bool TryGetAreaAt(CityPosition position, out CityPolygonArea area, string tag = null, Predicate<CityPolygonArea> predicate = null, bool recomputePosition = false, bool preferSmallest = true)
    {
        area = null;

        float bestMetric = float.PositiveInfinity;

        foreach (var a in _areasById.Values)
        {
            if (a == null)
                continue;

            if (!string.IsNullOrEmpty(tag) && !a.HasTag(tag))
                continue;

            if (predicate != null && !predicate(a))
                continue;

            if (!a.Contains(position))
                continue;

            if (!preferSmallest)
            {
                area = a;
                return true;
            }

            float metric = a.ApproxArea();
            if (area == null || metric < bestMetric)
            {
                bestMetric = metric;
                area = a;
            }
        }

        return area != null;
    }

    public IEnumerable<CityMarker> QueryMarkers(string tag = null, string areaId = null, Predicate<CityMarker> predicate = null)
    {
        return _markersById.Values.QueryMarkers(tag, areaId, predicate);
    }

    public CityMarker GetRandomMarker(string tag = null, string areaId = null, Predicate<CityMarker> predicate = null)
    {
        var list = QueryMarkers(tag, areaId, predicate).ToList();
        if (list.Count == 0) return null;
        return list[UnityEngine.Random.Range(0, list.Count)];
    }

    private readonly Dictionary<string, TrafficLightRuntimeController> _trafficLightsById = new();
    public IReadOnlyDictionary<string, TrafficLightRuntimeController> TrafficLightsById => _trafficLightsById;

    private readonly Dictionary<string, List<TrafficLightRuntimeController>> _trafficLightsByNodeId = new();

    internal void InitializeTrafficLights(IEnumerable<TrafficLightRuntimeController> trafficLights)
    {
        _trafficLightsById.Clear();
        _trafficLightsByNodeId.Clear();

        if (trafficLights == null)
            return;

        foreach (var tl in trafficLights)
        {
            if (tl == null)
                continue;

            if (!string.IsNullOrEmpty(tl.Id))
            {
                _trafficLightsById[tl.Id] = tl;
            }

            if (!string.IsNullOrEmpty(tl.NodeId))
            {
                if (!_trafficLightsByNodeId.TryGetValue(tl.NodeId, out var list))
                {
                    list = new List<TrafficLightRuntimeController>();
                    _trafficLightsByNodeId.Add(tl.NodeId, list);
                }
                list.Add(tl);
            }
        }
    }

    public bool TryGetTrafficLight(string id, out TrafficLightRuntimeController trafficLight)
    {
        if (string.IsNullOrEmpty(id))
        {
            trafficLight = null;
            return false;
        }

        return _trafficLightsById.TryGetValue(id, out trafficLight);
    }

    public bool TryGetTrafficLightAtNode(RoadNode node, out TrafficLightRuntimeController trafficLight)
    {
        trafficLight = null;

        if (node == null || string.IsNullOrEmpty(node.Id))
            return false;

        if (_trafficLightsByNodeId.TryGetValue(node.Id, out var list) && list != null && list.Count > 0)
        {
            trafficLight = list[0]; // current assumption: one traffic light per node
            return trafficLight != null;
        }

        return false;
    }

    public bool TryGetTrafficLightState(RoadNode node, RoadEdge edge, out TrafficLightState state)
    {
        state = default;

        if (!TryGetTrafficLightAtNode(node, out var tl) || tl == null)
            return false;

        // If node has a traffic light, treat unknown edges as "not allowed"
        // (so we don't accidentally allow through a missing config).
        return tl.TryGetStateForEdge(edge, out state);
    }

    public City(CityConfig cityConfig, Transform graphRoot, AspectsSystem aspectsSystem)
    {
        Config = cityConfig;
        // IMPORTANT: pass a valid root that contains your authored edges/nodes when available.
        // If null, CityGraphLoader must scan the active scene root(s).
        CityGraphLoader.LoadFromScene(this, cityConfig.CityGraph, graphRoot);
        this.AspectsSystem = aspectsSystem;
        SpatialGridManager = SpatialGridManager.Create(cityConfig.SpatialGridConfig, this);
    }

    public void InitializeFromGraph(CityGraphAsset graphAsset)
    {
        Debug.Assert(graphAsset != null, "CityGraphAsset must not be null.");

        _nodes.Clear();
        _edges.Clear();

        var nodeMap = new Dictionary<string, RoadNode>();
        foreach (var n in graphAsset.Nodes)
        {
            Debug.Assert(!string.IsNullOrEmpty(n.Id), "NodeData.Id must be set.");
            var rn = new RoadNode { Id = n.Id, Position = n.Position };
            nodeMap[n.Id] = rn;
            _nodes.Add(rn);
        }

        foreach (var e in graphAsset.Edges)
        {
            Debug.Assert(!string.IsNullOrEmpty(e.Id), "EdgeData.Id must not be null.");
            Debug.Assert(nodeMap.ContainsKey(e.FromNodeId), $"Edge.FromNodeId '{e.FromNodeId}' not found.");
            Debug.Assert(nodeMap.ContainsKey(e.ToNodeId), $"Edge.ToNodeId '{e.ToNodeId}' not found.");

            var edge = new RoadEdge
            {
                Id = e.Id,
                From = nodeMap[e.FromNodeId],
                To = nodeMap[e.ToNodeId],
                Tags = e.Tags,
                // DO NOT assign scene references here. Let CityGraphLoader resolve Container/Spline later.
                Container = null,
                SplineIndex = Mathf.Max(0, e.SplineIndex),
                Bidirectional = e.Bidirectional,
                Length = 0f
            };

            _edges.Add(edge);
            edge.From.Outgoing.Add(edge);
            edge.To.Incoming.Add(edge);

            if (edge.Bidirectional)
            {
                edge.To.Outgoing.Add(edge);
                edge.From.Incoming.Add(edge);
            }
        }

        // Markers are finalized after edges' containers are resolved in CityGraphLoader.
        InitializeMarkers(Array.Empty<CityMarker>());
    }
    public ILocation[] GetLocations() => Entities.Select(x => x.Value).ToArray();

    public bool TryGetEntity(ILocatable locatable, out CityEntity entity)
    {
        return Entities.TryGetValue(locatable, out entity);
    }

    public IReadOnlyDictionary<ILocatable, CityEntity> GetEntities() => Entities;

    internal CityPosition GetClosestPosition(Vector2 worldPosition)
    {
        // Prefer closest point on any edge (spline). Fallback to closest node.
        RoadEdge bestEdge = null;
        float bestEdgeT = 0f;
        float bestEdgeDist2 = float.PositiveInfinity;

        foreach (var edge in _edges)
        {
            if (!edge.TryGetClosestPoint(worldPosition, out float edgeT, out float edgeDist2))
            {
                continue;
            }

            if (edgeDist2 < bestEdgeDist2)
            {
                bestEdgeDist2 = edgeDist2;
                bestEdge = edge;
                bestEdgeT = edgeT;
            }
        }

        if (bestEdge != null)
        {
            // Choose forward=true by default; if you need direction-aware selection, compute tangent dot with a desired direction.
            return CityPosition.On(bestEdge, bestEdgeT, forward: true);
        }

        // No edges resolved; pick closest node
        RoadNode bestNode = null;
        float bestNodeDist2 = float.PositiveInfinity;
        foreach (var node in _nodes)
        {
            float d2 = (node.Position - worldPosition).sqrMagnitude;
            if (d2 < bestNodeDist2)
            {
                bestNodeDist2 = d2;
                bestNode = node;
            }
        }

        return bestNode != null ? CityPosition.At(bestNode) : default;
    }

    internal CityPosition GetRandomPosition()
    {
        if (_edges.Count > 0)
        {
            var edge = _edges[UnityEngine.Random.Range(0, _edges.Count)];
            float t = UnityEngine.Random.value; // [0,1]
            bool forward = edge.Bidirectional ? (UnityEngine.Random.value < 0.5f) : true;
            return CityPosition.On(edge, t, forward);
        }

        // Fallback: random node if there are no edges
        if (_nodes.Count > 0)
        {
            var node = _nodes[UnityEngine.Random.Range(0, _nodes.Count)];
            return CityPosition.At(node);
        }

        // Nothing available
        return default;
    }

    public void Dispose()
    {
        if(SpatialGridManager != null)
            GameObject.Destroy(SpatialGridManager.gameObject);
        AspectsSystem?.Dispose();

    }
}

public readonly struct CityPosition
{
    public RoadNode Node { get; }
    public RoadEdge Edge { get; }
    /// <summary>
    /// A percentage [0..1] along the edge if Edge is not null.
    /// From the 'From' node (0) to the 'To' node (1) if Forward is true,
    /// </summary>
    public float Percentage { get; }
    public bool Forward { get; }
    /// <summary>
    /// Important: doesn't get updated automatically if the underlying node/edge moves
    /// </summary>
    public Vector2 WorldPosition { get; }

    CityPosition(RoadNode atNode)
    {
        Debug.Assert(atNode != null, "CityPosition.At requires a non-null node.");
        Node = atNode;
        Edge = null;
        Percentage = 0f;
        Forward = true;
        WorldPosition = atNode.Position;
    }

    CityPosition(RoadEdge edge, float t, bool forward = true)
    {
        Debug.Assert(edge != null, "CityPosition.On requires a non-null edge.");
        var spline = edge?.GetSpline();
        Debug.Assert(spline != null, "CityPosition.On requires an edge with a valid SplineContainer/Spline.");
        Debug.Assert(t >= 0f && t <= 1f, "CityPosition.On percentage t must be in [0,1].");
        if (!forward)
        {
            Debug.Assert(edge.Bidirectional, "Reverse traversal requested but edge is not bidirectional.");
        }

        Node = null;
        Edge = edge;
        Percentage = Mathf.Clamp01(t);
        Forward = forward;
        WorldPosition = SpecificWorldPositionOnEdge(Edge, Percentage, Forward);
    }

    private static Vector2 SpecificWorldPositionOnEdge(RoadEdge edge, float t, bool forward)
    {
        Debug.Assert(edge != null, "SpecificWorldPositionOnEdge can only be used for edge positions.");
        Debug.Assert(t >= 0f && t <= 1f, "SpecificWorldPositionOnEdge t must be in [0,1].");
        var spline = edge.GetSpline();
        Debug.Assert(spline != null, "SpecificWorldPositionOnEdge: Edge spline is missing.");
        var localPos = SplineUtility.EvaluatePosition(spline, forward ? t : 1f - t);
        var worldPos = edge.Container != null
            ? edge.Container.transform.TransformPoint((Vector3)localPos)
            : (Vector3)localPos;
        return new Vector2(worldPos.x, worldPos.y);
    }

    public Vector2 SpecificWorldPositionOnEdge(float t)
    {
        return SpecificWorldPositionOnEdge(Edge, t, Forward);
    }

    public CityPosition WithPercentage(float t)
    {
        Debug.Assert(Edge != null, "WithPercentage can only be used for edge positions.");
        Debug.Assert(t >= 0f && t <= 1f, "WithPercentage t must be in [0,1].");
        return new CityPosition(Edge, t, Forward);
    }

    /// <summary>
    /// Finds the closest point on an edge to <paramref name="worldTarget"/> and returns it as a <see cref="CityPosition"/>.
    /// Uses <see cref="RoadEdge.TryGetClosestPoint"/> (coarse sampling + refinement).
    /// </summary>
    public static CityPosition GetClosestOnEdge(RoadEdge edge, Vector2 worldTarget, int coarseDivisions = 64, int refineIterations = 8)
    {
        edge.TryGetClosestPoint(worldTarget, out float t, out _, coarseDivisions, refineIterations);

        return On(edge, t, forward: true);
    }

    public bool FlowsIntoAnotherOnTheSameEdge(CityPosition other)
    {
        Debug.Assert(this.Edge != null);
        Debug.Assert(Edge == other.Edge);
        float t1 = this.Percentage;
        float t2 = this.Forward == other.Forward ? other.Percentage : 1f - other.Percentage;

        return t2 > t1;
    }

    public CityPosition TowardsAnotherForDistance(CityPosition other, float distance)
    {
        Debug.Assert(this.Edge != null);
        Debug.Assert(Edge == other.Edge);

        float deltaT = distance / Edge.Length;
        float t1 = this.Percentage;
        float t2 = other.Percentage;

        float newT;

        if (Forward == other.Forward)
        {
            if (t1 > t2)
            {
                newT = t1 - deltaT;
            }
            else
            {
                newT = t1 + deltaT;
            }
        }
        else
        {
            if (t1 > (1f - t2))
            {
                newT = t1 - deltaT;
            }
            else
            {
                newT = t1 + deltaT;
            }
        }
        newT = Mathf.Clamp01(newT);
        return On(Edge, newT, Forward);
    }

    public float DistanceToAnotherOnSameEdge(CityPosition other)
    {
        Debug.Assert(this.Edge != null && other.Edge != null, "DistanceTo currently only supports positions on edges.");
        Debug.Assert(this.Edge == other.Edge, "DistanceTo currently only supports positions on the same edge.");

        if (other.Forward != this.Forward)
        {
            other = other.Reversed();
        }
        float deltaT = Mathf.Abs(this.Percentage - other.Percentage);
        return deltaT * Edge.Length;
    }

    public CityPosition Reversed()
    {
        return On(Edge, 1f - Percentage, !Forward);
    }

    public Vector2 GetCurrentTangent()
    {
        Debug.Assert(Edge != null, "GetCurrentTangent can only be used for edge positions.");
        if (Forward)
        {
            return Edge.GetTangentFromNode(Edge.From, Percentage, out _);
        }
        else
        {
            return Edge.GetTangentFromNode(Edge.To, Percentage, out _);
        }
    }

    public static CityPosition At(RoadNode node) => new CityPosition(node);
    public static CityPosition On(RoadEdge edge, float t, bool forward = true) => new CityPosition(edge, t, forward);

    public CityPosition GetConnectionPositionFromAnotherEdge(RoadEdge AnotherEdge)
    {
        Debug.Assert(this.Edge != null, "GetConnectionPositionFromAnotherEdge can only be used for edge positions.");

        if (AnotherEdge.From == Edge.From)
        {
            return On(Edge, 0f, forward: true);
        }
        else if (AnotherEdge.To == Edge.From)
        {
            return On(Edge, 0f, forward: true);
        }
        else if (AnotherEdge.From == Edge.To)
        {
            return On(Edge, 0f, forward: false);
        }
        else if (AnotherEdge.To == Edge.To)
        {
            return On(Edge, 0f, forward: false);
        }
        else
        {
            throw new InvalidOperationException("Edges are not connected.");
        }
    }

    public CityPosition GetConnectionPositionTowardsEdge(RoadEdge AnotherEdge)
    {
        Debug.Assert(this.Edge != null, "GetConnectionPositionTowardsEdge can only be used for edge positions.");

        if (AnotherEdge.From == Edge.To)
        {
            return On(Edge, 1f, forward: true);
        }
        else if (AnotherEdge.To == Edge.To)
        {
            return On(Edge, 1f, forward: true);
        }
        else if (AnotherEdge.From == Edge.From)
        {
            return On(Edge, 1f, forward: false);
        }
        else if (AnotherEdge.To == Edge.From)
        {
            return On(Edge, 1f, forward: false);
        }
        throw new InvalidOperationException("Edges are not connected.");
    }
}

public static class CityExtensions
{
    public static IEnumerable<CityMarker> QueryMarkers(this IEnumerable<CityMarker> collection, string tag = null, string areaId = null, Predicate<CityMarker> predicate = null)
    {
        IEnumerable<CityMarker> q = collection;
        if (!string.IsNullOrEmpty(tag)) q = q.Where(m => m.HasTag(tag));
        if (!string.IsNullOrEmpty(areaId))
        {
            q = q.Where(m => m.AreaIds != null && m.AreaIds.Any(a => string.Equals(a, areaId, StringComparison.OrdinalIgnoreCase)));
        }
        if (predicate != null) q = q.Where(m => predicate(m));
        return q;
    }

    public static CityArea[] GetAreas(this City.CityMarker marker)
    {
        if (marker == null || marker.AreaIds == null || marker.AreaIds.Length == 0)
            return Array.Empty<CityArea>();

        CityArea[] areas = new CityArea[marker.AreaIds.Length];
        for (int i = 0; i < marker.AreaIds.Length; i++)
        {
            var id = marker.AreaIds[i];
            if (string.IsNullOrEmpty(id))
                continue;

            if (G.Areas != null && G.Areas.TryGetValue(id, out var cityArea))
            {
                areas[i] = cityArea;
            }
        }
        return areas;
    }

    // This is the “single method” to codify “use the first one”.

    public static bool TryGetPrimaryArea(this City.CityMarker marker, out CityArea area)
    {
        area = null;

        if (marker == null || marker.AreaIds == null || marker.AreaIds.Length == 0)
            return false;

        if (G.Areas == null)
            return false;

        for (int i = 0; i < marker.AreaIds.Length; i++)
        {
            var id = marker.AreaIds[i];
            if (string.IsNullOrEmpty(id))
                continue;

            if (G.Areas.TryGetValue(id, out area) && area != null)
                return true;
        }

        area = null;
        return false;
    }

    public static CityArea GetPrimaryAreaOrNull(this City.CityMarker marker)
    {
        return marker.TryGetPrimaryArea(out var area) ? area : null;
    }

    public static bool TryGetCityAreaAt(this City city, CityPosition position, out CityArea area, string tag = null, Predicate<City.CityPolygonArea> predicate = null, bool preferSmallest = true)
    {
        area = null;

        if (city == null)
            return false;

        if (!city.TryGetAreaAt(position, out var polygonArea, tag, predicate, preferSmallest: preferSmallest))
            return false;

        area = polygonArea.GetArea();
        return area != null;
    }

    public static CityArea GetArea(this City.CityPolygonArea area)
    {
        if (G.Areas.TryGetValue(area.Id, out var cityArea))
        {
            return cityArea;
        }
        return null;
    }
}