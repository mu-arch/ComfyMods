using System.Reflection;

using BepInEx;

using HarmonyLib;

using UnityEngine;

using static Dramamist.PluginConfig;

namespace Dramamist {
  [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
  public class Dramamist : BaseUnityPlugin {
    public const string PluginGuid = "redseiko.valheim.dramamist";
    public const string PluginName = "Dramamist";
    public const string PluginVersion = "1.0.0";

    Harmony _harmony;

    public void Awake() {
      BindConfig(Config);

      _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), harmonyInstanceId: PluginGuid);
    }

    public void OnDestroy() {
      _harmony?.UnpatchSelf();
    }

    public static void UpdateParticleMistSettings() {
      if (!ParticleMist.m_instance) {
        return;
      }

      ParticleMist particleMist = ParticleMist.m_instance;

      ParticleSystem.MainModule main = particleMist.m_ps.main;
      main.duration = MainDuration.Value;
      main.startLifetime = MainStartLifetime.Value;

      ParticleSystem.VelocityOverLifetimeModule velocityOverLifetime = particleMist.m_ps.velocityOverLifetime;
      velocityOverLifetime.enabled = VelocityOverLifetimeEnabled.Value;
      velocityOverLifetime.speedModifierMultiplier = VelocityOverLifetimeSpeedModiferMultiplier.Value;
      velocityOverLifetime.radialMultiplier = VelocityOverLifetimeRadialMultiplier.Value;
      velocityOverLifetime.xMultiplier = VelocityOverLifetimeXMultiplier.Value;
      velocityOverLifetime.yMultiplier = VelocityOverLifetimeYMultiplier.Value;
      velocityOverLifetime.zMultiplier = VelocityOverLifetimeZMultiplier.Value;

      ParticleSystem.RotationOverLifetimeModule rotationOverLifetime = particleMist.m_ps.rotationOverLifetime;
      rotationOverLifetime.enabled = RotationOverLifetimeEnabled.Value;
      rotationOverLifetime.xMultiplier = RotationOverLifetimeXMultiplier.Value;
      rotationOverLifetime.yMultiplier = RotationOverLifetimeYMultiplier.Value;
      rotationOverLifetime.zMultiplier = RotationOverLifetimeZMultiplier.Value;

      //ParticleSystem.ColorOverLifetimeModule colorOverLifetime = particleMist.m_ps.colorOverLifetime;
      //colorOverLifetime.enabled = ColorOverLifetimeEnabled.Value;
      //colorOverLifetime.color = ColorOverLifetimeColor.Value;
    }

    public static void UpdateDemisterSettings() {
      if (Demister.m_instances.Count <= 0) {
        return;
      }

      Vector3 forceFieldDirection = DemisterForceFieldDirection.Value;

      foreach (Demister demister in Demister.m_instances) {
        demister.m_forceField.gravity = DemisterForceFieldGravity.Value;

        demister.m_forceField.directionX = forceFieldDirection.x;
        demister.m_forceField.directionY = forceFieldDirection.y;
        demister.m_forceField.directionZ = forceFieldDirection.z;
      }
    }
  }
}