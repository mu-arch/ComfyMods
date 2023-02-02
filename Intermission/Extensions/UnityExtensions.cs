using UnityEngine;
using UnityEngine.UI;

namespace ComfyLib {
  public static class ComponentExtensions {
    public static T GetOrAddComponent<T>(this Component component) where T : Component {
      return component.TryGetComponent(out T componentOut) ? componentOut : component.gameObject.AddComponent<T>();
    }

    public static T SetActive<T>(this T component, bool active) where T : Component {
      component.gameObject.SetActive(active);
      return component;
    }
  }

  public static class ImageExtensions {
    public static Image SetColor(this Image image, Color color) {
      image.color = color;
      return image;
    }

    public static Image SetPreserveAspect(this Image image, bool preserveAspect) {
      image.preserveAspect = preserveAspect;
      return image;
    }

    public static Image SetSprite(this Image image, Sprite sprite) {
      image.sprite = sprite;
      return image;
    }

    public static Image SetType(this Image image, Image.Type type) {
      image.type = type;
      return image;
    }
  }

  public static class ObjectExtensions {
    public static T Ref<T>(this T unityObject) where T : Object {
      return unityObject ? unityObject : null;
    }

    public static T SetName<T>(this T unityObject, string name) where T : Object {
      unityObject.name = name;
      return unityObject;
    }
  }

  public static class OutlineExtensions {
    public static Outline SetEnabled(this Outline outline, bool enabled) {
      outline.enabled = enabled;
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

    public static RectTransform SetPosition(this RectTransform rectTransform, Vector2 position) {
      rectTransform.anchoredPosition = position;
      return rectTransform;
    }

    public static RectTransform SetSizeDelta(this RectTransform rectTransform, Vector2 sizeDelta) {
      rectTransform.sizeDelta = sizeDelta;
      return rectTransform;
    }
  }

  public static class ShadowExtensions {
    public static Shadow SetEnabled(this Shadow shadow, bool enabled) {
      shadow.enabled = enabled;
      return shadow;
    }

    public static Shadow SetEffectColor(this Shadow shadow, Color effectColor) {
      shadow.effectColor = effectColor;
      return shadow;
    }

    public static Shadow SetEffectDistance(this Shadow shadow, Vector2 effectDistance) {
      shadow.effectDistance = effectDistance;
      return shadow;
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

    public static Text SetFontSize(this Text text, int fontSize) {
      text.fontSize = fontSize;
      return text;
    }

    public static Text SetHorizontalOverflow(this Text text, HorizontalWrapMode wrapMode) {
      text.horizontalOverflow = wrapMode;
      return text;
    }

    public static Text SetText(this Text text, string value) {
      text.text = value;
      return text;
    }
  }
}
