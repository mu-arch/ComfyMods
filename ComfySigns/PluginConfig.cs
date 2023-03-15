using System;
using System.Collections.Generic;
using System.Linq;

using BepInEx.Configuration;

using ComfyLib;

using HarmonyLib;

using TMPro;

using UnityEngine;

namespace ComfySigns {
  public static class PluginConfig {
    public static ConfigEntry<bool> IsModEnabled { get; private set; }

    public static ConfigEntry<string> SignDefaultTextFont { get; private set; }
    public static ExtendedColorConfigEntry SignDefaultTextColor { get; private set; }

    public static void BindConfig(ConfigFile config) {
      IsModEnabled = config.BindInOrder("_Global", "isModEnabled", true, "Globally enable or disable this mod.");

      _fejdStartupBindConfigQueue.Enqueue(() => BindSignConfig(config));
    }

    static readonly EventHandler _onSignConfigChanged = (_, _) => {
      List<Sign> signs =
          Resources.FindObjectsOfTypeAll<Sign>().Where(sign => sign && sign.m_nview && sign.m_nview.IsValid()).ToList();

      ZLog.Log($"Updating {signs.Count} active signs.");

      TMP_FontAsset font = UIFonts.GetFontAsset(SignDefaultTextFont.Value);
      Color color = SignDefaultTextColor.Value;

      foreach (Sign sign in signs) {
        if (sign && sign.m_textWidget) {
          sign.m_textWidget.font = font;
          sign.m_textWidget.color = color;
        }
      }
    };

    public static void BindSignConfig(ConfigFile config) {
      string[] fontNames =
          Resources.FindObjectsOfTypeAll<Font>()
              .Select(f => f.name)
              .Concat(Resources.FindObjectsOfTypeAll<TMP_FontAsset>().Select(f => f.name))
              .OrderBy(f => f)
              .ToArray();

      SignDefaultTextFont =
          config.BindInOrder(
              "Sign.Text",
              "defaultTextFont",
              "Norse SDF",
              "Sign.m_textWidget.font default value.",
              new AcceptableValueList<string>(fontNames));

      SignDefaultTextFont.SettingChanged += _onSignConfigChanged;

      SignDefaultTextColor =
          new(config,
              "Sign.Text",
              "defaultTextColor",
              Color.white,
              "Sign.m_textWidget.color default value.");

      SignDefaultTextColor.ConfigEntry.SettingChanged += _onSignConfigChanged;
    }

    static readonly Queue<Action> _fejdStartupBindConfigQueue = new();

    [HarmonyPatch(typeof(FejdStartup))]
    static class FejdStartupPatch {
      [HarmonyPostfix]
      [HarmonyPatch(nameof(FejdStartup.Awake))]
      static void AwakePostfix() {
        while (_fejdStartupBindConfigQueue.Count > 0) {
          _fejdStartupBindConfigQueue.Dequeue()?.Invoke();
        }
      }
    }
  }
}
