using UnityEngine;

public class ProgressionUIManager : MonoBehaviour
{
    public ProgressionUI progressionUI;

    CanvasGroup canvasGroup;
    CityArea currentArea;

    private void Awake()
    {
        canvasGroup = progressionUI.GetComponent<CanvasGroup>();
        Debug.Assert(canvasGroup != null, "ProgressionUI should have a CanvasGroup component.");
    }

    void Start()
    {
        GameEvents.Instance.onAreaProgressed += onAreaProgressed;
        resetProgressbar(null, 0f);
    }

    private void OnDestroy()
    {
        if (GameEvents.Instance != null)
            GameEvents.Instance.onAreaProgressed -= onAreaProgressed;
    }

    public static float NormalizeXP(float xp, AreaLevel level)
    {
        if (level == null || level.XpToNextLevel <= 0f)
            return 0f;

        return Mathf.Clamp01(xp / level.XpToNextLevel);
    }

    private void resetProgressbar(AreaLevel level, float xp)
    {
        var levelIndex = level != null ? level.Index : 0;
        var normalizedProgress = NormalizeXP(xp, level);
        progressionUI.SetupInitValues(levelIndex, normalizedProgress);
        progressionUI.targetIsMax = level?.Final ?? false;
    }

    bool _showing = false;
    float _showingTime = 0f;
    public float fadeSpeed = 2f;
    public float shownDuration = 3f;

    private void Update()
    {
        // if showing, rise group alpha to 1, then after shownDuration, turn showing to false
        // if not showing, lower group alpha to 0
        if (_showing)
        {
            canvasGroup.alpha = Mathf.Min(1f, canvasGroup.alpha + fadeSpeed * Time.deltaTime);
            _showingTime += Time.deltaTime;
            if (_showingTime >= shownDuration)
            {
                _showing = false;
            }
        }
        else
        {
            canvasGroup.alpha = Mathf.Max(0f, canvasGroup.alpha - fadeSpeed * Time.deltaTime);
        }
    }

    void onAreaProgressed(AreaProgressEventData data)
    {
        if (currentArea != data.Area)
        {
            currentArea = data.Area;
            resetProgressbar(data.InitialLevel, data.InitialXP);
            progressionUI.description.text = $"Progress in {currentArea.DisplayName} Area";
        }

        _showing = true;
        _showingTime = 0f;

        progressionUI.targetProgressLevel = data.NewLevel.Index;
        progressionUI.targetProgressValue = NormalizeXP(data.NewXP, data.NewLevel);
        progressionUI.targetIsMax = data.NewLevel.Final;
        //progressionUI.targetIsMax = data.NewLevel.Final;
    }
}