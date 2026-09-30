using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class InteractionPhysics
{
    public static Interactable RaycastForInteractable(LayerMask layerMask, Vector2 position, bool use3dPhysics = false)
    {
        //Interactable hitInteractable = null;

        //if (use3dPhysics)
        //{
        //    var rayHits = Physics.SphereCastAll(position, 0.1f, Vector3.up, 0.1f, layerMask);
        //    if (rayHits.Length > 0)
        //        hitInteractable = rayHits[0].transform.GetComponentInParent<Interactable>();
        //}
        //else
        //{
        //    var rayHits = Physics2D.RaycastAll(position, Vector2.zero, 1000f, layerMask);
        //    if (rayHits.Length > 0)
        //    {
        //        var tr = rayHits[0].rigidbody != null ? rayHits[0].rigidbody.transform : rayHits[0].transform;
        //        hitInteractable = tr.GetComponentInParent<Interactable>();
        //    }
        //}

        //return hitInteractable;
        var interactables = RaycastForInteractables(layerMask, position, use3dPhysics);
        if (interactables == null || interactables.Length == 0)
            return null;

        return interactables.OrderByDescending(x => x.sortingOrder).First();

    }

    public static Interactable[] RaycastForInteractables(LayerMask layerMask, Vector2 position, bool use3dPhysics = false)
    {
        List<Interactable> hitInteractables = new List<Interactable>();

        if (use3dPhysics)
        {
            var rayHits = Physics.SphereCastAll(position, 0.1f, Vector3.up, 0.1f, layerMask);
            if (rayHits.Length > 0)
            {
                foreach (var hit in rayHits)
                {
                    var interactable = hit.transform.GetComponentInParent<Interactable>();
                    if (interactable != null && !hitInteractables.Contains(interactable))
                    {
                        hitInteractables.Add(interactable);
                    }
                }
            }
        }
        else
        {
            var rayHits = Physics2D.RaycastAll(position, Vector2.zero, 1000f, layerMask);
            if (rayHits.Length > 0)
            {
                foreach (var hit in rayHits)
                {
                    var tr = hit.rigidbody != null ? hit.rigidbody.transform : hit.transform;
                    var interactable = tr.GetComponentInParent<Interactable>();
                    if (interactable != null && !hitInteractables.Contains(interactable))
                    {
                        hitInteractables.Add(interactable);
                    }
                }
            }
        }

        return hitInteractables.ToArray();
    }
}