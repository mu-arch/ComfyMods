using System;

namespace Chatter {
  public static class ObjectExtensions {
    public static T Ref<T>(this T o) where T : UnityEngine.Object {
      return o ? o : null;
    }

    public static UnityEngine.GameObject SetName(this UnityEngine.GameObject gameObject, string name) {
      gameObject.name = name;
      return gameObject;
    }

    public static UnityEngine.GameObject SetParent(
        this UnityEngine.GameObject gameObject, UnityEngine.Transform transform, bool worldPositionStays = false) {
      gameObject.transform.SetParent(transform, worldPositionStays);
      return gameObject;
    }

    public static void OnSettingChanged<T>(
        this BepInEx.Configuration.ConfigEntry<T> configEntry, EventHandler settingChangedHandler) {
      configEntry.SettingChanged -= settingChangedHandler;
      configEntry.SettingChanged += settingChangedHandler;
    }
  }
}
