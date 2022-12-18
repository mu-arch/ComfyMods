using BepInEx.Configuration;

using ComfyLib;

namespace Dramamist {
  public static class PluginConfig {
    public static ConfigFile Config { get; private set; }
    public static ConfigEntry<bool> IsModEnabled { get; private set; }

    public static ConfigEntry<bool> ParticleMistReduceMotion { get; private set; }

    public static ConfigEntry<float> DemisterForceFieldGravity { get; private set; }
    public static ConfigEntry<bool> DemisterTriggerFadeOutParticleMist { get; private set; }

    public static void BindConfig(ConfigFile config) {
      IsModEnabled = config.BindInOrder("_Global", "isModEnabled", true, "Globally enable or disable this mod.");

      ParticleMistReduceMotion =
          config.BindInOrder(
              "ParticleMist",
              "particleMistReduceMotion",
              defaultValue: true,
              "ParticleMist: reduces motion of the ParticleMist by setting Velocity/Rotation multipliers to 0.");

      ParticleMistReduceMotion.SettingChanged += (_, _) => Dramamist.UpdateParticleMistSettings();

      DemisterForceFieldGravity =
          config.BindInOrder(
              "Demister",
              "demisterForceFieldGravity",
              -5f,
              "Demister.m_forceField.gravity (vanilla: -0.08)",
              new AcceptableValueRange<float>(-10f, 0f));

      DemisterForceFieldGravity.SettingChanged += (_, _) => Dramamist.UpdateDemisterSettings();

      DemisterTriggerFadeOutParticleMist =
          config.BindInOrder(
              "Demister",
              "demisterTriggerFadeOutParticleMist",
              defaultValue: true,
              "Demister: add a sphere collider trigger that fades-out ParticleMist inside.");

      DemisterTriggerFadeOutParticleMist.SettingChanged += (_, _) => Dramamist.UpdateParticleMistSettings();
      DemisterTriggerFadeOutParticleMist.SettingChanged += (_, _) => Dramamist.UpdateDemisterSettings();
    }
  }
}
