using System.Collections.Generic;

namespace ComfyLoadingScreens {
  public static class PluginExtensions {
    public static T RandomElement<T>(this List<T> sourceList) {
      return sourceList[UnityEngine.Random.Range(0, sourceList.Count)];
    }
  }
}
