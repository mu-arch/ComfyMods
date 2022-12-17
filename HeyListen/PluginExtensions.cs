namespace HeyListen {
  public static class ObjectExtensions {
    public static T Ref<T>(this T gameObject) where T : UnityEngine.Object {
      return gameObject ? gameObject : null;
    }
  }
}
