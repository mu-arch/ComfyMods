using BepInEx.Configuration;

using ComfyLib;

using UnityEngine;

namespace Dramamist {
  public static class PluginConfig {
    public static ConfigFile Config { get; private set; }
    public static ConfigEntry<bool> IsModEnabled { get; private set; }

    public static void BindConfig(ConfigFile config) {
      Config = config;
      IsModEnabled = config.BindInOrder("_Global", "isModEnabled", true, "Globally enable or disable this mod.");
    }

    static bool _isParticleMistConfigBound = false;

    public static ConfigEntry<float> UpdateCombinedMovementUpperClamp { get; private set; }

    public static ConfigEntry<float> MainDuration { get; private set; }
    public static ConfigEntry<float> MainStartLifetime { get; private set; }

    public static ConfigEntry<bool> VelocityOverLifetimeEnabled { get; private set; }
    public static ConfigEntry<float> VelocityOverLifetimeSpeedModiferMultiplier { get; private set; }
    public static ConfigEntry<float> VelocityOverLifetimeRadialMultiplier { get; private set; }
    public static ConfigEntry<float> VelocityOverLifetimeXMultiplier { get; private set; }
    public static ConfigEntry<float> VelocityOverLifetimeYMultiplier { get; private set; }
    public static ConfigEntry<float> VelocityOverLifetimeZMultiplier { get; private set; }

    public static ConfigEntry<bool> RotationOverLifetimeEnabled { get; private set; }
    public static ConfigEntry<float> RotationOverLifetimeXMultiplier { get; private set; }
    public static ConfigEntry<float> RotationOverLifetimeYMultiplier { get; private set; }
    public static ConfigEntry<float> RotationOverLifetimeZMultiplier { get; private set; }

    public static ConfigEntry<bool> ColorOverLifetimeEnabled { get; private set; }
    public static ConfigEntry<Color> ColorOverLifetimeColor { get; private set; }

    public static ConfigEntry<string> DemisterBallPrefab { get; private set; }
    public static ConfigEntry<bool> DemisterBallLockPosition { get; private set; }
    public static ConfigEntry<Vector3> DemisterBallLockOffset { get; private set; }
    public static ConfigEntry<float> DemisterForceFieldGravity { get; private set; }
    public static ConfigEntry<Vector3> DemisterForceFieldDirection { get; private set; }

    public static void BindParticleMistConfig(ref ParticleMist particleMist) {
      if (_isParticleMistConfigBound) {
        return;
      }

      _isParticleMistConfigBound = true;

      ConfigFile config = Config;

      UpdateCombinedMovementUpperClamp =
          config.BindInOrder(
              "Update.CombinedMovement",
              "updateCombinedMovementUpperClamp",
              1000f, // vanilla: 10f
              "ParticleMist.Update() -- upper value clamp on ParticleMist.m_combinedMovement",
              new AcceptableValueRange<float>(0f, 1000f));

      ParticleSystem.MainModule main = particleMist.m_ps.main;

      MainDuration =
          config.BindInOrder(
              "Main",
              "mainDuration",
              main.duration,
              "Main.duration (vanilla: 5f",
              new AcceptableValueRange<float>(5f, 30f));

      MainDuration.SettingChanged += (_, _) => Dramamist.UpdateParticleMistSettings();

      MainStartLifetime =
          config.BindInOrder(
              "Main",
              "mainStartLifetime",
              5f,
              "Main.startLifeTime (vanilla: 5)",
              new AcceptableValueRange<float>(5f, 30f));

      MainStartLifetime.SettingChanged += (_, _) => Dramamist.UpdateParticleMistSettings();

      ParticleSystem.VelocityOverLifetimeModule velocityOverLifetime = particleMist.m_ps.velocityOverLifetime;

      VelocityOverLifetimeEnabled =
          config.BindInOrder(
              "VelocityOverLifetime",
              "velocityOverLifetimeTimeEnabled",
              true, // vanilla: true
              "VelocityOverLifetime.enabled (vanilla: true)");

      VelocityOverLifetimeEnabled.SettingChanged += (_, _) => Dramamist.UpdateParticleMistSettings();

      VelocityOverLifetimeSpeedModiferMultiplier =
          config.BindInOrder(
              "VelocityOverLifetime",
              "velocityOverLifetimeSpeedModiferMultiplier",
              1f, // vanilla: 1f
              "VelocityOverLifetime.speedModifierMultiplier (vanilla: 1)",
              new AcceptableValueRange<float>(-10f, 10f));

      VelocityOverLifetimeSpeedModiferMultiplier.SettingChanged += (_, _) => Dramamist.UpdateParticleMistSettings();

      VelocityOverLifetimeRadialMultiplier =
          config.BindInOrder(
              "VelocityOverLifetime",
              "velocityOverLifetimeRadialMultiplier",
              0f, // vanilla: 0f;
              "VelocityOverLifetime.radialMultiplier (vanilla: 0)",
              new AcceptableValueRange<float>(-10f, 10f));

      VelocityOverLifetimeRadialMultiplier.SettingChanged += (_, _) => Dramamist.UpdateParticleMistSettings();

      VelocityOverLifetimeXMultiplier =
          config.BindInOrder(
              "VelocityOverLifetime",
              "velocityOverLifetimeXMultiplier",
              0f, // vanilla: -0.06528816f
              "VelocityOverLifetime.xMultiplier (vanilla: -0.06528816)",
              new AcceptableValueRange<float>(-10f, 10f));

      VelocityOverLifetimeXMultiplier.SettingChanged += (_, _) => Dramamist.UpdateParticleMistSettings();

      VelocityOverLifetimeYMultiplier =
          config.BindInOrder(
              "VelocityOverLifetime",
              "velocityOverLifetimeYMultiplier",
              0f, // vanilla: 0f
              "VelocityOverLifetime.yMultiplier (vanilla: 0)",
              new AcceptableValueRange<float>(-10f, 10f));

      VelocityOverLifetimeYMultiplier.SettingChanged += (_, _) => Dramamist.UpdateParticleMistSettings();

      VelocityOverLifetimeZMultiplier =
          config.BindInOrder(
              "VelocityOverLifetime",
              "velocityOverLifetimeZMultiplier",
              0f, // vanilla: -0.3459233f
              "VelocityOverLifetime.zMultiplier (vanilla: -0.3459233)",
              new AcceptableValueRange<float>(-10f, 10f));

      VelocityOverLifetimeZMultiplier.SettingChanged += (_, _) => Dramamist.UpdateParticleMistSettings();

      // RotationOverLifetime

      ParticleSystem.RotationOverLifetimeModule rotationOverLifetime = particleMist.m_ps.rotationOverLifetime;

      RotationOverLifetimeEnabled =
          config.BindInOrder(
              "RotationOverLifetime",
              "rotationOverLifetimeEnabled",
              true, // vanilla: true
              "RotationOverLifetime.enabled (vanilla: true)");

      RotationOverLifetimeEnabled.SettingChanged += (_, _) => Dramamist.UpdateParticleMistSettings();

      RotationOverLifetimeXMultiplier =
          config.BindInOrder(
              "RotationOverLifetime",
              "rotationOverLifetimeXMultiplier",
              0f, // vanilla: 0f
              "RotationOverLifetime.xMultiplier (vanilla: 0)",
              new AcceptableValueRange<float>(-10f, 10f));

      RotationOverLifetimeXMultiplier.SettingChanged += (_, _) => Dramamist.UpdateParticleMistSettings();

      RotationOverLifetimeYMultiplier =
          config.BindInOrder(
              "RotationOverLifetime",
              "rotationOverLifetimeYMultiplier",
              0f, // vanilla: 0f
              "RotationOverLifetime.yMultiplier (vanilla: 0)",
              new AcceptableValueRange<float>(-10f, 10f));

      RotationOverLifetimeYMultiplier.SettingChanged += (_, _) => Dramamist.UpdateParticleMistSettings();

      RotationOverLifetimeZMultiplier =
          config.BindInOrder(
              "RotationOverLifetime",
              "rotationOverLifetimeZMultiplier",
              0f, // vanilla: 0.08726646f
              "RotationOverLifetime.zMultiplier (vanilla: 0.08726646)",
              new AcceptableValueRange<float>(-10f, 10f));

      RotationOverLifetimeZMultiplier.SettingChanged += (_, _) => Dramamist.UpdateParticleMistSettings();

      // ColorOverLifetime
      //ParticleSystem.ColorOverLifetimeModule colorOverLifetime = particleMist.m_ps.colorOverLifetime;

      //ColorOverLifetimeEnabled =
      //    config.BindInOrder(
      //        "ColorOverLifetime",
      //        "colorOverLifetimeEnabled",
      //        false,
      //        "ColorOverLifetime.enabled (vanilla: ???)");

      //ColorOverLifetimeEnabled.SettingChanged += (_, _) => Dramamist.UpdateParticleMistSettings();

      //ColorOverLifetimeColor =
      //    config.BindInOrder(
      //        "ColorOverLifetime",
      //        "colorOverLifetimeColor",
      //        colorOverLifetime.color.color,
      //        "ColorOverLifetime.color (vanilla: ???)");

      //ColorOverLifetimeColor.SettingChanged += (_, _) => Dramamist.UpdateParticleMistSettings();

      // DemisterBall
      DemisterBallPrefab =
          config.BindInOrder(
              "DemisterBall",
              "demisterBallPrefab",
              "demister_ball",
              "SE_Demister.m_ballPrefab",
              new AcceptableValueList<string>(new string[] { "demister_ball", }));
              //new AcceptableValueList<string>(
              //    ZNetScene.m_instance.m_namedPrefabs.Values
              //        .Where(prefab => prefab.GetComponent<ZSyncTransform>())
              //        .OrderBy(prefab => prefab.name)
              //        .Select(prefab => prefab.name)
              //        .ToArray()));

      DemisterBallLockPosition =
          config.BindInOrder(
              "DemisterBall",
              "demisterBallLockPosition",
              true,
              "SE_Demister.m_ballPrefab.transform.position lock to player head.");

      DemisterBallLockOffset =
          config.BindInOrder(
              "DemisterBall",
              "demisterBallLockOffset",
              new Vector3(-0.1f, 0f, 0f),
              "SE_Demister.m_ballPrefab.transform.position offset when locked to player head.");

      // Demister
      DemisterForceFieldGravity =
          config.BindInOrder(
              "Demister.ForceField",
              "demisterForceFieldGravity",
              -3f,
              "Demister.m_forceField.gravity (vanilla: -0.08)",
              new AcceptableValueRange<float>(-100f, 100f));

      DemisterForceFieldGravity.SettingChanged += (_, _) => Dramamist.UpdateDemisterSettings();

      DemisterForceFieldDirection =
          config.BindInOrder(
              "Demister.ForceField",
              "demisterForceFieldDirection",
              Vector3.zero,
              "Demister.m_forceField.directionXYZ (vanilla: 0, 0, 0)");

      DemisterForceFieldDirection.SettingChanged += (_, _) => Dramamist.UpdateDemisterSettings();
    }
  }
}
