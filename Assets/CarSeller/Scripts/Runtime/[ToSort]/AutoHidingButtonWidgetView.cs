using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AutoHidingButtonWidgetView : ButtonView<AutoHidingButtonWidget>
{
    public TMP_Text ButtonText;
    public Image ButtonImage;

    protected override RectTransform childrenContainer => null;

    protected override void Awake()
    {
        base.Awake();
        if(ButtonImage == null)
        {
            ButtonImage = GetComponent<Image>();
        }
    }

    protected override void Bind(AutoHidingButtonWidget widget)
    {
        base.Bind(widget);
        ButtonText.text = widget.ButtonText;
        if (widget.OnClick == null)
        {
            gameObject.SetActive(false);
        }

        if(widget.ButtonColor.HasValue)
        {
            ButtonImage.color = widget.ButtonColor.Value;
        }

    }
}

public class AutoHidingButtonWidget : ButtonWidget
{
    public string ButtonText;
    public Color? ButtonColor;
    public AutoHidingButtonWidget(string ButtonText, Action onClick, Color? buttonColor = null, bool isInteractable = true, bool closeParentMenuOnClick = false, string unavailabilityReason = "") : base(onClick, isInteractable, closeParentMenuOnClick, unavailabilityReason)
    {
        this.ButtonText = ButtonText;
        this.ButtonColor = buttonColor;
    }

    public AutoHidingButtonWidget(Action onClick, bool isInteractable = true, bool closeParentMenuOnClick = false, string unavailabilityReason = "") : base(onClick, isInteractable, closeParentMenuOnClick, unavailabilityReason)
    {
    }
}