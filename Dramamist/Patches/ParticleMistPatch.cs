using System;
using System.Collections.Generic;
using System.Reflection.Emit;

using HarmonyLib;

using UnityEngine;

using static Dramamist.PluginConfig;

namespace Dramamist {
  [HarmonyPatch(typeof(ParticleMist))]
  static class ParticleMistPatch {
    [HarmonyPostfix]
    [HarmonyPatch(nameof(ParticleMist.Awake))]
    static void Awake(ref ParticleMist __instance) {
      if (IsModEnabled.Value) {
        Dramamist.UpdateParticleMistSettings();
      }

      __instance.m_ps.gameObject.AddComponent<ParticleMistTriggerCallback>();
    }
  }

  public class ParticleMistTriggerCallback : MonoBehaviour {
    private ParticleSystem _particleSystem;
    private readonly List<ParticleSystem.Particle> _insideParticles = new();

    private void OnEnable() {
      _particleSystem = GetComponent<ParticleSystem>();

      ParticleSystem.MainModule main = _particleSystem.main;
      _insideParticles.Capacity = main.maxParticles;
    }

    private void OnParticleTrigger() {
      if (IsModEnabled.Value) {
        UpdateParticles(ParticleSystemTriggerEventType.Inside);
      }
    }

    void UpdateParticles(ParticleSystemTriggerEventType eventType) {
      float multiplier = DemisterTriggerFadeOutMultiplier.Value;
      int count = _particleSystem.GetTriggerParticles(eventType, _insideParticles);

      for (int i = 0; i < count; i++) {
        ParticleSystem.Particle particle = _insideParticles[i];
        Color color = particle.startColor;
        color.a *= multiplier;
        particle.startColor = color;

        _insideParticles[i] = particle;
      }

      _particleSystem.SetTriggerParticles(eventType, _insideParticles);
    }
  }
}
