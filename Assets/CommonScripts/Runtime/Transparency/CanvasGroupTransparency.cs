using UnityEngine;

public class CanvasGroupTransparency : MonoBehaviour, ITransparencyController
{
    public float Alpha
    {
        get
        {
            if (!initialize())
            {
                throw new UnityException(name + " can find an Alpha-Setting target");
            }
            return gr.alpha;
        }
        set
        {
            if (!initialize())
            {
                throw new UnityException(name + " can find an Alpha-Setting target");
            }
            gr.alpha = value;

        }
    }

    CanvasGroup gr;
    private bool initialize()
    {
        if (gr != null)
            return true;
        gr = GetComponent<CanvasGroup>();
        if (gr == null)
            return false;
        return true;
    }
}