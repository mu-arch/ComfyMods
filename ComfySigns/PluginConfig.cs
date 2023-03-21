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

    public static void BindConfig(ConfigFile config) {
      IsModEnabled = config.BindInOrder("_Global", "isModEnabled", true, "Globally enable or disable this mod.");

      _fejdStartupBindConfigQueue.Enqueue(() => BindLoggingConfig(config));
      _fejdStartupBindConfigQueue.Enqueue(() => BindSignConfig(config));
      _fejdStartupBindConfigQueue.Enqueue(() => BindSignEffectConfig(config));
    }

    public static ConfigEntry<bool> SuppressUnicodeNotFoundWarning { get; private set; }

    public static void BindLoggingConfig(ConfigFile config) {
      SuppressUnicodeNotFoundWarning =
          config.BindInOrder(
              "Logging",
              "suppressUnicodeNotFoundWarning",
              true,
              "Hide 'The character with Unicode value ... was not found...' log warnings.");

      SuppressUnicodeNotFoundWarning.SettingChanged +=
          (_, _) => TMP_Settings.instance.m_warningsDisabled = SuppressUnicodeNotFoundWarning.Value;

      TMP_Settings.instance.m_warningsDisabled = SuppressUnicodeNotFoundWarning.Value;
    }

    public static ConfigEntry<string> SignDefaultTextFont { get; private set; }
    public static ExtendedColorConfigEntry SignDefaultTextColor { get; private set; }

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

      SignDefaultTextFont.SettingChanged += ComfySigns.OnSignConfigChanged;

      SignDefaultTextColor =
          new(config,
              "Sign.Text",
              "defaultTextColor",
              Color.white,
              "Sign.m_textWidget.color default value.");

      SignDefaultTextColor.ConfigEntry.SettingChanged += ComfySigns.OnSignConfigChanged;
    }

    public static ConfigEntry<bool> SignEffectEnablePartyEffect{ get; private set; }

    public static void BindSignEffectConfig(ConfigFile config) {
      SignEffectEnablePartyEffect =
          config.BindInOrder(
              "SignEffect",
              "enablePartyEffect",
              true,
              "Enables the 'Party' Sign effect for signs using the party tag.");

      // Removing any existing Party effect components when this setting is toggled off.
      SignEffectEnablePartyEffect.SettingChanged += (_, _) => {
        if (SignEffectEnablePartyEffect.Value) {
          return;
        }

        foreach (
            Sign sign in
                UnityEngine.Object.FindObjectsOfType<Sign>()
                    .Where(sign => sign && sign.m_nview && sign.m_nview.IsValid())) {
          if (sign.m_textWidget && sign.m_textWidget.TryGetComponent(out VertexColorCycler colorCycler)) {
            UnityEngine.Object.Destroy(colorCycler);
            sign.m_textWidget.ForceMeshUpdate(ignoreActiveState: true);
          }
        }
      };
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
