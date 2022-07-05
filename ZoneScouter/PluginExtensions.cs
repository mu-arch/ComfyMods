using System;

using UnityEngine;

namespace ZoneScouter {
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

  public static class ObjectExtensions {
    public static T Ref<T>(this T o) where T : UnityEngine.Object {
      return o ? o : null;
    }
  }

  public static class Vector2iExtensions {
    public static Vector2i Left(this Vector2i sector) {
      sector.x--;
      return sector;
    }

    public static Vector2i Right(this Vector2i sector) {
      sector.x++;
      return sector;
    }

    public static Vector2i Up(this Vector2i sector) {
      sector.y++;
      return sector;
    }

    public static Vector2i Down(this Vector2i sector) {
      sector.y--;
      return sector;
    }

    public static Vector2i UpLeft(this Vector2i sector) {
      sector.x--;
      sector.y++;
      return sector;
    }

    public static Vector2i UpRight(this Vector2i sector) {
      sector.x++;
      sector.y++;
      return sector;
    }

    public static Vector2i DownLeft(this Vector2i sector) {
      sector.x--;
      sector.y--;
      return sector;
    }

    public static Vector2i DownRight(this Vector2i sector) {
      sector.x++;
      sector.y--;
      return sector;
    }
  }
}
