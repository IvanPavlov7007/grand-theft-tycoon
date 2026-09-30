using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Utility tools for building context menu UI elements.
/// Collection of methods based on similar functionality
/// ( to have implementation in one place and avoid code duplication ).
/// </summary>
public static class CTX_Menu_Tools
{
    // General
    public static UIElement Header(string name)
    {
        return new UIElement
        {
            Type = UIElementType.Text,
            Text = $"{name}",
            Style = "header"
        };
    }

    public static UIElement CarRarityText(Car car)
    {
        return Header(TextConventionsHelper.GetColoredRarityText(car.Kind.Rarity.ToString(), car.Kind.Rarity));
    }
    public static UIElement Price(float price, bool withSign = false)
    {
        return new UIElement
        {
            Type = UIElementType.Text,
            Text = withSign? TextConventionsHelper.FormatPriceWithSign(price) : TextConventionsHelper.FormatPrice(price),
            Style = "price"
        };
    }
    public static UIElement Description(string description)
    {
        return new UIElement
        {
            Type = UIElementType.Text,
            Text = $"{description}",
            Style = "description"
        };
    }

    public static UIElement Hint(string text)
    {
        return new UIElement
        {
            Type = UIElementType.Text,
            Text = $"{text}",
            Style = "hint"
        };
    }

    public static UIElement Image(Sprite iconSprite)
    {
        return new UIElement
        {
            Type = UIElementType.Image,
            Image = iconSprite,
        };
    }

    public static UIElement CancelButton()
    {
        return new UIElement
        {
            Type = UIElementType.Button,
            Text = "Cancel",
            OnClick = () =>
            {
                GameEvents.Instance.OnPlayerCancel(new PlayerActionEventData(PlayerOutcome.Canceled, null));
            },
        };
    }


    // Warehouse
    public static List<UIElement> WarehouseBaseInfoElements(Warehouse warehouse)
    {
        return new List<UIElement>()
        {
            Header(warehouse.Name),
            Image(warehouse.Config.image),
            WarehouseStats(warehouse),
        };
    }

    public static List<UIElement> WarehousePurchaseElements(WarehouseOffer offer)
    {
        return new List<UIElement>()
        {
            Price(offer.Price),
            new UIElement
            {
                Type = UIElementType.Button,
                Text = "Purchase",
                IsInteractable = offer.CanAccept(),
                OnClick = () =>
                {
                    var transaction = offer.Accept();
                    var result = G.TransactionProcessor.Process(transaction);
                },
                closePopupOnClick = true,
                UnavailabilityReason = "Cannot afford"
            }
        };
    }

    public static UIElement WarehouseStats(Warehouse warehouse)
    {
        return new UIElement
        {
            Type = UIElementType.Text,
            Text = $"\n - Size: {warehouse.SizeCategory}\n - Location: {warehouse.DistrictName}\n - Parking lots: {warehouse.AvailableCarParkingSpots}/{warehouse.OverallCarParkingSpots}",
            Style = "description"
        };
    }


    //Car
    public static List<UIElement> CarBaseInfoElements(Car car)
    {
        return new List<UIElement>()
        {
            Header(car.Name),
            CarIcon(car),
            CarRarityText(car),
            CarPrice(car),
            //CarStats(car)
        };
    }

    // TODO Currently not working properly - transaction should be created in the context of the car sell menu
    // TODO Make use of this in the car sell context menu
    public static List<UIElement> SellCarOfferElements(CarSellOffer offer)
    {
        return new List<UIElement>()
        {
            new UIElement
            {
                Type = UIElementType.Text,
                Text = $"sell for: {TextConventionsHelper.FormatPrice(offer.InitialOfferPrice)}",
                Style = "price"
            },
            new UIElement
            {
                Type = UIElementType.Button,
                Text = "Sell Car",
                IsInteractable = offer.CanAccept(),
                OnClick = () =>
                {
                    var transaction = offer.Accept();
                    var result = G.TransactionProcessor.Process(transaction);
                },
                closePopupOnClick = true,
                UnavailabilityReason = "Cannot sell car"
            }
        };

    }

    public static UIElement CarIcon(Car car)
    {
        return Image(IconBuilderHelper.BuildCarSprite(car));
    }

    public static UIElement CarStats(Car car)
    {
        // placeholder
        return new UIElement
        {
            Type = UIElementType.Text,
            Text = "Placeholder stats:\n- Max Speed: 180\n- Acceleration: 5.0\n- Handling: 80",
            Style = "description"
        };
    }

    

    public static UIElement CarPrice(Car car)
    {
        return Price(G.Economy.ProductPriceCalculator.Calculate(car));
    }

    // Mission
    public static UIElement MissionLauncherTrigger(MissionLauncher launcher)
    {
        var mission = launcher.MissionRuntime;
        return new UIElement
        {
            Type = UIElementType.Container,
            Children = new List<UIElement>()
            {
                Header(mission.Config.MissionId),
                Description(launcher.Config.ctxDescription),
                new UIElement
                {
                    Type = UIElementType.Button,
                    Text = "Start",
                    // TODO check that other conditions are also met
                    IsInteractable = mission.Status == MissionStatus.Available,
                    OnClick = () =>
                    {
                        GameEvents.Instance.OnPlayerAccept(new PlayerAcceptedEventData(mission));
                    },
                    closePopupOnClick = true,
                    UnavailabilityReason = "Mission cannot be started"
                }
            }
        };     
    }

    public static UIElement MissionLauncherHint(MissionLauncher launcher)
    {
        var mission = launcher.MissionRuntime;
        return new UIElement
        {
            Type = UIElementType.Container,
            Children = new List<UIElement>()
            {
                Header(mission.Config.MissionId),
                Description(launcher.Config.ctxDescription),
                Hint("Get here, to start the mission")
            }
        };
    }

    public static UIElement MissionCompletedInfo(MissionRuntime mission)
    {
        var rewardsText = "";
        foreach(var rewardBundle in mission.RewardBundles)
        {
            rewardsText += $"- {rewardBundle.GetRewardDescription()}\n";
        }

        return new UIElement
        {
            Type = UIElementType.Container,
            Children = new List<UIElement>()
            {
                Header($"Mission {mission.Config.MissionId} Completed!"),
                Description("Rewards:\n" + rewardsText),
                new UIElement
                {
                    Type = UIElementType.Button,
                    Text = "Great!",
                    OnClick = () =>
                    {
                    },
                    closePopupOnClick = true
                }
            }
        };
    }

    internal static UIElement MissionFailedInfo(MissionRuntime mission)
    {
        var failureReasonsText = "";
        foreach (var failureCondition in mission.failureConditions)
        {
            Debug.Log($"Checking failure condition: {failureCondition.GetType().Name}, IsSatisfied: {failureCondition.IsSatisfied()}");
            if (failureCondition.IsSatisfied())
            {
                if (failureCondition is IExplainable expl)
                {
                    failureReasonsText += $"- {expl.GetExplanation()}\n";
                }
                else
                {
                    Debug.LogWarning($"Mission failure condition {failureCondition.GetType().Name} does not implement IExplainable, cannot get explanation text.");
                }
            }
        }
        return new UIElement
        {
            Type = UIElementType.Container,
            Children = new List<UIElement>()
            {
                Header($"Mission {mission.Config.MissionId} Failed!"),
                Description("Failure reasons:\n" + failureReasonsText),
                new UIElement
                {
                    Type = UIElementType.Button,
                    Text = "Close",
                    OnClick = () =>
                    {
                    },
                    closePopupOnClick = true
                }
            }
        };
    }

    // Confirmation Data

    public static UIElement PutCarInsideWarehoseConfirmation(PutCarInWarehouseOffer offer, Action onAccept, Action onCancel)
    {
        return new UIElement { 
            Type = UIElementType.Container, Children = new List<UIElement>() {
                Description("Do you want to put your car inside warehouse?"),
                new UIElement { 
                    Type = UIElementType.Button, Text = "Yes",
                    OnClick = onAccept, closePopupOnClick = true }, 
                new UIElement { 
                    Type = UIElementType.Button, Text = "No",
                    OnClick = onCancel, closePopupOnClick = true }, }, 
        };
    }

    public static UIElement StripCarInsideWarehoseConfirmation(WarehouseStripCarOffer offer, Action onAccept, Action onCancel)
    {
        return new UIElement
        {
            Type = UIElementType.Container,
            Children = new List<UIElement>() {
                Description("Do you want to strip this car in your warehouse?\n"),
                new UIElement {
                    Type = UIElementType.Button, Text = "Yes",
                    OnClick = onAccept, closePopupOnClick = true },
                new UIElement {
                    Type = UIElementType.Button, Text = "No",
                    OnClick = onCancel, closePopupOnClick = true }, },
        };
    }

    public static UIElement EnterWarehouseOnFoot(Warehouse warehouse, Action onAccept, Action onCancel)
    {
        return new UIElement
        {
            Type = UIElementType.Container,
            Children = WarehouseBaseInfoElements(warehouse).Concat(new List<UIElement>() {
                Description("Do you want to enter?"),
                new UIElement {
                    Type = UIElementType.Button, Text = "Yes",
                    OnClick = onAccept, closePopupOnClick = true },
                new UIElement {
                    Type = UIElementType.Button, Text = "No",
                    OnClick = onCancel, closePopupOnClick = true }, }).ToList(),
        };
    }

    public static UIElement StipReslutsClaim(IReadOnlyList<Product> strippedProducts)
    {
        var children = new List<UIElement>()
        {
            Header("Scavaged parts:"),
        };

        foreach (var product in strippedProducts)
        {
            children.Add(Image(IconBuilderHelper.BuildProdutSpite(product)));
        }

        children.Add(new UIElement()
        {
            Type = UIElementType.Button,
            Text = "Claim",
            closePopupOnClick = true,
            OnClick = () => { }

        });

        return new UIElement
        {
            Type = UIElementType.Container,
            Children = children
        };
    }
}