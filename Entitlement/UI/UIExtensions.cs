using UnityEngine;
using UnityEngine.UI;

namespace ComfyLib {
  public static class CanvasGroupExtensions {
    public static CanvasGroup SetAlpha(this CanvasGroup canvasGroup, float alpha) {
      canvasGroup.alpha = alpha;
      return canvasGroup;
    }

    public static CanvasGroup SetBlocksRaycasts(this CanvasGroup canvasGroup, bool blocksRaycasts) {
      canvasGroup.blocksRaycasts = blocksRaycasts;
      return canvasGroup;
    }
  }

  public static class ComponentExtensions {
    public static T SetName<T>(this T component, string name) where T : Component {
      component.gameObject.name = name;
      return component;
    }
  }

  public static class ObjectExtensions {
    public static T Ref<T>(this T o) where T : Object {
      return o ? o : null;
    }
  }

  public static class RectTransformExtensions {
    public static RectTransform SetAnchorMin(this RectTransform rectTransform, Vector2 anchorMin) {
      rectTransform.anchorMin = anchorMin;
      return rectTransform;
    }

    public static RectTransform SetAnchorMax(this RectTransform rectTransform, Vector2 anchorMax) {
      rectTransform.anchorMax = anchorMax;
      return rectTransform;
    }

    public static RectTransform SetPivot(this RectTransform rectTransform, Vector2 pivot) {
      rectTransform.pivot = pivot;
      return rectTransform;
    }

    public static RectTransform SetPosition(this RectTransform rectTransform, Vector2 position) {
      rectTransform.anchoredPosition = position;
      return rectTransform;
    }

    public static RectTransform SetSizeDelta(this RectTransform rectTransform, Vector2 sizeDelta) {
      rectTransform.sizeDelta = sizeDelta;
      return rectTransform;
    }
  }

  public static class TextExtensions {
    public static Text SetAlignment(this Text text, TextAnchor alignment) {
      text.alignment = alignment;
      return text;
    }

    public static Text SetColor(this Text text, Color color) {
      text.color = color;
      return text;
    }

    public static Text SetFont(this Text text, Font font) {
      text.font = font;
      return text;
    }

    public static Text SetFontStyle(this Text text, FontStyle fontStyle) {
      text.fontStyle = fontStyle;
      return text;
    }

    public static Text SetFontSize(this Text text, int fontSize) {
      text.fontSize = fontSize;
      return text;
    }

    public static Text SetHorizontalOverflow(this Text text, HorizontalWrapMode wrapMode) {
      text.horizontalOverflow = wrapMode;
      return text;
    }

    public static Text SetResizeTextForBestFit(this Text text, bool resizeTextForBestFit) {
      text.resizeTextForBestFit = resizeTextForBestFit;
      return text;
    }

    public static Text SetResizeTextMinSize(this Text text, int minSize) {
      text.resizeTextMinSize = minSize;
      return text;
    }

    public static Text SetResizeTextMaxSize(this Text text, int maxSize) {
      text.resizeTextMaxSize = maxSize;
      return text;
    }

    public static Text SetSupportRichText(this Text text, bool supportRichText) {
      text.supportRichText = supportRichText;
      return text;
    }

    public static Text SetText(this Text text, string value) {
      text.text = value;
      return text;
    }

    public static Text SetVerticalOverflow(this Text text, VerticalWrapMode wrapMode) {
      text.verticalOverflow = wrapMode;
      return text;
    }
  }
}
