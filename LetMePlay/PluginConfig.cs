using BepInEx.Configuration;

namespace LetMePlay {
  public class PluginConfig {
    public static ConfigEntry<bool> IsModEnabled;
    public static ConfigEntry<bool> DisableWardShieldFlash;
    public static ConfigEntry<bool> DisableCameraSwayWhileSitting;
    public static ConfigEntry<bool> DisableBuildPlacementMarker;

    public static ConfigEntry<bool> DisableWeatherSnowParticles;
    public static ConfigEntry<bool> DisableWeatherAshParticles;

    public static void BindConfig(ConfigFile config) {
      IsModEnabled = config.Bind("_Global", "isModEnabled", true, "Globally enable or disable this mod.");

      DisableWardShieldFlash =
          config.Bind("Effects", "disableWardShieldFlash", false, "Disable wards from flashing their blue shield.");

      DisableCameraSwayWhileSitting =
          config.Bind("Camera", "disableCameraSwayWhileSitting", false, "Disables the camera sway while sitting.");

      DisableBuildPlacementMarker =
          config.Bind(
              "Build",
              "disableBuildPlacementMarker",
              false,
              "Disables the yellow placement marker (and gizmo indicator) when building.");

      DisableWeatherSnowParticles =
          config.Bind(
              "Weather",
              "disableWeatherSnowParticles",
              false,
              "Disables ALL snow particles during snow/snowstorm weather.");

      DisableWeatherAshParticles =
          config.Bind(
              "Weather",
              "disableWeatherAshParticles",
              false,
              "Disables ALL ash particles during ash rain weather.");
    }
  }
}
