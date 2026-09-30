using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CityUIPin : MonoBehaviour
{
    CityUIPinPositioner positioner;
    CityViewObjectController cityViewObjectController;

    Interactable interactable => cityViewObjectController.GetComponent<Interactable>();


    private void Awake()
    {
        positioner = GetComponent<CityUIPinPositioner>();
    }

    public void Initialize(CityViewObjectController cityViewObjectController, PinStyle pinStyle)
    {
        this.cityViewObjectController = cityViewObjectController;
        Camera cam = G.CityUIManager.Camera;
        Canvas canvas = G.CityUIManager.Canvas;
        positioner.Initialize(cam, canvas, cam.transform, cityViewObjectController.transform);

        var txt = positioner.GetComponentInChildren<TextMeshProUGUI>();
        txt.text = pinStyle.Text;
        txt.color = pinStyle.ForegroundColor;
        positioner.FrameRectTransform.GetComponent<Image>().color = pinStyle.BackgroundColor;
        var iconImage = positioner.IconRectTransform.GetComponent<Image>();
        iconImage.sprite = pinStyle.Icon;
        if(pinStyle.Icon != null)
            iconImage.color = pinStyle.ForegroundColor;
        else
            iconImage.color = Color.clear;

        gameObject.AddComponent<ViewStateChangerUI>().Initialize(cityViewObjectController);

        cityViewObjectController.OnDestroyed += handleObjectDestroyed;
        addButtonListeners();

        var scaler = cityViewObjectController.CityEntity.GetAspect<VisibleDistanceScalerAspect>();
        gameObject.AddComponent<VisionPinScaler>().Initialize(scaler);

        positioner.update();
        Update();
    }

    private void Update()
    {
        checkDiscovered();
        checkConfined();
    }

    private void checkConfined()
    {
        PinAspect pinAspect = cityViewObjectController.CityEntity.GetAspect<PinAspect>();
        if (pinAspect != null)
        {
            positioner.ConfinedToScreen = pinAspect.IsScreenConfined;
        }
    }

    private void checkDiscovered()
    {
        CityVisibleAspect visibleAspect = cityViewObjectController.CityEntity.GetAspect<CityVisibleAspect>();
        if (visibleAspect != null)
        {
            if (visibleAspect.Discovered)
            {
                positioner.SetGraphicsMode(CityUIPinPositioner.GraphicsMode.Pin);
                cityViewObjectController.CityEntity.GetAspect<PinAspect>().IsScreenConfined = true;
            }
            else
            {
                positioner.SetGraphicsMode(CityUIPinPositioner.GraphicsMode.Circle);
            }
        }
    }

    private void OnDisable()
    {
        if (cityViewObjectController != null)
        cityViewObjectController.OnDestroyed -= handleObjectDestroyed;

        removeButtonListeners();
    }

    private void addButtonListeners()
    {
        var btns = GetComponentsInChildren<Button>(true);
        foreach (var btn in btns)
        {
            btn.onClick.AddListener(onClick);
        }
    }

    private void removeButtonListeners()
    {
        var btns = GetComponentsInChildren<Button>(true);
        foreach (var btn in btns)
            btn.onClick.RemoveListener(onClick);
    }

    private void handleObjectDestroyed()
    {
        if(gameObject != null)
            Destroy(gameObject);
    }

    private void onClick()
    {
        Debug.Assert(interactable != null, "CityUIPin: No Interactable component found on the CityViewObjectController.");
        if(positioner.IsDragging)
            return;
        interactable?.CursorClick();
    }

}