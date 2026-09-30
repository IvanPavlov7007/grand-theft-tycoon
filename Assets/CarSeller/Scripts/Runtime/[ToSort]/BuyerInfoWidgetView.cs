using System;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuyerInfoWidgetView : WidgetView<BuyerInfoWidget>
{
    public Image CarTypeImage;
    public TMP_Text PaymentMultipliersText;
    public TMP_Text any_TypeHelperText;
    protected override RectTransform childrenContainer => null;
    protected override void Bind(BuyerInfoWidget widget)
    {
        CarTypeImage.sprite = G.cityViewObjectBuilder.BuyersPinSprites[widget.RequiredCarType];
        PaymentMultipliersText.text = getPaymentMultipliers(widget.RequiredCarType);
        if(widget.RequiredCarType == CarType.Any)
        {
            any_TypeHelperText.gameObject.SetActive(true);
        }
        else
        {
            any_TypeHelperText.gameObject.SetActive(false);
        }
    }

    string getPaymentMultipliers(CarType requiredType)
    {
        StringBuilder builder = new StringBuilder();
        for (int i = 0; i < Enum.GetNames(typeof(CarRarity)).Length; i++)
        {
            CarRarity rarity = (CarRarity)i;
            float multiplier = G.SellPriceCalculator.CalculateMultiplier(rarity,requiredType);
            builder.AppendLine($"{TextConventionsHelper.GetColoredRarityText(rarity)}: x{multiplier}");
        }
        return builder.ToString();
    }
}

public class BuyerInfoWidget : BlockingInputWidget
{
    public CarType RequiredCarType;

    public BuyerInfoWidget(Buyer buyer)
    {
        RequiredCarType = buyer.RequiredCarType;
    }   
}
