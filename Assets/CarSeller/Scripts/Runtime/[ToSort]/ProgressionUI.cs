using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using Pixelplacement;

public class ProgressionUI : MonoBehaviour
{
    public TMP_Text description;
    public TMP_Text nextLevelIndexDisplayer;
    public Slider slider;
    public ParticleSystem particlesSystem;
    public Image levelBackground;
    public CanvasGroup maxHint;

    public float progressSpeed = 1f;

    public float targetProgressValue;
    public int targetProgressLevel;
    public bool targetIsMax;

    float _currentProgressValue;
    int _currentProgressLevel;
    

    const float TERMINAL_PROGRESS_VALUE = 0.999f;

    public void SetupInitValues(int currentLevel, float currentProgress)
    {
        _currentProgressLevel = currentLevel;
        _currentProgressValue = currentProgress;
        targetProgressLevel = currentLevel;
        targetProgressValue = currentProgress;
    }

    [Button]
    public void ResetBar()
    {
        _currentProgressLevel = 1;
        _currentProgressValue = 0f;
    }
    
    
    void Update()
    {
        if (_currentProgressLevel < targetProgressLevel)
        {
            fillUp(float.MaxValue);
            if (_currentProgressValue > TERMINAL_PROGRESS_VALUE)
            {
                iterateLevel();
            }
        }
        else
        {
            fillUp(targetProgressValue);
        }
        updateIndexDisplayer();
        updateProgressBar();
    }

    void iterateLevel()
    {
        _currentProgressLevel++;
        _currentProgressValue = 0f;
        particlesSystem.Play();
        animateLevelBackground();
        updateIndexDisplayer();
    }

    void animateLevelBackground()
    {
        Tween.Size(levelBackground.rectTransform, levelBackground.rectTransform.sizeDelta * 1.2f, 0.3f,0f,Tween.EaseOutBack,
            completeCallback: () =>
            {
                Tween.Size(levelBackground.rectTransform, levelBackground.rectTransform.sizeDelta / 1.2f, 0.3f,0f, Tween.EaseOutBack);
            });
        var initColor = levelBackground.color;
        Tween.Color(levelBackground, Color.white, 0.3f, 0f, Tween.EaseOutBack);
        Tween.Color(levelBackground, initColor, 0.3f, 0.3f, Tween.EaseOutBack);
    }

    bool atMax => targetIsMax && _currentProgressLevel == targetProgressLevel;

    void updateIndexDisplayer()
    {
        nextLevelIndexDisplayer.text = _currentProgressLevel.ToString();
        if(atMax)
        {
            maxHint.alpha = 1f;
        }
        else
        {
            maxHint.alpha = 0f;
        }
    }

    void updateProgressBar()
    {
        if(atMax)
        {
            slider.value = 1f;
            return;
        }
        slider.value = _currentProgressValue;
    }

    void fillUp(float maxProgress)
    {
        _currentProgressValue += progressSpeed * Time.deltaTime;
        _currentProgressValue = Mathf.Min(_currentProgressValue, maxProgress);
    }

}