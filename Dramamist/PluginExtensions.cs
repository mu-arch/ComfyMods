namespace Dramamist {
  public static class ComponentExtensions {
    public static bool TryGetComponentInParent<T>(
        this UnityEngine.Component go, out T component) where T : UnityEngine.Component {
      component = go.GetComponentInParent<T>();
      return component;
    }
  }

  public static class ObjectExtensions {
    public static T Ref<T>(this T o) where T : UnityEngine.Object {
      return o ? o : null;
    }
  }
}
