using BepInEx.Configuration;

using ComfyLib;

using UnityEngine;

namespace Shortcuts {
  public class PluginConfig {
    public static ConfigEntry<bool> IsModEnabled { get; private set; } 

    public static ConfigEntry<KeyboardShortcut> ToggleConsoleShortcut { get; private set; } 
    public static ConfigEntry<KeyboardShortcut> ToggleHudShortcut { get; private set; } 
    public static ConfigEntry<KeyboardShortcut> ToggleConnectPanelShortcut { get; private set; } 

    public static ConfigEntry<KeyboardShortcut> TakeScreenshotShortcut { get; private set; } 
    public static ConfigEntry<KeyboardShortcut> ToggleMouseCaptureShortcut { get; private set; } 

    public static ConfigEntry<KeyboardShortcut> ToggleDebugFlyShortcut { get; private set; } 
    public static ConfigEntry<KeyboardShortcut> ToggleDebugNoCostShortcut { get; private set; } 
    public static ConfigEntry<KeyboardShortcut> DebugKillAllShortcut { get; private set; } 
    public static ConfigEntry<KeyboardShortcut> DebugRemoveDropsShortcut { get; private set; } 

    public static ConfigEntry<KeyboardShortcut> HotbarItem1Shortcut { get; private set; } 
    public static ConfigEntry<KeyboardShortcut> HotbarItem2Shortcut { get; private set; } 
    public static ConfigEntry<KeyboardShortcut> HotbarItem3Shortcut { get; private set; } 
    public static ConfigEntry<KeyboardShortcut> HotbarItem4Shortcut { get; private set; } 
    public static ConfigEntry<KeyboardShortcut> HotbarItem5Shortcut { get; private set; } 
    public static ConfigEntry<KeyboardShortcut> HotbarItem6Shortcut { get; private set; } 
    public static ConfigEntry<KeyboardShortcut> HotbarItem7Shortcut { get; private set; } 
    public static ConfigEntry<KeyboardShortcut> HotbarItem8Shortcut { get; private set; }

    public static void BindConfig(ConfigFile config) {
      IsModEnabled =
          config.BindInOrder(
              "_Global", "isModEnabled", true, "Globally enable or disable this mod (restart required).");

      ToggleConsoleShortcut =
          config.BindInOrder(
              "Console",
              "toggleConsoleShortcut",
              new KeyboardShortcut(KeyCode.F5),
              "Shortcut to toggle the Console on/off.");

      ToggleHudShortcut =
          config.BindInOrder(
              "Hud",
              "toggleHudShortcut",
              new KeyboardShortcut(KeyCode.F3, KeyCode.LeftControl),
              "Shortcut to toggle the Hud on/off.");

      ToggleConnectPanelShortcut =
          config.BindInOrder(
              "ConnectPanel",
              "toggleConnectPanelShortcut",
              new KeyboardShortcut(KeyCode.F2),
              "Shortcut to toggle the ConnectPanel on/off.");

      TakeScreenshotShortcut =
          config.BindInOrder(
              "GameCamera",
              "takeScreenshotShortcut",
              new KeyboardShortcut(KeyCode.F11),
              "Shortcut to take a screenshot.");

      ToggleMouseCaptureShortcut =
          config.BindInOrder(
              "GameCamera",
              "toggleMouseCaptureShortcut",
              new KeyboardShortcut(KeyCode.F1, KeyCode.LeftControl),
              "Shortcut to toggle mouse capture from the GameCamera.");

      ToggleDebugFlyShortcut =
          config.BindInOrder(
              "Debugmode",
              "toggleDebugFlyShortcut",
              new KeyboardShortcut(KeyCode.Z),
              "Shortcut to toggle flying when in debugmode.");

      ToggleDebugNoCostShortcut =
          config.BindInOrder(
              "Debugmode",
              "toggleDebugNoCostShortcut",
              new KeyboardShortcut(KeyCode.B),
              "Shortcut to toggle no-cost building when in debugmode.");

      DebugKillAllShortcut =
          config.BindInOrder(
              "Debugmode",
              "debugKillAllShortcut",
              new KeyboardShortcut(KeyCode.None),
              "Shortcut to kill/damage all mobs around player. Unbound by default.");

      DebugRemoveDropsShortcut =
          config.BindInOrder(
              "Debugmode",
              "debugRemoveDropsShortcut",
              new KeyboardShortcut(KeyCode.None),
              "Shortcut to 'removedrops' command. Unbound by default.");

      HotbarItem1Shortcut =
          config.BindInOrder(
              "Hotbar",
              "hotbarItem1Shortcut",
              new KeyboardShortcut(KeyCode.Alpha1),
              "Shortcut for the first slot in the Hotbar.");

      HotbarItem2Shortcut =
          config.BindInOrder(
              "Hotbar",
              "hotbarItem2Shortcut",
              new KeyboardShortcut(KeyCode.Alpha2),
              "Shortcut for the second slot in the Hotbar.");

      HotbarItem3Shortcut =
          config.BindInOrder(
              "Hotbar",
              "hotbarItem3Shortcut",
              new KeyboardShortcut(KeyCode.Alpha3),
              "Shortcut for the third slot in the Hotbar.");

      HotbarItem4Shortcut =
          config.BindInOrder(
              "Hotbar",
              "hotbarItem4Shortcut",
              new KeyboardShortcut(KeyCode.Alpha4),
              "Shortcut for the fourth slot in the Hotbar.");

      HotbarItem5Shortcut =
          config.BindInOrder(
              "Hotbar",
              "hotbarItem5Shortcut",
              new KeyboardShortcut(KeyCode.Alpha5),
              "Shortcut for the fifth slot in the Hotbar.");

      HotbarItem6Shortcut =
          config.BindInOrder(
              "Hotbar",
              "hotbarItem6Shortcut",
              new KeyboardShortcut(KeyCode.Alpha6),
              "Shortcut for the sixth slot in the Hotbar.");

      HotbarItem7Shortcut =
          config.BindInOrder(
              "Hotbar",
              "hotbarItem7Shortcut",
              new KeyboardShortcut(KeyCode.Alpha7),
              "Shortcut for the seventh slot in the Hotbar.");

      HotbarItem8Shortcut =
          config.BindInOrder(
              "Hotbar",
              "hotbarItem8Shortcut",
              new KeyboardShortcut(KeyCode.Alpha8),
              "Shortcut for the eight slot in the Hotbar.");
    }
  }
}
