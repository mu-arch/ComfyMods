using UnityEngine;
using UnityEngine.UI;

namespace ComfyLib {
  public static class BehaviourExtensions {
    public static T SetEnabled<T>(this T behaviour, bool enabled) where T: Behaviour {
      behaviour.enabled = enabled;
      return behaviour;
    }
  }

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

    public static Image SetFillAmount(this Image image, float amount) {
      image.fillAmount = amount;
      return image;
    }

    public static Image SetFillCenter(this Image image, bool fillCenter) {
      image.fillCenter = fillCenter;
      return image;
    }

    public static Image SetFillMethod(this Image image, Image.FillMethod fillMethod) {
      image.fillMethod = fillMethod;
      return image;
    }

    public static Image SetFillOrigin(this Image image, Image.OriginHorizontal origin) {
      image.fillOrigin = (int) origin;
      return image;
    }

    public static Image SetFillOrigin(this Image image, Image.OriginVertical origin) {
      image.fillOrigin = (int) origin;
      return image;
    }

    public static Image SetMaskable(this Image image, bool maskable) {
      image.maskable = maskable;
      return image;
    }

    public static Image SetMaterial(this Image image, Material material) {
      image.material = material;
      return image;
    }

    public static Image SetPreserveAspect(this Image image, bool preserveAspect) {
      image.preserveAspect = preserveAspect;
      return image;
    }

    public static Image SetRaycastTarget(this Image image, bool raycastTarget) {
      image.raycastTarget = raycastTarget;
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
  }

  public static class OutlineExtensions {
    public static Outline SetEffectColor(this Outline outline, Color effectColor) {
      outline.effectColor = effectColor;
      return outline;
    }

    public static Outline SetEffectDistance(this Outline outline, Vector2 effectDistance) {
      outline.effectDistance = effectDistance;
      return outline;
    }

    public static Outline SetUseGraphicAlpha(this Outline outline, bool useGraphicAlpha) {
      outline.useGraphicAlpha = useGraphicAlpha;
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

  public static class ShadowExtensions {
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

    public static Text SetResizeTextForBestFit(this Text text, bool resizeTextForBestFit) {
      text.resizeTextForBestFit = resizeTextForBestFit;
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
  }
}
