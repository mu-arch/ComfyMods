namespace ComfyLib {
  public static class UnityExtensions {
    public static T Ref<T>(this T gameObject) where T : UnityEngine.Object {
      return gameObject ? gameObject : default;
    }

    public static bool TryGetComponentInParent<T>(
        this UnityEngine.GameObject gameObject, out T component) where T : UnityEngine.Component {
      component = gameObject.GetComponentInParent<T>();
      return component;
    }
  }
}
