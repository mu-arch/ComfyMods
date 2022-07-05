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
    public static ConfigEntry<GridSize> SectorZdoCountGridSize { get; private set; }

    public static ConfigEntry<Color> CellZdoCountBackgroundImageColor { get; private set; }
    public static ConfigEntry<int> CellZdoCountTextFontSize { get; private set; }
    public static ConfigEntry<Color> CellZdoCountTextColor { get; private set; }

    public static ConfigEntry<Color> CellSectorBackgroundImageColor { get; private set; }
    public static ConfigEntry<int> CellSectorTextFontSize { get; private set; }
    public static ConfigEntry<Color> CellSectorTextColor { get; private set; }

    public static ConfigEntry<int> SectorInfoPanelFontSize { get; private set; }

    public static ConfigEntry<Color> PositionValueXTextColor { get; private set; }
    public static ConfigEntry<Color> PositionValueYTextColor { get; private set; }
    public static ConfigEntry<Color> PositionValueZTextColor { get; private set; }

    public static ConfigEntry<bool> ShowSectorBoundaries { get; private set; }
    public static ConfigEntry<Color> SectorBoundaryColor { get; private set; }

    public enum GridSize {
      ThreeByThree,
      FiveByFive
    }

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
              "SectorZdoCountGrid",
              "showSectorZdoCountGrid",
              true,
              new ConfigDescription(
                  "Show the SectorZdoCount grid in the SectorInfo panel.",
                  acceptableValues: null,
                  new ConfigurationManagerAttributes { Order = 4 }));

      SectorZdoCountGridSize =
          config.Bind(
              "SectorZdoCountGrid",
              "sectorZdoCountGridSize",
              GridSize.ThreeByThree,
              new ConfigDescription(
                  "Size of the SectorZdoCount grid.",
                  acceptableValues: null,
                  new ConfigurationManagerAttributes { Order = 3 }));

      CellZdoCountBackgroundImageColor =
          config.Bind(
              "SectorZdoCountGrid",
              "cellZdoCountBackgroundImageColor",
              Color.clear,
              new ConfigDescription(
                  "SectorZdoCountCell.ZdoCount.Background.Image color.",
                  acceptableValues: null,
                  new ConfigurationManagerAttributes { Order = 2 }));

      CellZdoCountTextFontSize =
          config.Bind(
              "SectorZdoCountGrid",
              "cellZdoCountTextFontSize",
              16,
              new ConfigDescription(
                  "SectorZdoCountCell.ZdoCount.Text font size.",
                  acceptableValues: null,
                  new ConfigurationManagerAttributes { Order = 2 }));

      CellZdoCountTextColor =
          config.Bind(
              "SectorZdoCountGrid",
              "cellZdoCountTextColor",
              Color.white,
              new ConfigDescription(
                  "SectorZdoCountCell.ZdoCount.Text color.",
                  acceptableValues: null,
                  new ConfigurationManagerAttributes { Order = 2 }));

      CellSectorBackgroundImageColor =
          config.Bind(
              "SectorZdoCountGrid",
              "cellSectorBackgroundImageColor",
              new Color(0.5f, 0.5f, 0.5f, 0.5f),
              new ConfigDescription(
                  "SectorZdoCountCell.Sector.Background.Image color.",
                  acceptableValues: null,
                  new ConfigurationManagerAttributes { Order = 1 }));

      CellSectorTextFontSize =
          config.Bind(
              "SectorZdoCountGrid",
              "cellSectorTextFontSize",
              16,
              new ConfigDescription(
                  "SectorZdoCountCell.Sector.Text font size.",
                  acceptableValues: null,
                  new ConfigurationManagerAttributes { Order = 1 }));

      CellSectorTextColor =
          config.Bind(
              "SectorZdoCountGrid",
              "cellSectorTextColor",
              new Color(0.9f, 0.9f, 0.9f, 1f),
              new ConfigDescription(
                  "SectorZdoCountCell.Sector.Text color.",
                  acceptableValues: null,
                  new ConfigurationManagerAttributes { Order = 1 }));

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
