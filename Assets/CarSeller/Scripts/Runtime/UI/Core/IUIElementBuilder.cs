using UnityEngine;

public interface IUIElementBuilder<T>
{
    T Build(UIElement content, RectTransform container);
    T Build(Widget widget, Transform parent);
}