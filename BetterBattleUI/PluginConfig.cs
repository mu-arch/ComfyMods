using BepInEx.Configuration;

using ComfyLib;

using UnityEngine;

namespace BetterBattleUI {
  public static class PluginConfig {
    public static ConfigEntry<bool> IsModEnabled { get; private set; }

    public static void BindConfig(ConfigFile config) {
      IsModEnabled = config.BindInOrder("_Global", "isModEnabled", true, "Globally enable or disable this mod.");

      BindDamageTextConfig(config);
    }

    public static ConfigEntry<float> DamageTextPopupDuration { get; private set; }
    public static ConfigEntry<Vector3> DamageTextPopupLerpPosition { get; private set; }

    public static ConfigEntry<float> DamageTextMaxPopupDistance { get; private set; }
    public static ConfigEntry<float> DamageTextSmallPopupDistance { get; private set; }

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

    public static void BindDamageTextConfig(ConfigFile config) {
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
              "Maximum distance to popup DamageText messages.",
              new AcceptableValueRange<float>(0f, 30f));

      DamageTextSmallPopupDistance =
          config.BindInOrder(
              "DamageText.Popup",
              "smallPopupDistance",
              10f,
              "Distance to popup small (far-away) DamageText messages.",
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
    }
  }
}
