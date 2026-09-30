using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Pure rendering layer for fog-of-war.
/// Tracks city entities that currently have `VisionCenterAspect` and maintains SpriteMask circles.
/// 
/// Game logic that decides which entities are centers lives in `CityVision`.
/// </summary>
public sealed class CityFogRenderer : GlobalSingletonBehaviour<CityFogRenderer>
{
    protected override CityFogRenderer GlobalInstance { get => G.CityFogRenderer; set => G.CityFogRenderer = value; }

    [Header("Mask Prefab")]
    [Tooltip("Prefab with a SpriteMask (and optionally a SpriteRenderer for debug).")]
    public GameObject CircleMaskPrefab;

    [Header("Runtime")]
    [Tooltip("Optional parent for spawned circle masks. If null, masks are parented to this GameObject.")]
    [SerializeField] private Transform masksParent;

    private readonly Dictionary<VisionCenterAspect, GameObject> circleMasks = new();

    CityVisionCentersSystem centersSystem;

    protected override void Awake()
    {
        base.Awake();
        if (!IsActiveSingleton) return;

        if (masksParent == null) masksParent = transform;

        if(!G.runIntialized)
            return;
        InitializeSystem(G.City.AspectsSystem.centersSystem);
    }

    public void InitializeSystem(CityVisionCentersSystem centersSystem)
    {
        Debug.Assert(centersSystem != null);
        this.centersSystem = centersSystem;

        centersSystem.OnCenterAdded += OnCenterChanged;
        centersSystem.OnCenterRemoved += OnCenterChanged;

        Rebuild();
    }

    private void OnDisable()
    {
        if (centersSystem != null)
        {
            centersSystem.OnCenterAdded -= OnCenterChanged;
            centersSystem.OnCenterRemoved -= OnCenterChanged;
        }

        ClearAllMasks();
    }

    private void LateUpdate()
    {
        UpdateMasksAndCenters();
    }

    private void OnCenterChanged(CityEntity entity, VisionCenterAspect aspect) => Rebuild();

    private void Rebuild()
    {
        Debug.Assert(centersSystem != null, "CityFogRenderer.Rebuild called but G.CityVision is null");

        var centers = centersSystem.VisionCenters;

        var toRemove = circleMasks.Keys.Where(e => !centers.Contains(e)).ToList();
        for (int i = 0; i < toRemove.Count; i++)
            RemoveMask(toRemove[i]);

        for (int i = 0; i < centers.Count; i++)
            EnsureMask(centers[i]);

        UpdateMasksAndCenters();
    }

    private void EnsureMask(VisionCenterAspect aspect)
    {

        Debug.Assert(aspect != null, "CityFogRenderer.EnsureMask called with null aspect");

        if (circleMasks.ContainsKey(aspect)) return;

        if (CircleMaskPrefab == null)
        {
            Debug.LogWarning("CityFogRenderer: CircleMaskPrefab is not assigned.");
            return;
        }

        var go = Instantiate(CircleMaskPrefab, masksParent);
        go.name = $"VisionMask_{aspect.Entity}";
        circleMasks[aspect] = go;
    }

    private void RemoveMask(VisionCenterAspect aspect)
    {
        Debug.Assert(aspect != null, "CityFogRenderer.RemoveMask called with null aspect");

        if (circleMasks.TryGetValue(aspect, out var go))
        {
            if (go != null) Destroy(go);
            circleMasks.Remove(aspect);
        }
    }

    private void ClearAllMasks()
    {
        foreach (var kv in circleMasks)
        {
            if (kv.Value != null) Destroy(kv.Value);
        }
        circleMasks.Clear();
    }

    private void UpdateMasksAndCenters()
    {
        foreach (var kv in circleMasks)
        {
            var aspect = kv.Key;
            var go = kv.Value;
            if (aspect == null || go == null) continue;

            var pos = aspect.Entity.Position.WorldPosition;

            go.transform.position = new Vector3(pos.x, pos.y, go.transform.position.z);
            float diameter = Mathf.Max(0.01f, aspect.Config.Radius * 2f);
            go.transform.localScale = new Vector3(diameter, diameter, 1f);

            var mask = go.GetComponent<SpriteMask>();
        }
    }
}
