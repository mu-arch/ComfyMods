namespace ComfyLib {
  public static class ObjectExtensions {
    public static T Ref<T>(this T obj) where T : UnityEngine.Object {
      return obj ? obj : default;
    }
  }
}