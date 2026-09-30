using Pixelplacement;
using System.Collections.Generic;
using System.Linq;
using Unity.Cinemachine;
using UnityEngine;

public class ProgressOverviewManager : MonoBehaviour
{
    [SerializeField]
    CinemachineCamera progressOverviewCamera;
    [SerializeField]
    CanvasGroup progressOverviewCanvasGroup;

    [Header("UI elements")]
    [SerializeField]
    ProgressionUI area1progression;
    [SerializeField]
    ProgressionUI area2progression;
    [SerializeField]
    ProgressionUI area3progression;

    public bool IsProgressOverviewActive { get; private set; } = false;

    const int priorityWhenActive = 100;
    const int priorityWhenInactive = 0;

    Dictionary<CityArea, ProgressionUI> _areaToProgressionUI = new Dictionary<CityArea, ProgressionUI>();

    private void Start()
    {
        if(!G.runIntialized)
            return;
        
        var list = G.Areas.Values.ToList();
        _areaToProgressionUI[list[0]] = area1progression;
        _areaToProgressionUI[list[1]] = area2progression;
        _areaToProgressionUI[list[2]] = area3progression;
        foreach (var area in G.Areas.Values)
        {
                resetProgressbar(area);
        }

        CityAreasVisualsController.Ensure(World.Instance.City);
        HideProgressOverview();
    }

    private void OnEnable()
    {
        GameEvents.Instance.onAreaProgressed += onAreaProgressed;
    }

    private void OnDisable()
    {
        GameEvents.Instance.onAreaProgressed -= onAreaProgressed;
    }

    public void ShowProgressOverview()
    {
        progressOverviewCamera.Priority = priorityWhenActive;
        Tween.CanvasGroupAlpha(progressOverviewCanvasGroup, 1f, 0.5f ,0f);
        SetAreas(true);
    }

    public void HideProgressOverview()
    {
        progressOverviewCamera.Priority = priorityWhenInactive;
        Tween.CanvasGroupAlpha(progressOverviewCanvasGroup, 0f, 0.5f, 0f);
        SetAreas(false);
    }

    private void SetAreas(bool shown)
    {
        if(!G.runIntialized)
            return;
        var vc = GameObject.FindAnyObjectByType<CityAreasVisualsController>();
        Debug.Assert(vc != null, "CityAreasVisualsController not found in scene");
        foreach (var area in G.Areas.Values)
        {
            vc.SetHidden(area.Id, !shown);
        }
    }

    public void SwitchProgressOverview()
    {
        if (IsProgressOverviewActive)
            HideProgressOverview();
        else
            ShowProgressOverview();
        IsProgressOverviewActive = !IsProgressOverviewActive;
    }

    void onAreaProgressed(AreaProgressEventData data)
    {
        Debug.Assert(_areaToProgressionUI != null, "areaToProgressionUI is not initialized");

        UpdateAreaProgression(data.Area, data.NewLevel, data.NewXP);
        //progressionUI.targetIsMax = data.NewLevel.Final;
    }

    private void resetProgressbar(CityArea area)
    {
        if (!_areaToProgressionUI.TryGetValue(area, out var progressionUI))
            return;
        progressionUI.description.text = $"Progress in {area.DisplayName} Area";
        var levelIndex = area.CurrentLevel.Index;
        var normalizedProgress = ProgressionUIManager.NormalizeXP(area.currentXP, area.CurrentLevel);
        progressionUI.SetupInitValues(levelIndex, normalizedProgress);
        progressionUI.targetIsMax = area.CurrentLevel.Final;
    }

    void UpdateAreaProgression(CityArea area, AreaLevel level, float xp)
    {
        if (!_areaToProgressionUI.TryGetValue(area, out var progressionUI))
            return;
        progressionUI.targetProgressLevel = level.Index;
        progressionUI.targetProgressValue = ProgressionUIManager.NormalizeXP(xp, level);
        progressionUI.targetIsMax = level.Final;
    }

}
