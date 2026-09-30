using UnityEngine;

[DisallowMultipleComponent]
public class ParallaxTransform : MonoBehaviour
{
    public Transform target;

    public float parallaxRange = 1f;

    public bool repeating;
    public float periodDistance;

    private Vector2 initDist;
    private void Start()
    {
        initDist = transform.position;
    }

    private void LateUpdate()
    {
        Vector2 camPosition = target.position;

        Vector2 dist = camPosition - (Vector2) transform.position;

        Vector2 newPos;
        if (repeating)
            newPos = new Vector2(determineParallaxedClosestPeriodicPosition(camPosition.x, initDist.x, periodDistance, parallaxRange),
                determineParallaxedClosestPeriodicPosition(camPosition.y, initDist.y, periodDistance, parallaxRange));
        else
            newPos = new Vector2( parallaxedPosition(camPosition.x, initDist.x, parallaxRange),
                parallaxedPosition(camPosition.y, initDist.y, parallaxRange));

        transform.position = newPos;
    }

     protected float determineClosestPeriodicPosition(float target_pos, float offsetDistance, float periodDistance)
    {
        float pos_inPeriod = (target_pos - offsetDistance) % periodDistance;
        if (pos_inPeriod < 0f)
            pos_inPeriod = periodDistance + pos_inPeriod;

        float globalPos_left = target_pos - pos_inPeriod;
        float globalPos_right = target_pos + (periodDistance - pos_inPeriod);

        float distToLeft = target_pos - globalPos_left;
        float distToRight = globalPos_right - target_pos;

        if (distToLeft <= distToRight)
        {
            return globalPos_left;
        }
        else
            return globalPos_right;
    }

    protected float parallaxedPosition(float target_pos, float offsetDistance, float parallax)
    {
        float par_coord_target_pos = target_pos  / parallax - offsetDistance / parallax;
        return target_pos - par_coord_target_pos;
    }

    protected float determineParallaxedClosestPeriodicPosition(float target_pos, float offsetDistance, float periodDistance, float parallax)
    {
        float pos_inPeriod = (target_pos - offsetDistance * parallax) % (periodDistance * parallax);

        if (pos_inPeriod < 0f)
            pos_inPeriod = periodDistance * parallax + pos_inPeriod;

        float globalPos_left = target_pos - pos_inPeriod;
        float globalPos_right = target_pos + (periodDistance * parallax - pos_inPeriod);

        float distToLeft = target_pos - globalPos_left;
        float distToRight = globalPos_right - target_pos;
        //I DONT KNOW HOW IT WORKS I JUST FORCED IT
        if (distToLeft <= distToRight)
        {
            return target_pos - (target_pos - globalPos_left) / parallax;
        }
        else
            return target_pos - (target_pos - globalPos_right) / parallax;
    }

}
