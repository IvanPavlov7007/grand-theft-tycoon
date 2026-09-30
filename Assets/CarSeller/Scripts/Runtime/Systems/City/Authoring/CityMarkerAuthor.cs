using System;
using System.Linq;
using UnityEngine;

[ExecuteAlways]
public class CityMarkerAuthor : MonoBehaviour
{
    public enum AnchorKind
    {
        WorldPoint,
        Node,
        Edge
    }

    [SerializeField] private string _id;
    public string Id => _id;

    [Header("Identification")]
    public string DisplayName;
    public string[] Tags;
    [Obsolete("Legacy field. Use CityAreaAuthor polygons; marker AreaIds are computed based on containment.")]
    [HideInInspector] public string RegionId;
    public float Radius;

    [Header("Anchor")]
    public AnchorKind Kind = AnchorKind.Edge; // Keep default as Edge, since most markers will be on roads

    [Tooltip("When Kind=Node")]
    public RoadNodeAuthor Node;

    [Tooltip("When Kind=Edge")]
    public RoadEdgeAuthor Edge;
    [Range(0f, 1f)] public float T = 0.5f;
    public bool Forward = true;

    private void OnValidate()
    {
        EnsureId();
    }

    public void EnsureId()
    {
        if (string.IsNullOrEmpty(_id))
        {
            _id = Guid.NewGuid().ToString("N");
        }
    }

#if UNITY_EDITOR
    // Used by baker to fix dupes in a selection
    void EnsureUniqueIdInScene()
    {
        var all = FindObjectsOfType<CityMarkerAuthor>(true);
        if (all.Count(x => x != this && x._id == _id) > 0)
        {
            _id = Guid.NewGuid().ToString("N");
        }
    }
#endif
}