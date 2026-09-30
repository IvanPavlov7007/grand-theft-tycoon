// Version: 2
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public sealed class CustomTrigger2D : MonoBehaviour
{
    public UnityEvent<Collider2D> onEnter;
    public UnityEvent<Collider2D> onStay;
    public UnityEvent<Collider2D> onExit;

    bool armed; // To prevent triggering on the same frame as enabling

    IEnumerator Start()
    {
#if DEBUG
        if(onEnter == null)
        {
            Debug.LogWarning("No event listeners on this trigger" + ToString());
        }
#endif
        yield return new WaitForFixedUpdate();
        armed = true;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (armed && onEnter != null)
            onEnter.Invoke(collision);
    }

    void OnTriggerStay2D(Collider2D collision)
    {
        if (armed && onStay != null)
            onStay.Invoke(collision);
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (armed && onExit != null)
            onExit.Invoke(collision);
    }

}
