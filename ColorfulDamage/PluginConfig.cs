using System;
using System.Collections.Generic;
using System.Linq;

using BepInEx.Configuration;

using ComfyLib;

using HarmonyLib;

using UnityEngine;

namespace ColorfulDamage {
  public static class PluginConfig {
    public static ConfigEntry<bool> IsModEnabled { get; private set; }

    public static void BindConfig(ConfigFile config) {
      IsModEnabled = config.BindInOrder("_Global", "isModEnabled", true, "Globally enable or disable this mod.");

      BindDamageTextPopupConfig(config);
      BindDamageTextShadowEffectConfig(config);
      BindDamageTextColorConfig(config);

      _fejdStartupBindConfigQueue.Clear();
      _fejdStartupBindConfigQueue.Enqueue(() => BindDamageTextFontConfig(config));
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

    public static ConfigEntry<float> DamageTextPopupDuration { get; private set; }
    public static ConfigEntry<Vector3> DamageTextPopupLerpPosition { get; private set; }
    public static ConfigEntry<float> DamageTextMaxPopupDistance { get; private set; }
    public static ConfigEntry<float> DamageTextSmallPopupDistance { get; private set; }

    private static void BindDamageTextPopupConfig(ConfigFile config) {
      DamageTextPopupDuration =
          config.BindInOrder(
              "DamageText.Popup",
              "popupDuration",
              1.5f,
              "Duration (in seconds) to show DamageText messages.",
              new AcceptableValueRange<float>(0f, 10f));

      DamageTextPopupLerpPosition =
          config.BindInOrder(
              "DamageText.Popup",
              "popupLerpPosition",
              new Vector3(0f, 1.5f, 0f),
              "Position (Vector3) offset to lerp the DamageText message to.");

      DamageTextMaxPopupDistance =
          config.BindInOrder(
              "DamageText.Popup",
              "maxPopupDistance",
              30f,
              "Maximum distance to popup ANY (small/ or large) DamageText messages.",
              new AcceptableValueRange<float>(0f, 60f));

      DamageTextSmallPopupDistance =
          config.BindInOrder(
              "DamageText.Popup",
              "smallPopupDistance",
              10f,
              "Distance to popup DamageText messages using small (far-away) font size.",
              new AcceptableValueRange<float>(0f, 60f));
    }

    public static ConfigEntry<bool> DamageTextUseShadowEffect { get; private set; }
    public static ConfigEntry<Color> DamageTextShadowEffectColor { get; private set; }
    public static ConfigEntry<Vector2> DamageTextShadowEffectDistance { get; private set; }

    private static void BindDamageTextShadowEffectConfig(ConfigFile config) {
      DamageTextUseShadowEffect =
          config.BindInOrder(
              "DamageText.ShadowEffect",
              "useShadowEffect",
              false,
              "If true, uses a Shadow effect on the DamageText instead of Outline effect.");

      DamageTextShadowEffectColor =
          config.BindInOrder(
              "DamageText.ShadowEffect",
              "shadowEffectColor",
              new Color(0f, 0f, 0f, 0.5f),
              "Color of the Shadow effect to use on the DamageText.");

      DamageTextShadowEffectDistance =
          config.BindInOrder(
              "DamageText.ShadowEffect",
              "shadowEffectDistance",
              new Vector2(1.50f, -1.50f),
              "Distance of the Shadow effect to use on the DamageText.");
    }

    public static ConfigEntry<Color> DamageTextPlayerDamageColor { get; private set; }
    public static ConfigEntry<Color> DamageTextPlayerNoDamageColor { get; private set; }
    public static ConfigEntry<Color> DamageTextNormalColor { get; private set; }
    public static ConfigEntry<Color> DamageTextResistantColor { get; private set; }
    public static ConfigEntry<Color> DamageTextWeakColor { get; private set; }
    public static ConfigEntry<Color> DamageTextImmuneColor { get; private set; }
    public static ConfigEntry<Color> DamageTextHealColor { get; private set; }
    public static ConfigEntry<Color> DamageTextTooHardColor { get; private set; }
    public static ConfigEntry<Color> DamageTextBlockedColor { get; private set; }

    private static void BindDamageTextColorConfig(ConfigFile config) {
      DamageTextPlayerDamageColor =
          config.BindInOrder(
              "DamageText.Color",
              "playerDamageColor",
             Color.red,
              "DamageText.color for damage to player > 0.");

      DamageTextPlayerNoDamageColor =
          config.BindInOrder(
              "DamageText.Color",
              "playerNoDamageColor",
              Color.gray,
              "DamageText.color for damage to player = 0.");

      DamageTextNormalColor =
          config.BindInOrder(
              "DamageText.Color",
              "normalColor",
              Color.white,
              "DamageText.color for TextType.Normal damage.");

      DamageTextResistantColor =
          config.BindInOrder(
              "DamageText.Color",
              "resistantColor",
              new Color(0.6f, 0.6f, 0.6f, 1f),
              "DamageText.color for TextType.Resistant damage.");

      DamageTextWeakColor =
          config.BindInOrder(
              "DamageText.Color",
              "weakColor",
              new Color(1f, 1f, 0f, 1f),
              "DamageText.color for TextType.Weak damage.");

      DamageTextImmuneColor =
          config.BindInOrder(
              "DamageText.Color",
              "immuneColor",
              new Color(0.6f, 0.6f, 0.6f, 1f),
              "DamageText.color for TextType.Immune damage.");

      DamageTextHealColor =
          config.BindInOrder(
              "DamageText.Color",
              "healColor",
              new Color(0.5f, 1f, 0.5f, 0.7f),
              "DamageText.color for TextType.Heal damage.");

      DamageTextTooHardColor =
          config.BindInOrder(
              "DamageText.Color",
              "tooHardColor",
              new Color(0.8f, 0.7f, 0.7f, 1f),
              "DamageText.color for TextType.TooHard damage.");

      DamageTextBlockedColor =
          config.BindInOrder(
              "DamageText.Color",
              "blockedColor",
              Color.white,
              "DamageText.color for TextType.Blocked damage.");
    }

    public static ConfigEntry<string> DamageTextMessageFont { get; private set; }
    public static ConfigEntry<int> DamageTextSmallFontSize { get; private set; }
    public static ConfigEntry<int> DamageTextLargeFontSize { get; private set; }

    private static void BindDamageTextFontConfig(ConfigFile config) {
      string[] fontNames = Resources.FindObjectsOfTypeAll<Font>().Select(f => f.name).OrderBy(f => f).ToArray();

      DamageTextMessageFont =
          config.BindInOrder(
              "DamageText.Font",
              "messageFont",
              "AveriaSerifLibre-Bold",
              "DamageText.font for all damage messages.",
              new AcceptableValueList<string>(fontNames));

      DamageTextSmallFontSize ??=
          config.BindInOrder(
              "DamageText.Font",
              "smallFontSize",
              14,
              "DamageText.fontSize for small (far-away) damage messages.",
              new AcceptableValueRange<int>(0, 32));

      DamageTextLargeFontSize =
          config.BindInOrder(
              "DamageText.Font",
              "largeFontSize",
              18,
              "DamageText.fontSize for large (nearby) damage messages.",
              new AcceptableValueRange<int>(0, 32));
    }
  }
}
