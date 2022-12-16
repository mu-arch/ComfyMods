namespace Dramamist {
  public static class ColliderExtensions {
    public static UnityEngine.Collider SetEnabled(this UnityEngine.Collider collider, bool enabled) {
      collider.enabled = enabled;
      return collider;
    }

    public static UnityEngine.Collider SetIsTrigger(this UnityEngine.Collider collider, bool isTrigger) {
      collider.isTrigger = isTrigger;
      return collider;
    }

    public static UnityEngine.SphereCollider SetRadius(this UnityEngine.SphereCollider collider, float radius) {
      collider.radius = radius;
      return collider;
    }
  }

  public static class ComponentExtensions {
    public static T GetOrAddComponent<T>(this UnityEngine.Component component) where T : UnityEngine.Component {
      return component.TryGetComponent(out T componentOut) ? componentOut : component.gameObject.AddComponent<T>();
    }

    public static bool TryGetComponentInParent<T>(
        this UnityEngine.Component component, out T componentOut) where T : UnityEngine.Component {
      componentOut = component.GetComponentInParent<T>();
      return componentOut;
    }
  }

  public static class ObjectExtensions {
    public static T Ref<T>(this T gameObject) where T : UnityEngine.Object {
      return gameObject ? gameObject : null;
    }
  }
}
