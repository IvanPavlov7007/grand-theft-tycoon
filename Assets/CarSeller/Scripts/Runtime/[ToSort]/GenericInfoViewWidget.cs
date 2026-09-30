using TMPro;
using UnityEngine;

public class GenericInfoViewWidget : WidgetView<GenericInfoWidget>
{
    public TMP_Text Header;
    public TMP_Text Text;

    protected override RectTransform childrenContainer => null;

    protected override void Bind(GenericInfoWidget widget)
    {
        Header.text = widget.Header;
        Text.text = widget.Text;
    }
}

public class GenericInfoWidget : BlockingInputWidget
{
    public string Header;
    public string Text;

    public GenericInfoWidget(string header, string text)
    {
        Header = header;
        Text = text;
    }
}