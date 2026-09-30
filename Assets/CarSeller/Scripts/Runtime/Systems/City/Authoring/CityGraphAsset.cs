using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CityGraph", menuName = "City/Graph")]
public class CityGraphAsset : ScriptableObject
{
    public GameObject PrefabRoot; // prefab root for authoring linkage
    public GameObject TrafficLightPrefab;

    [Serializable]
    public class NodeData
    {
        public string Id;
        public Vector2 Position;
    }

    [Serializable]
    public class EdgeData
    {
        public string Id;
        public string FromNodeId;
        public string ToNodeId;
        public bool Bidirectional = true;

        public string[] Tags;

        public string EdgeAuthorId;     // RoadEdgeAuthor.Id
        public string EdgeAuthorPath;   // optional: scene hierarchy path for robust lookup
        public int SplineIndex = 0;     // convention: one spline per edge
    }

    [Serializable]
    public enum MarkerAnchorKind
    {
        WorldPoint,
        Node,
        Edge
    }

    [Serializable]
    public class MarkerAnchorData
    {
        public MarkerAnchorKind Kind = MarkerAnchorKind.WorldPoint;

        public string NodeId;

        [Range(0f, 1f)] public float T;
        public bool Forward = true;
        public string EdgeId;

        public Vector2 WorldPoint;
    }

    [Serializable]
    public class MarkerData
    {
        public string Id;
        public string DisplayName;
        public string[] Tags;
        public string RegionId;
        public float Radius;

        public MarkerAnchorData Anchor = new MarkerAnchorData();

        public string AuthorId;
        public string AuthorPath;
    }

    [Serializable]
    public class AreaData
    {
        public string Id;
        public string DisplayName;
        public string[] Tags;

        public Vector2[] Polygon;

        public string AuthorId;
        public string AuthorPath;
    }

    [Serializable]
    public class TrafficLightEdgeSlotData
    {
        public string Key;     // local key: a,b,c,d...
        public string EdgeId;  // baked RoadEdgeAuthor.Id
    }

    [Serializable]
    public class TrafficLightProgramStepData
    {
        public float DurationSeconds = 5f;

        // Keys from TrafficLightEdgeSlotData.Key that should be "Go" in this phase.
        public string[] GoEdgeKeys;
    }

    [Serializable]
    public class TrafficLightData
    {
        public string Id;

        public string NodeId;

        public string AuthorId;
        public string AuthorPath;

        public float PreparationTimeSeconds = 0.75f;
        public float InitialTimeOffsetSeconds = 0f;

        public List<TrafficLightEdgeSlotData> EdgeSlots = new List<TrafficLightEdgeSlotData>();
        public List<TrafficLightProgramStepData> Program = new List<TrafficLightProgramStepData>();
    }

    public List<NodeData> Nodes = new List<NodeData>();
    public List<EdgeData> Edges = new List<EdgeData>();

    public List<MarkerData> Markers = new List<MarkerData>();
    public List<AreaData> Areas = new List<AreaData>();

    public List<TrafficLightData> TrafficLights = new List<TrafficLightData>();
}