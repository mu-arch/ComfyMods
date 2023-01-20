using System.Collections.Generic;

namespace ComfyLoadingScreens {
  public static class PluginExtensions {
    public static T RandomElement<T>(this List<T> sourceList) {
      return sourceList[UnityEngine.Random.Range(0, sourceList.Count)];
    }

    public static T Ref<T>(this T o) where T : UnityEngine.Object {
      return o ? o : null;
    }
  }
}
