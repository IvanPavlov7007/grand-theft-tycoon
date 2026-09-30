using TMPro;
using UnityEngine;

public class VerticalContentWidgetView : WidgetView<VerticalContentWidget>
{
    [SerializeField] TMP_Text header;
    [SerializeField] RectTransform contentRoot;

    protected override RectTransform childrenContainer => contentRoot;

    protected override void Bind(VerticalContentWidget widget)
    {
        header.text = widget.Header;
    }
}

public class VerticalContentWidget : BlockingInputWidget
{
        public readonly string Header;

    public VerticalContentWidget(string header)
    {
        Header = header;
    }
}