using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CarInfoWidgetView : WidgetView<CarInfoWidget>
{
    public TMP_Text Header;
    public Image CarImage;
    public TMP_Text RarityText;
    public TMP_Text NameText;
    public Image TypeImage;
    public AutoHidingButtonWidgetView ExitButton;

    protected override RectTransform childrenContainer => null;

    protected override void Bind(CarInfoWidget widget)
    {
        Header.text = widget.HeaderName;
        CarImage.sprite = widget.CarImage;
        RarityText.text = TextConventionsHelper.GetColoredRarityText(widget.CarRarity);
        NameText.text = widget.CarName;
        TypeImage.sprite = G.cityViewObjectBuilder.BuyersPinSprites[widget.CarType];
        ExitButton.Bind(widget.ExitButton);
    }
}

public class CarInfoWidget : BlockingInputWidget
{
    public string HeaderName;
    public string CarName;
    public CarType CarType;
    public CarRarity CarRarity;
    public Sprite CarImage;
    public AutoHidingButtonWidget ExitButton;

    private CarInfoWidget(Car car)
   {
        CarName = car.Name;
        CarType = car.Kind.Type;
        CarRarity = car.Kind.Rarity;
        CarImage = IconBuilderHelper.BuildProdutSpite(car);
    }


    public static CarInfoWidget StolenCar(Car car)
    {
        var carInfo = new CarInfoWidget(car);
        carInfo.HeaderName = "Stolen Vehicle";
        carInfo.ExitButton = new AutoHidingButtonWidget("Exit",G.VehicleController.ExitWorldVehicle, G.ColorPalette.WhiteColor, true, true);
        return carInfo;
    }

    public static CarInfoWidget WorldCar(Car car)
    {
        var carInfo = new CarInfoWidget(car);
        carInfo.HeaderName = "World Vehicle";
        carInfo.ExitButton = new AutoHidingButtonWidget(null, true, true);
        return carInfo;
    }

    public static CarInfoWidget PrimaryCar(Car car)
    {
        var carInfo = new CarInfoWidget(car);
        carInfo.HeaderName = "Primary Vehicle";
        carInfo.ExitButton = new AutoHidingButtonWidget(null, true, true);
        return carInfo;
    }
}
