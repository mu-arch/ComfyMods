using BepInEx.Configuration;

using UnityEngine;

namespace ZoneScouter {
  public class PluginConfig {
    public static ConfigEntry<bool> IsModEnabled { get; private set; }

    public static ConfigEntry<bool> ShowSectorBoundaries { get; private set; }
    public static ConfigEntry<Color> SectorBoundaryColor { get; private set; }

    public static void BindConfig(ConfigFile config) {
      IsModEnabled = config.Bind("_Global", "isModEnabled", true, "Globally enable or disable this mod.");

      ShowSectorBoundaries =
          config.Bind(
              "Sector",
              "showSectorBoundaries",
              false,
              "Shows sector boundaries using semi-transparent walls at each boundary.");

      SectorBoundaryColor =
          config.Bind(
              "Sector",
              "sectorBoundaryColor",
              (Color) new Color32(255, 255, 255, 48),
              "Color to use for the sector boundary walls.");
    }
  }
}
