using TMPro;

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

  public static class OutlineExtensions {
    public static Outline SetEffectColor(this Outline outline, Color color) {
      outline.effectColor = color;
      return outline;
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

  public static class TextMeshProUGUIExtensions {
    public static TextMeshProUGUI SetAlignment(this TextMeshProUGUI text, TextAlignmentOptions alignment) {
      text.alignment = alignment;
      return text;
    }

    public static TextMeshProUGUI SetColor(this TextMeshProUGUI text, Color color) {
      text.color = color;
      return text;
    }

    public static TextMeshProUGUI SetFontSize(this TextMeshProUGUI text, int fontSize) {
      text.fontSize = fontSize;
      return text;
    }
  }
}
