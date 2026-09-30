using System.Collections.Generic;
using UnityEngine;

public sealed class CityAreaVisuals : MonoBehaviour
{
    public string AreaId { get; private set; }

    public bool IsHidden { get; private set; }
    public bool IsHighlighted { get; private set; }

    private CityAreasVisualsController.Settings _settings;

    private LineRenderer _outline;
    private MeshFilter _fillMeshFilter;
    private MeshRenderer _fillMeshRenderer;

    private Mesh _fillMesh;
    private readonly List<int> _triangles = new List<int>();

    private MaterialPropertyBlock _fillProps;

    public void Initialize(string areaId, Vector2[] polygonWorld, CityAreasVisualsController.Settings settings)
    {
        AreaId = areaId;
        _settings = settings;

        EnsureComponents();
        BuildOutline(polygonWorld);

        if (_settings.DrawFill)
            BuildFill(polygonWorld);
        else
            DisableFill();

        SetHidden(_settings.StartHidden);
        SetHighlighted(false);
    }

    public void SetHidden(bool hidden)
    {
        IsHidden = hidden;
        gameObject.SetActive(!hidden);
    }

    public void SetHighlighted(bool highlighted)
    {
        IsHighlighted = highlighted;

        if (_outline != null)
        {
            _outline.startWidth = highlighted ? _settings.HighlightOutlineWidth : _settings.NormalOutlineWidth;
            _outline.endWidth = _outline.startWidth;

            var c = highlighted ? _settings.HighlightOutlineColor : _settings.NormalOutlineColor;
            _outline.startColor = c;
            _outline.endColor = c;
        }

        if (_settings.DrawFill && _fillMeshRenderer != null)
        {
            EnsureFillProps();
            var c = highlighted ? _settings.HighlightFillColor : _settings.NormalFillColor;
            _fillProps.SetColor(_settings.FillColorProperty, c);
            _fillMeshRenderer.SetPropertyBlock(_fillProps);
        }
    }

    private void EnsureComponents()
    {
        _outline = GetComponent<LineRenderer>();
        if (_outline == null)
            _outline = gameObject.AddComponent<LineRenderer>();

        _outline.useWorldSpace = true;
        _outline.loop = true;
        _outline.numCornerVertices = 2;
        _outline.numCapVertices = 2;
        _outline.alignment = LineAlignment.View;
        _outline.textureMode = LineTextureMode.Stretch;

        if (!string.IsNullOrEmpty(_settings.SortingLayerName))
            _outline.sortingLayerName = _settings.SortingLayerName;

        _outline.sortingOrder = _settings.OutlineSortingOrder;

        if (_settings.OutlineMaterial != null)
            _outline.sharedMaterial = _settings.OutlineMaterial;

        if (_settings.DrawFill)
        {
            _fillMeshFilter = GetComponent<MeshFilter>();
            if (_fillMeshFilter == null)
                _fillMeshFilter = gameObject.AddComponent<MeshFilter>();

            _fillMeshRenderer = GetComponent<MeshRenderer>();
            if (_fillMeshRenderer == null)
                _fillMeshRenderer = gameObject.AddComponent<MeshRenderer>();

            if (!string.IsNullOrEmpty(_settings.SortingLayerName))
                _fillMeshRenderer.sortingLayerName = _settings.SortingLayerName;

            _fillMeshRenderer.sortingOrder = _settings.FillSortingOrder;

            if (_settings.FillMaterial != null)
                _fillMeshRenderer.sharedMaterial = _settings.FillMaterial;
        }
    }

    private void BuildOutline(Vector2[] polygonWorld)
    {
        if (polygonWorld == null || polygonWorld.Length < 3)
        {
            _outline.positionCount = 0;
            return;
        }

        _outline.positionCount = polygonWorld.Length;
        for (int i = 0; i < polygonWorld.Length; i++)
            _outline.SetPosition(i, new Vector3(polygonWorld[i].x, polygonWorld[i].y, _settings.ZOffset));
    }

    private void BuildFill(Vector2[] polygonWorld)
    {
        if (_fillMeshFilter == null || _fillMeshRenderer == null)
            return;

        if (polygonWorld == null || polygonWorld.Length < 3)
        {
            DisableFill();
            return;
        }

        if (!PolygonTriangulator.TryTriangulate(polygonWorld, _triangles))
        {
            DisableFill();
            return;
        }

        if (_fillMesh == null)
        {
            _fillMesh = new Mesh();
            _fillMesh.name = $"AreaFill_{AreaId}";
        }
        else
        {
            _fillMesh.Clear();
        }

        var verts = new Vector3[polygonWorld.Length];
        var uvs = new Vector2[polygonWorld.Length];

        for (int i = 0; i < polygonWorld.Length; i++)
        {
            verts[i] = new Vector3(polygonWorld[i].x, polygonWorld[i].y, _settings.ZOffset);
            uvs[i] = polygonWorld[i];
        }

        _fillMesh.vertices = verts;
        _fillMesh.uv = uvs;
        _fillMesh.triangles = _triangles.ToArray();
        _fillMesh.RecalculateBounds();
        _fillMesh.RecalculateNormals();

        _fillMeshFilter.sharedMesh = _fillMesh;

        EnsureFillProps();
        _fillProps.SetColor(_settings.FillColorProperty, _settings.NormalFillColor);
        _fillMeshRenderer.SetPropertyBlock(_fillProps);
        _fillMeshRenderer.enabled = true;
    }

    private void DisableFill()
    {
        if (_fillMeshRenderer != null)
            _fillMeshRenderer.enabled = false;

        if (_fillMeshFilter != null)
            _fillMeshFilter.sharedMesh = null;
    }

    private void EnsureFillProps()
    {
        if (_fillProps == null)
            _fillProps = new MaterialPropertyBlock();
    }
}