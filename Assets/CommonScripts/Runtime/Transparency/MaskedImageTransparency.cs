using UnityEngine;
using UnityEngine.UI;

public class MaskedImageTransparency : MonoBehaviour, ITransparencyController
{
    public float cutout;

    private Material material;
    private Image image;

    private void Awake()
    {
        image = GetComponent<Image>();
        // Make sure we have an instance of the material
        material = image.material;
        if (material == null)
        {
            Debug.LogWarning("No material assigned to Image.");
        }
    }
    void Update()
    {
        if (material != null)
        {
            material.SetFloat("_Cutoff", cutout);
        }
    }

    public static MaskedImageTransparency GetController(Image image)
    {
        MaskedImageTransparency result;
        if (!image.gameObject.TryGetComponent<MaskedImageTransparency>(out result))
        {
            result = image.gameObject.AddComponent<MaskedImageTransparency>();
        }
        return result;
    }

    public float Alpha { 
        get => cutout;
        set => cutout = value;
    }

}