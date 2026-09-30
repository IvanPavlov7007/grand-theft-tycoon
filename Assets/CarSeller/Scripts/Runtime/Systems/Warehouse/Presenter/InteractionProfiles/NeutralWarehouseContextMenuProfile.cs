using System.Collections.Generic;
using UnityEngine;

public sealed class NeutralWarehouseContextMenuProfile : IWarehouseContextMenuProfile
{
    private readonly ProductContextMenuContentBuilder _productContextMenuContentBuilder = new ProductContextMenuContentBuilder();

    public UIElement GenerateContent(object model, GameState gameState)
    {
        var neutralState = gameState as NeutralGameState;
        if (neutralState == null)
        {
            Debug.LogWarning($"NeutralWarehouseContextMenuProfile used with non-neutral state {gameState?.GetType().Name}");
            return null;
        }

        switch (model)
        {
            case Product product:
                return generateNormalContent(neutralState, product);
            default:
                Debug.LogWarning(
                    $"NeutralWarehouseContextMenuProfile: No context menu available for model type {model.GetType()}");
                return null;
        }
    }

    private UIElement generateNormalContent(NeutralGameState neutralGameState, Product product)
    {
        return product.GetRepresentation(_productContextMenuContentBuilder);
    }

    private class ProductContextMenuContentBuilder : IProductViewBuilder<UIElement>
    {
        public UIElement BuildCar(Car car)
        {
            var elementsList = CTX_Menu_Tools.CarBaseInfoElements(car);
            elementsList.Add(new UIElement
            {
                Type = UIElementType.Button,
                Text = "Sell",
                IsInteractable = car.IsComplete(),
                OnClick = () =>
                {
                    //G.GameFlowManager.SellCar(
                    //    G.Economy.CarSellOfferProvider.GetOfferByCar(car));
                },
                UnavailabilityReason = "Some car part's are missing"
            });
            elementsList.Add(new UIElement
            {
                Type = UIElementType.Button,
                Text = "Disassemble",
                IsInteractable = G.CarMechanicService.CanDisassembleCar(car),
                OnClick = () =>
                {
                    G.CarMechanicService.DisassembleCar(car);
                },
                UnavailabilityReason = "Car has nothing to disassemble"
            });

            return new UIElement
            {
                Type = UIElementType.Container,
                Children = elementsList
            };
        }

        public UIElement BuildCarFrame(CarFrame carFrame)
        {
            var elementsList = new List<UIElement>
            {
                CTX_Menu_Tools.Header(carFrame.Name),
                CTX_Menu_Tools.Image(IconBuilderHelper.BuildCarFrameSprite(carFrame))
            };

            return new UIElement
            {
                Type = UIElementType.Container,
                Children = elementsList
            };
        }

        public UIElement BuildEngine(Engine engine)
        {
            var elementsList = new List<UIElement>
            {
                CTX_Menu_Tools.Header(engine.Name),
                CTX_Menu_Tools.Image(IconBuilderHelper.BuildEngineSprite(engine))
            };

            return new UIElement
            {
                Type = UIElementType.Container,
                Children = elementsList
            };
        }

        public UIElement BuildSpoiler(Spoiler spoiler)
        {
            var elementsList = new List<UIElement>
            {
                CTX_Menu_Tools.Header(spoiler.Name),
                CTX_Menu_Tools.Image(IconBuilderHelper.BuildSpoilerSprite(spoiler))
            };

            return new UIElement
            {
                Type = UIElementType.Container,
                Children = elementsList
            };
        }

        public UIElement BuildWheel(Wheel wheel)
        {
            var elementsList = new List<UIElement>
            {
                CTX_Menu_Tools.Header(wheel.Name),
                CTX_Menu_Tools.Image(IconBuilderHelper.BuildWheelSprite(wheel))
            };

            return new UIElement
            {
                Type = UIElementType.Container,
                Children = elementsList
            };
        }
    }
}