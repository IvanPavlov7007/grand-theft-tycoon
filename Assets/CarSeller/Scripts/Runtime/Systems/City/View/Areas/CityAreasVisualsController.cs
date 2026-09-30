using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class CityAreasVisualsController : MonoBehaviour
{
    [Serializable]
    public sealed class Settings
    {
        [Header("Behavior")]
        public bool StartHidden = true;

        [Header("Rendering")]
        public bool DrawFill = true;

        [Tooltip("Optional. If null, a default material will be created at runtime.")]
        public Material OutlineMaterial;

        [Tooltip("Optional. If null, a default material will be created at runtime.")]
        public Material FillMaterial;

        public string SortingLayerName = "Default";
        public int FillSortingOrder = -60;
        public int OutlineSortingOrder = -50;

        [Header("Appearance")]
        public float NormalOutlineWidth = 0.05f;
        public float HighlightOutlineWidth = 0.10f;

        public Color NormalOutlineColor = new Color(0.2f, 0.9f, 1f, 0.65f);
        public Color HighlightOutlineColor = new Color(1f, 0.95f, 0.2f, 1f);

        public Color NormalFillColor = new Color(0.2f, 0.9f, 1f, 0.10f);
        public Color HighlightFillColor = new Color(1f, 0.95f, 0.2f, 0.18f);

        [Header("Shader")]
        public string FillColorProperty = "_Color";

        [Header("Depth")]
        public float ZOffset = 0f;
    }

    [SerializeField] private Settings _settings = new Settings();

    private readonly Dictionary<string, CityAreaVisuals> _visualsByAreaId = new Dictionary<string, CityAreaVisuals>();
    private City _city;

    public static CityAreasVisualsController Ensure(City city)
    {
        var existing = FindObjectOfType<CityAreasVisualsController>(true);
        if (existing != null)
        {
            existing.Initialize(city);
            return existing;
        }

        var go = new GameObject("City Areas Visuals");
        var created = go.AddComponent<CityAreasVisualsController>();
        created.Initialize(city);
        return created;
    }

    public void Initialize(City city)
    {
        if (city == null)
        {
            Debug.LogWarning("CityAreasVisualsController.Initialize: city is null.");
            return;
        }

        _city = city;

        EnsureMaterials();
        Rebuild();
    }

    public bool TryGet(string areaId, out CityAreaVisuals visuals)
    {
        visuals = null;

        if (string.IsNullOrEmpty(areaId))
            return false;

        return _visualsByAreaId.TryGetValue(areaId, out visuals) && visuals != null;
    }

    public void SetHidden(string areaId, bool hidden)
    {
        if (TryGet(areaId, out var v))
            v.SetHidden(hidden);
    }

    public void SetHighlighted(string areaId, bool highlighted)
    {
        if (TryGet(areaId, out var v))
            v.SetHighlighted(highlighted);
    }

    public void SetAllHidden(bool hidden)
    {
        foreach (var v in _visualsByAreaId.Values)
        {
            if (v != null)
                v.SetHidden(hidden);
        }
    }

    public void ClearHighlights()
    {
        foreach (var v in _visualsByAreaId.Values)
        {
            if (v != null)
                v.SetHighlighted(false);
        }
    }

    public void HighlightOnly(string areaId)
    {
        foreach (var kv in _visualsByAreaId)
        {
            if (kv.Value == null)
                continue;

            bool isTarget = string.Equals(kv.Key, areaId, StringComparison.Ordinal);
            kv.Value.SetHidden(!isTarget);
            kv.Value.SetHighlighted(isTarget);
        }
    }

    private void Rebuild()
    {
        // clear old
        foreach (var v in _visualsByAreaId.Values)
        {
            if (v != null)
                Destroy(v.gameObject);
        }
        _visualsByAreaId.Clear();

        if (_city == null || _city.AreasById == null)
            return;

        foreach (var kv in _city.AreasById)
        {
            var area = kv.Value;
            if (area == null || string.IsNullOrEmpty(area.Id))
                continue;

            var go = new GameObject($"Area [{area.Id}] Visuals");
            go.transform.SetParent(transform, false);

            var visuals = go.AddComponent<CityAreaVisuals>();
            visuals.Initialize(area.Id, area.Polygon, _settings);

            _visualsByAreaId[area.Id] = visuals;
        }
    }

    private void EnsureMaterials()
    {
        if (_settings.OutlineMaterial == null)
        {
            var shader = Shader.Find("Sprites/Default");
            if (shader != null)
                _settings.OutlineMaterial = new Material(shader) { name = "Generated_AreaOutline" };
        }

        if (_settings.FillMaterial == null)
        {
            var shader = Shader.Find("Sprites/Default");
            if (shader != null)
                _settings.FillMaterial = new Material(shader) { name = "Generated_AreaFill" };
        }
    }
}