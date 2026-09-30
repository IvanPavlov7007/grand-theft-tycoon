using System.Collections.Generic;
using UnityEngine;

public class TrafficLightViewController : MonoBehaviour
{
    public GameObject TrafficLight_LightPrefab;

    [SerializeField] private Transform LightsRoot;
    [SerializeField] private float LightOffset = 0.25f;

    [SerializeField] private Color GoColor;
    [SerializeField] private Color YellowColor;
    [SerializeField] private Color StopColor;

    private readonly Dictionary<string, TrafficLight_LightView> map = new Dictionary<string, TrafficLight_LightView>();

    public void InstantiateLights(Dictionary<string, Vector2> directions)
    {
        EnsureLightsRoot();

        map.Clear();

        for (int i = LightsRoot.childCount - 1; i >= 0; i--)
        {
            Destroy(LightsRoot.GetChild(i).gameObject);
        }

        foreach (var item in directions)
        {
            var direction = item.Value.sqrMagnitude > 0f ? item.Value.normalized : Vector2.up;

            var light = Instantiate(TrafficLight_LightPrefab, LightsRoot).GetComponent<TrafficLight_LightView>();
            light.transform.localPosition = direction * LightOffset;
            light.transform.up = direction;

            map[item.Key] = light;
        }
    }

    public void SetLightState(Dictionary<string, TrafficLightState> states)
    {
        foreach (var item in states)
        {
            if (map.TryGetValue(item.Key, out var light))
            {
                switch (item.Value)
                {
                    case TrafficLightState.Go:
                        light.SetColor(GoColor);
                        break;
                    case TrafficLightState.Yellow:
                        light.SetColor(YellowColor);
                        break;
                    case TrafficLightState.Stop:
                        light.SetColor(StopColor);
                        break;
                }
            }
        }
    }

    private void EnsureLightsRoot()
    {
        if (LightsRoot != null)
            return;

        var existing = transform.Find("Lights");
        if (existing != null)
        {
            LightsRoot = existing;
            return;
        }

        var root = new GameObject("Lights");
        root.transform.SetParent(transform, false);
        LightsRoot = root.transform;
    }
}

public enum TrafficLightState
{
    Go,
    Yellow,
    Stop
}