using System;
using System.Collections.Generic;
using UnityEngine;

public class CarShopUIManager : GlobalSingletonBehaviour<CarShopUIManager>
{
    protected override CarShopUIManager GlobalInstance { get => G.CarShopUIManager; set => G.CarShopUIManager = value; }

    public RectTransform content;
    public SimpleUIBuilder uiBuilder;

    public void UpdateOffers(CarShopOffer[] offers, Action<CarShopOffer> onOfferClick)
    {
        foreach(Transform child in content)
        {
            Destroy(child.gameObject);
        }
        var rootElement = GenerateUIElements(offers, onOfferClick);
        uiBuilder.Build(rootElement, content);
    }

    private UIElement GenerateUIElements(CarShopOffer[] offers, Action<CarShopOffer> onOfferClick)
    {
        UIElement rootElement = new UIElement
        {
            Type = UIElementType.Container,
            Children = new List<UIElement>()
        };

        foreach(var offer in offers)
        {
            rootElement.Children.Add(generateCarOfferUIElement(offer, onOfferClick));
        }
        return rootElement;
    }

    private UIElement generateCarOfferUIElement(CarShopOffer offer, Action<CarShopOffer> onOfferClick)
    {
        var carToPlayer = offer.toPlayer;
        UIElement offerElement = new UIElement
        {
            Type = UIElementType.ButtonContainer,
            IsInteractable = offer.CanAccept(),
            OnClick = () => onOfferClick(offer),
            UnavailabilityReason = "Cannot afford",
            Children = new List<UIElement>()
            {
                CTX_Menu_Tools.CarIcon(carToPlayer),
                CTX_Menu_Tools.CarRarityText(carToPlayer),
                CTX_Menu_Tools.Price(offer.MoneyDelta, true)
            }
        };
        return offerElement;
    }
}