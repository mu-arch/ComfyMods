using UnityEngine;

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

  public static class ObjectExtensions {
    public static T Ref<T>(this T unityObject) where T : Object {
      return unityObject ? unityObject : null;
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
}
