using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;



public class StayArea : MonoBehaviour
{
    [SerializeField]
    public CustomTrigger2D trigger;
    public float stayTargetTime;
    [Tooltip("Resend trigger each frame after condition met")]
    public bool repeatEveryFrame;

    protected Dictionary<Entity, Timer> timers = new Dictionary<Entity, Timer>();

    public UnityEvent<Entity> onStay;
    public UnityEvent<Entity> onExited;

    protected virtual void Start()
    {
        trigger.onEnter.AddListener(objectEntered);
        trigger.onStay.AddListener(objectStayed);
        trigger.onExit.AddListener(objectExited);
    }

    public virtual void objectEntered(Collider2D collision)
    {
        var entity = collision.GetComponentInParent<Entity>();
        if (entity == null)
            return;
        if (!timers.ContainsKey(entity))
            timers.Add(entity, new Timer());
    }
    public virtual void objectStayed(Collider2D collision)
    {
        var entity = collision.GetComponentInParent<Entity>();
        if (entity == null)
            return;
        if (!timers.ContainsKey(entity))
        {
            Debug.Log(collision.ToString() + "- unregistered, stays at trigger : " + ToString());
        }
        else
        {
            timers[entity].elapsedTime += Time.deltaTime;
            if(timers[entity].elapsedTime >= stayTargetTime)
            {
                if (repeatEveryFrame || !timers[entity].triggered)
                {
                    onStay.Invoke(entity);
                    timers[entity].triggered = true;
                }
            }
        }
    }
    public virtual void objectExited(Collider2D collision)
    {
        var entity = collision.GetComponentInParent<Entity>();
        if (entity == null)
            return;
        if(timers.Remove(entity))
            onExited.Invoke(entity);
    }

    protected class Timer
    {
        public bool triggered;
        public float elapsedTime;
    }
}
