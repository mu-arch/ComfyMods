using System;
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

    static ParticleSystemProfile _particleMistProfile;
    static readonly ParticleSystem.MinMaxCurve _zeroCurve = new(0f);
    static ParticleSystem.MinMaxGradient _flatStartColor;

    public static void UpdateParticleMistSettings() {
      if (!ParticleMist.m_instance) {
        return;
      }

      ParticleMist particleMist = ParticleMist.m_instance;
      ParticleSystem.MainModule main = particleMist.m_ps.main;
      ParticleSystem.TriggerModule trigger = particleMist.m_ps.trigger;
      ParticleSystem.VelocityOverLifetimeModule velocityOverLifetime = particleMist.m_ps.velocityOverLifetime;
      ParticleSystem.RotationOverLifetimeModule rotationOverLifetime = particleMist.m_ps.rotationOverLifetime;

      if (_particleMistProfile == null) {
        _particleMistProfile ??= new(particleMist.m_ps);
        _flatStartColor = new ParticleSystem.MinMaxGradient(main.startColor.colorMax);
      }

      if (IsModEnabled.Value && ParticleMistReduceMotion.Value) {
        main.startRotation = _zeroCurve;
        main.startColor = _flatStartColor;

        velocityOverLifetime.x = _zeroCurve;
        velocityOverLifetime.y = _zeroCurve;
        velocityOverLifetime.z = _zeroCurve;

        rotationOverLifetime.x = _zeroCurve;
        rotationOverLifetime.y = _zeroCurve;
        rotationOverLifetime.z = _zeroCurve;
      } else {
        main.startRotation = _particleMistProfile.StartRotation;
        main.startColor = _particleMistProfile.StartColor;

        velocityOverLifetime.x = _particleMistProfile.VelocityOverLifetimeX;
        velocityOverLifetime.y = _particleMistProfile.VelocityOverLifetimeY;
        velocityOverLifetime.z = _particleMistProfile.VelocityOverLifetimeZ;

        rotationOverLifetime.x = _particleMistProfile.RotationOverLifetimeX;
        rotationOverLifetime.y = _particleMistProfile.RotationOverLifetimeY;
        rotationOverLifetime.z = _particleMistProfile.RotationOverLifetimeZ;
      }

      trigger.enabled = IsModEnabled.Value && DemisterTriggerFadeOutParticleMist.Value;
      trigger.inside = ParticleSystemOverlapAction.Callback;
      trigger.colliderQueryMode = ParticleSystemColliderQueryMode.Disabled;
    }

    public static void UpdateDemisterSettings() {
      if (Demister.m_instances.Count <= 0) {
        return;
      }

      foreach (Demister demister in Demister.m_instances) {
        UpdateDemisterSettings(demister);
      }
    }

    public static void UpdateDemisterSettings(Demister demister) {
      if (demister.gameObject.layer == EffectLayer.Value && IsLocalPlayerDemisterBall(demister)) {
        demister.m_forceField.gravity = DemisterForceFieldGravity.Value;

        demister.m_forceField.GetOrAddComponent<SphereCollider>()
            .SetRadius(demister.m_forceField.endRange)
            .SetIsTrigger(true)
            .SetEnabled(IsModEnabled.Value && DemisterTriggerFadeOutParticleMist.Value);
      }
    }

    public static readonly Lazy<int> EffectLayer = new(() => LayerMask.NameToLayer("effect"));

    public static bool IsLocalPlayerDemisterBall(Demister demister) {
      GameObject ballInstance =
          ((SE_Demister) GetStatusEffect(Player.m_localPlayer, "Demister")).Ref()?.m_ballInstance;

      return ballInstance && ballInstance == demister.transform.root.gameObject;
    }

    public static StatusEffect GetStatusEffect(Player player, string statusEffectName) {
      return Player.m_localPlayer.Ref()?.m_seman.GetStatusEffect(statusEffectName);
    }
  }
}