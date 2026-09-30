using Sirenix.OdinInspector;
using System.Collections;
using UnityEngine;

public class PoliceLightsVisuals : MonoBehaviour
{
    [SerializeField]
    GameObject redLight;
    [SerializeField]
    GameObject blueLight;

    bool currentRed = true;
    [ShowInInspector]
    public bool LightsOn { get; private set; } = true;

    const float lightSwitchIntervalSlow = 0.5f;
    const float lightSwitchIntervalFast = 0.3f;

    float lightSwitchInterval =0.4f;

    Coroutine _lightsRoutine;

    private void Awake()
    {
        lightSwitchInterval = Random.Range(lightSwitchIntervalSlow, lightSwitchIntervalFast);
    }

    void OnEnable()
    {
        ApplyInitialState();
        TryStartLights();
    }

    void OnDisable()
    {
        StopLights();
    }

    void Update()
    {
        // Allow toggling lights via a public flag.
        // If this flag is driven externally, this keeps the coroutine state in sync.
        if (LightsOn && _lightsRoutine == null)
        {
            TryStartLights();
        }
        else if (!LightsOn && _lightsRoutine != null)
        {
            StopLights();
        }
    }

    public void TurnOn()
    {
        LightsOn = true;
        TryStartLights();
    }

    public void TurnOff()
    {
        LightsOn = false;
        StopLights();
        SetLightsActive(false, false);
    }

    public void Toggle()
    {
        if (LightsOn)
            TurnOff();
        else
            TurnOn();
    }

    void TryStartLights()
    {
        if (!LightsOn)
            return;

        if (_lightsRoutine == null)
            _lightsRoutine = StartCoroutine(LightsLoop());
    }

    void StopLights()
    {
        if (_lightsRoutine != null)
        {
            StopCoroutine(_lightsRoutine);
            _lightsRoutine = null;
        }
    }

    IEnumerator LightsLoop()
    {
        yield return new WaitForSeconds(Random.value * 0.4f);
        // Basic alternating flash between red and blue.
        while (LightsOn)
        {
            currentRed = !currentRed;

            if (currentRed)
                SetLightsActive(true, false);
            else
                SetLightsActive(false, true);

            yield return new WaitForSeconds(lightSwitchInterval);
        }

        _lightsRoutine = null;
    }

    void ApplyInitialState()
    {
        if (!LightsOn)
        {
            SetLightsActive(false, false);
            return;
        }

        // Ensure one light starts on.
        if (currentRed)
            SetLightsActive(true, false);
        else
            SetLightsActive(false, true);
    }

    void SetLightsActive(bool redActive, bool blueActive)
    {
        if (redLight != null)
            redLight.SetActive(redActive);
        if (blueLight != null)
            blueLight.SetActive(blueActive);
    }
}