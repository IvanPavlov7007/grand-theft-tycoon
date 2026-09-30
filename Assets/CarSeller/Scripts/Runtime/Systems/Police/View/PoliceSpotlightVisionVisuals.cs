using System;
using UnityEngine;

public class PoliceSpotlightVisionVisuals : MonoBehaviour
{
    PoliceStateViewController policeStateViewController;
    PoliceUnit policeUnit;
    SpotlightColors spotlightColors => G.PoliceManager.SpotlightColors;

    MeshFilter meshFilter;
    MeshRenderer meshRenderer;
    Mesh coneMesh;

    [Header("Visuals")]
    [Tooltip("Optional. If null, a simple material will be created at runtime.")]
    [SerializeField] private Material coneMaterial;

    public void Intialize(PoliceUnit policeUnit, PoliceStateViewController policeStateViewController)
    {
        this.policeUnit = policeUnit;
        this.policeStateViewController = policeStateViewController;
        policeStateViewController.OnStateChanged += stateUpdate;

        // Ensure we have required components
        meshFilter = GetComponent<MeshFilter>();
        if (meshFilter == null)
            meshFilter = gameObject.AddComponent<MeshFilter>();

        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer == null)
            meshRenderer = gameObject.AddComponent<MeshRenderer>();

        // Ensure we have a valid material, otherwise Unity shows pink
        EnsureMaterial();

        createVisualSlice();

        stateUpdate(policeStateViewController.currentState());
    }

    private void EnsureMaterial()
    {
        // Prefer explicit serialized material if provided
        if (coneMaterial != null)
        {
            meshRenderer.sharedMaterial = coneMaterial;
            return;
        }

        // If meshRenderer already has a material assigned in the scene, keep it
        if (meshRenderer.sharedMaterial != null)
            return;

        // Fallback: create a simple transparent material using a built‑in shader
        var shader = Shader.Find("Sprites/Default");
        if (shader == null)
        {
            shader = Shader.Find("Universal Render Pipeline/Unlit"); // if using URP
        }

        if (shader != null)
        {
            coneMaterial = new Material(shader)
            {
                color = new Color(1f, 1f, 1f, 0.3f) // default semi‑transparent
            };
            coneMaterial.name = "GeneratedPoliceConeMat";
            meshRenderer.sharedMaterial = coneMaterial;
        }
        else
        {
            Debug.LogWarning("PoliceSpotLightVisuals: Could not find a suitable shader. Please assign a material manually.");
        }
    }

    /// <summary>
    /// Creates a 2D cone mesh (fan) in local X‑Y plane, pointing along +Y.
    /// Radius and angle are taken from policeUnit.ConeVision.
    /// </summary>
    private void createVisualSlice()
    {
        if (policeUnit == null || policeUnit.ConeVision == null) return;

        float radius = policeUnit.ConeVision.Settings.Radius;
        float angleDeg = policeUnit.ConeVision.Settings.Angle;
        int segments = Mathf.Max(8, Mathf.RoundToInt(angleDeg / 5f)); // ~5° per segment

        coneMesh = new Mesh();
        coneMesh.name = "PoliceConeMesh";

        Vector3[] vertices = new Vector3[segments + 2];
        int[] triangles = new int[segments * 3];

        vertices[0] = Vector3.zero; // center

        float halfAngleRad = 0.5f * angleDeg * Mathf.Deg2Rad;
        float startAngle = -halfAngleRad;
        float endAngle = halfAngleRad;

        for (int i = 0; i <= segments; i++)
        {
            float t = (float)i / segments;
            float ang = Mathf.Lerp(startAngle, endAngle, t);

            float x = Mathf.Sin(ang) * radius;
            float y = Mathf.Cos(ang) * radius;

            vertices[i + 1] = new Vector3(x, y, 0);
        }

        for (int i = 0; i < segments; i++)
        {
            int baseIndex = i * 3;
            triangles[baseIndex] = 0;
            triangles[baseIndex + 1] = i + 1;
            triangles[baseIndex + 2] = i + 2;
        }

        coneMesh.vertices = vertices;
        coneMesh.triangles = triangles;
        coneMesh.RecalculateNormals();
        coneMesh.RecalculateBounds();

        meshFilter.sharedMesh = coneMesh;
    }

    private void stateUpdate(PoliceUnitState state)
    {
        if (policeUnit == null || policeUnit.GraphMovement == null) return;

        // Optional: change color by police state
        if (meshRenderer != null && meshRenderer.sharedMaterial != null)
        {
            Color c = spotlightColors.idle;
            switch (state)
            {
                case PoliceUnitState.Chase:
                    c = spotlightColors.chase;
                    break;
                case PoliceUnitState.TryRelocate:
                case PoliceUnitState.Backup:
                    c = spotlightColors.alert;
                    break;
            }

            meshRenderer.sharedMaterial.color = c;
        }
    }
}

[Serializable]
public class SpotlightColors
{
    public Color chase = Color.red;
    public Color alert = Color.yellow;
    public Color idle = Color.blue;
}