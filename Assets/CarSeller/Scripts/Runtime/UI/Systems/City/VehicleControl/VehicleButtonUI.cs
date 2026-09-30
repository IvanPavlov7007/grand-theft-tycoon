using UnityEngine;
using UnityEngine.UI;

public class VehicleButtonUI : MonoBehaviour
{
    public Button button;
    public Image vehicleImage;
    public Image backgroundImage;

    readonly Color interactableColor = new Color(1f, 1f, 1f, 1f);
    readonly Color nonInteractableColor = new Color(1f, 1f, 1f, 0.5f);
    readonly Color interactableBackgroundColor = new Color(1f, 1f, 1f, 0.5f);
    readonly Color nonInteractableBackgroundColor = new Color(0.5f, 0.5f, 0.5f, 0.25f);

    public void MakeInteractable()
    {
        vehicleImage.color = interactableColor;
        backgroundImage.color = interactableBackgroundColor;
    }

    public void MakeNonInteractable()
    {
        vehicleImage.color = nonInteractableColor;
        backgroundImage.color = nonInteractableBackgroundColor;
    }

    public void SetVehicleImage(Sprite sprite)
    {
        vehicleImage.sprite = sprite;
    }
}