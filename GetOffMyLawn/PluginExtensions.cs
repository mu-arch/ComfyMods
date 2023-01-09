using HarmonyLib;

using UnityEngine;

namespace GetOffMyLawn {
  public static class ObjectExtensions {
    public static T Ref<T>(this T o) where T : Object {
      return o ? o : null;
    }
  }
}
