using UnityEngine;
using UnityEngine.UI;

namespace ComfyLib {
  public static class ComponentExtensions {
    public static Image Image(this Component component) {
      return component.GetComponent<Image>();
    }

    public static RectTransform RectTransform(this Component component) {
      return component.GetComponent<RectTransform>();
    }
  }

  public static class ImageExtensions {
    public static Image SetRaycastTarget(this Image image, bool raycastTarget) {
      image.raycastTarget = raycastTarget;
      return image;
    }
  }

  public static class RectTransformExtensions {
    public static RectTransform SetPosition(this RectTransform rectTransform, Vector2 position) {
      rectTransform.anchoredPosition = position;
      return rectTransform;
    }
  }
}
