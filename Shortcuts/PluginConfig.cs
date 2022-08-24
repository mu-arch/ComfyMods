using BepInEx.Configuration;

using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace Shortcuts {
  public class PluginConfig {
    public static ConfigEntry<bool> IsModEnabled { get; private set; } = default!;

    public static ConfigEntry<KeyboardShortcut> ToggleConsoleShortcut { get; private set; } = default!;
    public static ConfigEntry<KeyboardShortcut> ToggleHudShortcut { get; private set; } = default!;
    public static ConfigEntry<KeyboardShortcut> ToggleConnectPanelShortcut { get; private set; } = default!;

    public static ConfigEntry<KeyboardShortcut> TakeScreenshotShortcut { get; private set; } = default!;
    public static ConfigEntry<KeyboardShortcut> ToggleMouseCaptureShortcut { get; private set; } = default!;

    public static ConfigEntry<KeyboardShortcut> ToggleDebugFlyShortcut { get; private set; } = default!;
    public static ConfigEntry<KeyboardShortcut> ToggleDebugNoCostShortcut { get; private set; } = default!;
    public static ConfigEntry<KeyboardShortcut> DebugKillAllShortcut { get; private set; } = default!;
    public static ConfigEntry<KeyboardShortcut> DebugRemoveDropsShortcut { get; private set; } = default!;

    public static ConfigEntry<KeyboardShortcut> HotbarItem1Shortcut { get; private set; } = default!;
    public static ConfigEntry<KeyboardShortcut> HotbarItem2Shortcut { get; private set; } = default!;
    public static ConfigEntry<KeyboardShortcut> HotbarItem3Shortcut { get; private set; } = default!;
    public static ConfigEntry<KeyboardShortcut> HotbarItem4Shortcut { get; private set; } = default!;
    public static ConfigEntry<KeyboardShortcut> HotbarItem5Shortcut { get; private set; } = default!;
    public static ConfigEntry<KeyboardShortcut> HotbarItem6Shortcut { get; private set; } = default!;
    public static ConfigEntry<KeyboardShortcut> HotbarItem7Shortcut { get; private set; } = default!;
    public static ConfigEntry<KeyboardShortcut> HotbarItem8Shortcut { get; private set; } = default!;

    public static List<ConfigEntry<KeyboardShortcut>> AllShortcuts { get; } = new();

    public static void BindConfig(ConfigFile config) {
      IsModEnabled =
          config.Bind("_Global", "isModEnabled", true, "Globally enable or disable this mod (restart required).");

      ToggleConsoleShortcut =
          config.Bind(
              "Console",
              "toggleConsoleShortcut",
              new KeyboardShortcut(KeyCode.F5),
              "Shortcut to toggle the Console on/off.");

      ToggleHudShortcut =
          config.Bind(
              "Hud",
              "toggleHudShortcut",
              new KeyboardShortcut(KeyCode.F3, KeyCode.LeftControl),
              "Shortcut to toggle the Hud on/off.");

      ToggleConnectPanelShortcut =
          config.Bind(
              "ConnectPanel",
              "toggleConnectPanelShortcut",
              new KeyboardShortcut(KeyCode.F2),
              "Shortcut to toggle the ConnectPanel on/off.");

      TakeScreenshotShortcut =
          config.Bind(
              "GameCamera",
              "takeScreenshotShortcut",
              new KeyboardShortcut(KeyCode.F11),
              "Shortcut to take a screenshot.");

      ToggleMouseCaptureShortcut =
          config.Bind(
              "GameCamera",
              "toggleMouseCaptureShortcut",
              new KeyboardShortcut(KeyCode.F1, KeyCode.LeftControl),
              "Shortcut to toggle mouse capture from the GameCamera.");

      ToggleDebugFlyShortcut =
          config.Bind(
              "Debugmode",
              "toggleDebugFlyShortcut",
              new KeyboardShortcut(KeyCode.Z),
              "Shortcut to toggle flying when in debugmode.");

      ToggleDebugNoCostShortcut =
          config.Bind(
              "Debugmode",
              "toggleDebugNoCostShortcut",
              new KeyboardShortcut(KeyCode.B),
              "Shortcut to toggle no-cost building when in debugmode.");

      DebugKillAllShortcut =
          config.Bind(
              "Debugmode",
              "debugKillAllShortcut",
              new KeyboardShortcut(KeyCode.None),
              "Shortcut to kill/damage all mobs around player. Unbound by default.");

      DebugRemoveDropsShortcut =
          config.Bind(
              "Debugmode",
              "debugRemoveDropsShortcut",
              new KeyboardShortcut(KeyCode.None),
              "Shortcut to 'removedrops' command. Unbound by default.");

      HotbarItem1Shortcut =
          config.Bind(
              "Hotbar",
              "hotbarItem1Shortcut",
              new KeyboardShortcut(KeyCode.Alpha1),
              "Shortcut for the first slot in the Hotbar.");

      HotbarItem2Shortcut =
          config.Bind(
              "Hotbar",
              "hotbarItem2Shortcut",
              new KeyboardShortcut(KeyCode.Alpha2),
              "Shortcut for the second slot in the Hotbar.");

      HotbarItem3Shortcut =
          config.Bind(
              "Hotbar",
              "hotbarItem3Shortcut",
              new KeyboardShortcut(KeyCode.Alpha3),
              "Shortcut for the third slot in the Hotbar.");

      HotbarItem4Shortcut =
          config.Bind(
              "Hotbar",
              "hotbarItem4Shortcut",
              new KeyboardShortcut(KeyCode.Alpha4),
              "Shortcut for the fourth slot in the Hotbar.");

      HotbarItem5Shortcut =
          config.Bind(
              "Hotbar",
              "hotbarItem5Shortcut",
              new KeyboardShortcut(KeyCode.Alpha5),
              "Shortcut for the fifth slot in the Hotbar.");

      HotbarItem6Shortcut =
          config.Bind(
              "Hotbar",
              "hotbarItem6Shortcut",
              new KeyboardShortcut(KeyCode.Alpha6),
              "Shortcut for the sixth slot in the Hotbar.");

      HotbarItem7Shortcut =
          config.Bind(
              "Hotbar",
              "hotbarItem7Shortcut",
              new KeyboardShortcut(KeyCode.Alpha7),
              "Shortcut for the seventh slot in the Hotbar.");

      HotbarItem8Shortcut =
          config.Bind(
              "Hotbar",
              "hotbarItem8Shortcut",
              new KeyboardShortcut(KeyCode.Alpha8),
              "Shortcut for the eight slot in the Hotbar.");

      foreach (
          ConfigEntry<KeyboardShortcut> configEntry in config.Keys
              .Where(key => config[key].SettingType == typeof(KeyboardShortcut))
              .Select(key => (ok: config.TryGetEntry(key, out ConfigEntry<KeyboardShortcut> value), value))
              .Where(entry => entry.ok)
              .Select(entry => entry.value)) {
        UpdateShortcutKeysCache(configEntry);
        configEntry.SettingChanged += (_, _) => UpdateShortcutKeysCache(configEntry);
      }
    }

    public static readonly Dictionary<KeyboardShortcut, KeyCode[]> ShortcutKeysCache = new();

    static void UpdateShortcutKeysCache(ConfigEntry<KeyboardShortcut> configEntry) {
      ShortcutKeysCache[configEntry.Value] = GetAllKeys(configEntry.Value);
    }

    static KeyCode[] GetAllKeys(KeyboardShortcut shortcut) {
      return new[] { shortcut.MainKey }.Concat(shortcut.Modifiers).ToArray();
    }
  }
}
