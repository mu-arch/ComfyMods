using System;
using System.Collections.Generic;

namespace Chatter {
  public static class ConfigEntryExtensions {
    public static void OnSettingChanged<T>(
        this BepInEx.Configuration.ConfigEntry<T> configEntry, Action settingChangedHandler) {
      configEntry.SettingChanged += (_, _) => settingChangedHandler();
    }

    public static void OnSettingChanged<T>(
        this BepInEx.Configuration.ConfigEntry<T> configEntry, Action<T> settingChangedHandler) {
      configEntry.SettingChanged += (_, _) => settingChangedHandler(configEntry.Value);
    }
  }

  public static class ListExtensions {
    public static List<T> Add<T>(this List<T> list, params T[] items) {
      list.AddRange(items);
      return list;
    }
  }
}
