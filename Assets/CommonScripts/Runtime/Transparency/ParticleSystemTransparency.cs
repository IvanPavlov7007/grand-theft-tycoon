using UnityEngine;

public class ParticleSystemTransparency : MonoBehaviour, ITransparencyController
{
    public float Alpha { 
        get
        {
            if (!initialize())
            {
                throw new UnityException(name + " can find an Alpha-Setting target");
            }
            return ps.main.startColor.color.a;
        }
        set
        {
            if (!initialize())
            {
                throw new UnityException(name + " can find an Alpha-Setting target");
            }
            var startColor = main.startColor;
            startColor.color = new Color(startColor.color.r, startColor.color.g, startColor.color.b, value);
            main.startColor = startColor;
        } 
    }

    ParticleSystem ps;
    ParticleSystem.MainModule main;
    private bool initialize()
    {
        if (ps != null)
            return true;
        ps = GetComponent<ParticleSystem>();
        main = ps.main;
        if (ps == null)
            return false;
        return true;
    }
}