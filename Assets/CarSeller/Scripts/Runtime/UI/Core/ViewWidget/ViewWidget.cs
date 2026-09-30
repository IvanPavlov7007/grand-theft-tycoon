using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class WidgetView : MonoBehaviour
{
    public abstract Type WidgetType { get; }

    public abstract void Bind(Widget widget);
    public void BuildChildren(IUIElementBuilder<RectTransform> builder, Widget widget)
    {
        if(widget.Children != null && widget.Children.Count > 0)
            Debug.Assert(childrenContainer != null, $"Widget {widget} requires building children, but the view {this} does not provide a contrainer");

        foreach (var child in widget.Children)
        {
            builder.Build(child, childrenContainer);
        }
    }

    protected abstract RectTransform childrenContainer { get; }
}

public abstract class WidgetView<TWidget> : WidgetView
    where TWidget : Widget
{
    public override Type WidgetType => typeof(TWidget);

    public override void Bind(Widget widget)
    {
        Bind((TWidget)widget);
    }

    protected abstract void Bind(TWidget widget);
}

public abstract class Widget
{
    public List<Widget> Children = new();
    public bool BlockingInput = false;
}

public abstract class  BlockingInputWidget : Widget
{
    protected BlockingInputWidget()
    {
        BlockingInput = true;
    }
}

public abstract class ButtonWidget : Widget
{
    public Action OnClick;
    public bool CloseParentMenuOnClick;
    public bool IsInteractable;
    public string UnavailabilityReason;

    protected ButtonWidget(Action onClick, bool isInteractable = true, bool closeParentMenuOnClick = false, string unavailabilityReason = "")
    {
        OnClick = onClick;
        CloseParentMenuOnClick = closeParentMenuOnClick;
        IsInteractable = isInteractable;
        UnavailabilityReason = unavailabilityReason;
    }
}