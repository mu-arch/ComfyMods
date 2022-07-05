using BepInEx.Configuration;

using UnityEngine;

namespace ZoneScouter {
  public class PluginConfig {
    public static ConfigEntry<bool> IsModEnabled { get; private set; }

    public static ConfigEntry<bool> ShowSectorInfoPanel { get; private set; }
    public static ConfigEntry<Vector2> SectorInfoPanelPosition { get; private set; }
    public static ConfigEntry<float> SectorInfoPanelWidth { get; private set; }
    public static ConfigEntry<Color> SectorInfoPanelBackgroundColor { get; private set; }

    public static ConfigEntry<bool> ShowSectorZdoCountGrid { get; private set; }

    public static ConfigEntry<int> SectorInfoPanelFontSize { get; private set; }

    public static ConfigEntry<Color> PositionValueXTextColor { get; private set; }
    public static ConfigEntry<Color> PositionValueYTextColor { get; private set; }
    public static ConfigEntry<Color> PositionValueZTextColor { get; private set; }

    public static ConfigEntry<bool> ShowSectorBoundaries { get; private set; }
    public static ConfigEntry<Color> SectorBoundaryColor { get; private set; }

    public static void BindConfig(ConfigFile config) {
      IsModEnabled = config.Bind("_Global", "isModEnabled", true, "Globally enable or disable this mod.");

      ShowSectorInfoPanel =
          config.Bind(
              "SectorInfoPanel",
              "showSectorInfoPanel",
              true,
              new ConfigDescription(
                  "Show the SectorInfoPanel on the Hud.",
                  acceptableValues: null,
                  new ConfigurationManagerAttributes { Order = 2 }));

      SectorInfoPanelPosition =
          config.Bind(
              "SectorInfoPanel",
              "sectorInfoPanelPosition",
              new Vector2(0f, -25f),
              new ConfigDescription(
                  "SectorInfoPanel position (relative to pivot/anchors).",
                  acceptableValues: null,
                  new ConfigurationManagerAttributes { Order = 1 }));


      SectorInfoPanelBackgroundColor =
          config.Bind(
              "SectorInfoPanel",
              "sectorInfoPanelBackgroundColor",
              new Color(0f, 0f, 0f, 0.9f),
              new ConfigDescription(
                  "SectorInfoPanel background color.",
                  acceptableValues: null,
                  new ConfigurationManagerAttributes { Order = 0 }));

      ShowSectorZdoCountGrid =
          config.Bind(
              "SectorInfoPanel.ZdoCountGrid",
              "showSectorZdoCountGrid",
              true,
              new ConfigDescription(
                  "Show the SectorZdoCount grid in the SectorInfoPanel.",
                  acceptableValues: null,
                  new ConfigurationManagerAttributes { Order = 3 }));

      SectorInfoPanelFontSize =
          config.Bind("SectorInfoPanel.Font", "sectorInfoPanelFontSize", 16, "SectorInfoPanel font size.");

      PositionValueXTextColor =
          config.Bind(
              "SectorInfoPanel.PositionRow",
              "positionValueXTextColor",
              new Color(1f, 0.878f, 0.51f),
              "SectorInfoPanel.PositionRow.X value text color.");

      PositionValueYTextColor =
          config.Bind(
              "SectorInfoPanel.PositionRow",
              "positionValueYTextColor",
              new Color(0.565f, 0.792f, 0.976f),
              "SectorInfoPanel.PositionRow.Y value text color.");

      PositionValueZTextColor =
          config.Bind(
              "SectorInfoPanel.PositionRow",
              "positionValueZTextColor",
              new Color(0.647f, 0.839f, 0.655f),
              "SectorInfoPanel.PositionRow.Z value text color.");

      ShowSectorBoundaries =
          config.Bind(
              "SectorBoundary",
              "showSectorBoundaries",
              false,
              "Shows sector boundaries using semi-transparent walls at each boundary.");

      SectorBoundaryColor =
          config.Bind(
              "SectorBoundary",
              "sectorBoundaryColor",
              (Color) new Color32(255, 255, 255, 48),
              "Color to use for the sector boundary walls.");
    }
  }

  internal sealed class ConfigurationManagerAttributes {
    public int? Order;
  }
}
