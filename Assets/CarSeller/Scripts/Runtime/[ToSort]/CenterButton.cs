using UnityEngine;
using UnityEngine.UI;

public class CenterButton : MonoBehaviour
{
    CanvasGroup group;
    Button button;
    Camera cam;

    //relative to the screen size( min of screen width and height)
    public float maxDistanceFromCenter = 0.5f;


    private void Awake()
    {
        group = GetComponent<CanvasGroup>();
        button = GetComponent<Button>();
        cam = Camera.main;
        button.onClick.AddListener(onClick);
    }

    private void onClick()
    {
        G.CameraMovementManager.Teleport(G.VehicleController.CurrentVehicleEntity.Position.WorldPosition);
    }

    // Update is called once per frame
    void Update()
    {
        if(!G.runIntialized)
            return;
        
        if(G.VehicleController.CurrentVehicleEntity == null)
        {
            group.alpha = 0;
            group.blocksRaycasts = false;
            return;
        }
        Vector3 targetPos = G.VehicleController.CurrentVehicleEntity.Position.WorldPosition;

        if (targetTooFarFromCenter(targetPos)) 
        {
            group.alpha = 1;
            group.blocksRaycasts = true;
        }
        else
        {
            group.alpha = 0;
            group.blocksRaycasts = false;
        }
    }

    // check if target too far from the center of screen(world z for reference point should be 0)
    bool targetTooFarFromCenter(Vector3 targetPos)
    {
        if (cam == null) cam = Camera.main;
        if (cam == null) return false;

        // Project the target onto the world Z=0 plane along the camera->target line.
        // This keeps the "distance from center" consistent for perspective cameras.
        Vector3 referencePos = targetPos;
        Plane z0Plane = new Plane(Vector3.forward, Vector3.zero);
        Vector3 camPos = cam.transform.position;
        Vector3 dir = targetPos - camPos;

        if (dir.sqrMagnitude > 0.000001f)
        {
            Ray ray = new Ray(camPos, dir);
            if (z0Plane.Raycast(ray, out float enter))
            {
                referencePos = ray.GetPoint(enter);
            }
        }

        Vector3 screenPos = cam.WorldToScreenPoint(referencePos);

        // Behind the camera => effectively "too far" (not centered / not visible in a sensible way).
        if (screenPos.z <= 0f) return true;

        float minDim = Mathf.Min(Screen.width, Screen.height);
        float maxDist = minDim * maxDistanceFromCenter;

        Vector2 screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        Vector2 p = new Vector2(screenPos.x, screenPos.y);

        return (p - screenCenter).sqrMagnitude > maxDist * maxDist;
    }
}
