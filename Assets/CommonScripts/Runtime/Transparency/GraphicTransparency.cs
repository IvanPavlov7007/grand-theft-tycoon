using UnityEngine;
using UnityEngine.UI;

public class GraphicTransparency : MonoBehaviour, ITransparencyController
{
    public float Alpha { get
        {
            if (!initialize())
            {
                throw new UnityException(name + " can find an Alpha-Setting target");
            }
            return gr.color.a;
        }
        set
        {
            if (!initialize())
            {
                throw new UnityException(name + " can find an Alpha-Setting target");
            }
            var c = gr.color;
            c.a = value;
            gr.color = c;

        }
    }

    Graphic gr;
    private bool initialize()
    {
        if (gr != null)
            return true;
        gr = GetComponent<Graphic>();
        if (gr == null)
            return false;
        return true;
    }

}