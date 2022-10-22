using BepInEx.Configuration;

using ComfyLib;

using System;

using UnityEngine;

namespace BetterBattleUI {
  public static class PluginConfig {
    public static ConfigEntry<bool> IsModEnabled { get; private set; }

    public static void BindConfig(ConfigFile config) {
      IsModEnabled = config.BindInOrder("_Global", "isModEnabled", true, "Globally enable or disable this mod.");

      BindDamageTextConfig(config);
    }

    public static ConfigEntry<float> DamageTextMaxTextDistance { get; private set; }
    public static ConfigEntry<float> DamageTextSmallFontDistance { get; private set; }

    public static ConfigEntry<int> DamageTextSmallFontSize { get; private set; }
    public static ConfigEntry<int> DamageTextLargeFontSize { get; private set; }

    public static ConfigEntry<Color> DamageTextPlayerDamageColor { get; private set; }
    public static ConfigEntry<Color> DamageTextPlayerNoDamageColor { get; private set; }
    public static ConfigEntry<Color> DamageTextNormalColor { get; private set; }
    public static ConfigEntry<Color> DamageTextResistantColor { get; private set; }
    public static ConfigEntry<Color> DamageTextWeakColor { get; private set; }
    public static ConfigEntry<Color> DamageTextImmuneColor { get; private set; }
    public static ConfigEntry<Color> DamageTextHealColor { get; private set; }
    public static ConfigEntry<Color> DamageTextTooHardColor { get; private set; }
    public static ConfigEntry<Color> DamageTextBlockedColor { get; private set; }

    public static ConfigEntry<float> DamageTextMessageDuration { get; private set; }
    public static ConfigEntry<bool> DamageTextFadeOutUseBezier { get; private set; }

    public static void BindDamageTextConfig(ConfigFile config) {
      DamageTextMaxTextDistance =
          config.BindInOrder(
              "DamageText.Distance",
              "maxTextDistance",
              30f,
              "DamageText maximum distance to show any damage messages.",
              new AcceptableValueRange<float>(0f, 30f));

      DamageTextSmallFontDistance =
          config.BindInOrder(
              "DamageText.Distance",
              "smallFontDistance",
              10f,
              "DamageText minimum distance to show small (far-away) damage messages.",
              new AcceptableValueRange<float>(0f, 30f));

      DamageTextSmallFontSize =
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

      DamageTextMessageDuration =
          config.BindInOrder(
              "DamageText.Behaviour",
              "messageDuration",
              1.5f,
              "Duration (in seconds) to show DamageText messages.",
              new AcceptableValueRange<float>(0f, 10f));

      DamageTextFadeOutUseBezier =
          config.BindInOrder(
              "DamageText.FadeOut",
              "useBezier",
              false,
              "If true, fades-out DamageText using Bezier curve instead of vanilla code.");
    }
  }
}
