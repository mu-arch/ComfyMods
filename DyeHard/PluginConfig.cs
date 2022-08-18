using BepInEx.Configuration;

using System.Linq;

using UnityEngine;

namespace DyeHard {
  public static class PluginConfig {
    public static ConfigFile Config { get; private set; }

    public static ConfigEntry<bool> IsModEnabled { get; private set; }

    public static ConfigEntry<bool> OverridePlayerHairColor { get; private set; }
    public static ConfigEntry<Color> PlayerHairColor { get; private set; }
    public static ConfigEntry<string> PlayerHairColorHex { get; private set; }
    public static ConfigEntry<float> PlayerHairGlow { get; private set; }

    public static ConfigEntry<bool> OverridePlayerHairItem { get; private set; }
    public static ConfigEntry<string> PlayerHairItem { get; private set; }

    public static ConfigEntry<bool> OverridePlayerBeardItem { get; private set; }
    public static ConfigEntry<string> PlayerBeardItem { get; private set; }

    public static ConfigEntry<Vector3> OffsetCharacterPreviewPosition { get; private set; }

    public static void BindConfig(ConfigFile config) {
      Config = config;

      IsModEnabled = config.Bind("_Global", "isModEnabled", true, "Globally enable or disable this mod.");

      OverridePlayerHairColor =
          config.BindInOrder(
              "HairColor", "overridePlayerHairColor", false, "Enable/disable overriding your player's hair color.");

      PlayerHairColor =
          config.BindInOrder("HairColor", "playerHairColor", Color.white, "Sets the color for your player's hair.");

      PlayerHairColorHex =
          config.BindInOrder(
              "HairColor",
              "playerHairColorHex",
              $"#{ColorUtility.ToHtmlStringRGB(Color.white)}",
              "Sets the color of player hair, in HTML hex form (alpha unsupported).");

      PlayerHairGlow =
          config.BindInOrder(
              "HairColor",
              "playerHairGlow",
              1f,
              "Hair glow multiplier for the hair color. Zero removes all color.",
              new AcceptableValueRange<float>(0f, 3f));

      OffsetCharacterPreviewPosition =
          config.BindInOrder(
              "Preview",
              "offsetCharacterPreviewPosition",
              new Vector3(1f, 0f, 1f),
              "Offsets the position of the character preview.");

      IsModEnabled.SettingChanged += (_, _) => DyeHard.SetPlayerZdoHairColor();

      OverridePlayerHairColor.SettingChanged += (_, _) => DyeHard.SetPlayerZdoHairColor();
      PlayerHairColor.SettingChanged += (_, _) => UpdatePlayerHairColorHexValue();
      PlayerHairColorHex.SettingChanged += (_, _) => UpdatePlayerHairColorValue();
      PlayerHairGlow.SettingChanged += (_, _) => DyeHard.SetPlayerZdoHairColor();

      IsModEnabled.SettingChanged += (_, _) => DyeHard.SetCharacterPreviewPosition();
      OffsetCharacterPreviewPosition.SettingChanged += (_, _) => DyeHard.SetCharacterPreviewPosition();
    }

    static void UpdatePlayerHairColorHexValue() {
      Color color = PlayerHairColor.Value;
      color.a = 1f; // Alpha transparency is unsupported.

      PlayerHairColorHex.Value = $"#{ColorUtility.ToHtmlStringRGB(color)}";
      PlayerHairColor.Value = color;

      DyeHard.SetPlayerZdoHairColor();
    }

    static void UpdatePlayerHairColorValue() {
      if (ColorUtility.TryParseHtmlString(PlayerHairColorHex.Value, out Color color)) {
        color.a = 1f; // Alpha transparency is unsupported.
        PlayerHairColor.Value = color;

        DyeHard.SetPlayerZdoHairColor();
      }
    }

    public static void BindCustomizationConfig(ObjectDB objectDb, PlayerCustomizaton customization) {
      if (OverridePlayerHairItem != null || OverridePlayerBeardItem != null) {
        return;
      }

      string[] hairItems =
          objectDb.GetAllItems(ItemDrop.ItemData.ItemType.Customization, "Hair")
              .Select(item => item.name)
              .AlphanumericSort()
              .ToArray();

      OverridePlayerHairItem =
          Config.BindInOrder(
              "Hair", "overridePlayerHairItem", false, "Enable/disable overriding your player's hair.");

      PlayerHairItem =
          Config.BindInOrder(
              "Hair",
              "playerHairItem",
              customization.m_noHair.name,
              "If non-empty, sets/overrides the player's hair (if any).",
              new AcceptableValueList<string>(hairItems));

      IsModEnabled.SettingChanged += (_, _) => DyeHard.SetPlayerHairItem();
      OverridePlayerHairItem.SettingChanged += (_, _) => DyeHard.SetPlayerHairItem();
      PlayerHairItem.SettingChanged += (_, _) => DyeHard.SetPlayerHairItem();

      string[] beardItems =
          objectDb.GetAllItems(ItemDrop.ItemData.ItemType.Customization, "Beard")
              .Select(item => item.name)
              .AlphanumericSort()
              .ToArray();

      OverridePlayerBeardItem =
          Config.BindInOrder(
              "Beard", "overridePlayerBeardItem", false, "Enable/disable overriding your player's beard.");

      PlayerBeardItem =
          Config.BindInOrder(
              "Beard",
              "playerBeardItem",
              customization.m_noBeard.name,
              "If non-empty, sets/overrides the player's beard (if any).",
              new AcceptableValueList<string>(beardItems));

      IsModEnabled.SettingChanged += (_, _) => DyeHard.SetPlayerBeardItem();
      OverridePlayerBeardItem.SettingChanged += (_, _) => DyeHard.SetPlayerBeardItem();
      PlayerBeardItem.SettingChanged += (_, _) => DyeHard.SetPlayerBeardItem();
    }
  }
}
