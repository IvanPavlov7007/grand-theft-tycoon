using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CarEntryView : WidgetView<CarEntryWidget>
{
    [SerializeField] Image image;
    [SerializeField] TMP_Text CarName;
    [SerializeField] TMP_Text CarRarity;
    [SerializeField] Image typeImage;
    [SerializeField] AutoHidingButtonWidgetView AutoHidingButtonWidgetView;

    protected override RectTransform childrenContainer => null;

    protected override void Bind(CarEntryWidget widget)
    {
        setUpVisuals(widget);
        AutoHidingButtonWidgetView.Bind(widget.buttonWidget);
    }

    void setUpVisuals(CarEntryWidget widget)
    {
        image.sprite = widget.sprite;
        CarName.text = widget.name;
        CarRarity.text = widget.rarity;
        typeImage.sprite = widget.typeSprite;

        if(image.sprite == null)
        {
            image.color = new Color(0, 0, 0, 0);
        }
        else
        {
            image.color = Color.white;
        }

        if(typeImage.sprite == null)
        {
            typeImage.color = new Color(0, 0, 0, 0);
        }
        else
        {
            typeImage.color = Color.white;
        }
    }
}

public class CarEntryWidget : Widget
{
    public Sprite sprite;
    public Sprite typeSprite;
    public string name;
    public string rarity;

    public AutoHidingButtonWidget buttonWidget;
    public string stateText;
    public Color? buttonColor;

    public CarEntryWidget(Car car, AutoHidingButtonWidget buttonWidget)
    {
        sprite = IconBuilderHelper.BuildProdutSpite(car);
        typeSprite = G.cityViewObjectBuilder.BuyersPinSprites[car.Kind.Type];
        name = car.Name;
        rarity = TextConventionsHelper.GetColoredRarityText(car.Kind.Rarity);
        this.buttonWidget = buttonWidget;
    }

    public CarEntryWidget(string name, AutoHidingButtonWidget buttonWidget)
    {
        sprite = null;
        typeSprite = null;
        this.name = name;
        rarity = "";
        this.buttonWidget = buttonWidget;
    }
}

public static class CarStashEntryWidgetCreator
{
    public static CarEntryWidget CreateDeployEntry(Car car, Action onClick)
    {
        var widget = new CarEntryWidget(car, new AutoHidingButtonWidget("Deploy",onClick, G.ColorPalette.LimeGreenColor, true, true));
        return widget;
    }

    public static CarEntryWidget CreateSwapStoreEntry(Car car, Action onClick)
    {
        var widget = new CarEntryWidget(car, new AutoHidingButtonWidget("Swap", onClick, G.ColorPalette.LimeGreenColor,true, true));
        return widget;
    }

    public static CarEntryWidget CreateEmptyStoreEntry(Action onClick)
    {
        var widget = new CarEntryWidget("Empty",new AutoHidingButtonWidget("Store", onClick, G.ColorPalette.LimeGreenColor,true, true));
        return widget;
    }
}

public static class  BuyCarButtonWidgetCreator
{
    static CarEntryWidget Create(string text, PersonalVehicleShopEntry entry, Action onClick)
    {
        bool interactable = GameRules.CanBePurchased.Check(entry);
        string unavailabilityReason = interactable ? "" : GameRules.CanBePurchased.GetUnavailabilityReason(entry);

        var widget = new CarEntryWidget(entry.PersonalVehicle.Car, new AutoHidingButtonWidget(text ,onClick, G.ColorPalette.LimeGreenColor, interactable, false, unavailabilityReason));

        return widget;
    }

    public static CarEntryWidget CreateBought(PersonalVehicleShopEntry entry, Action onClick)
    {
        var widget = Create("Select",entry, onClick);
        widget.buttonWidget.IsInteractable = true;
        widget.buttonWidget.CloseParentMenuOnClick = true;
        return widget;
    }

    public static CarEntryWidget CreateAvailable(PersonalVehicleShopEntry entry, Action onClick)
    {
        var widget = Create($"BUY: {TextConventionsHelper.FormatPrice(SellPriceWrapper.CalculateAbsolutePrice(entry.UnitPrice))}",
            entry, onClick);
        return widget;
    }
}

public static class PrimarySelectionCarButtonWidgetCreator
{
    public static CarEntryWidget CreateAvailable(Car car, Action onClick)
    {
        var widget = new CarEntryWidget(car, new AutoHidingButtonWidget("Select",onClick, G.ColorPalette.LimeGreenColor, true, true));
        return widget;
    }

    public static CarEntryWidget CreateSelected(Car car, Action onClick)
    {
        var widget = new CarEntryWidget(car, new AutoHidingButtonWidget("SELECTED",onClick, G.ColorPalette.LimeGreenColor, true, true));
        widget.buttonColor = Color.gray;

        return widget;
    }

}