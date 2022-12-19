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

    [HarmonyTranspiler]
    [HarmonyPatch(nameof(ParticleMist.MisterEmit))]
    static IEnumerable<CodeInstruction> MisterEmitTranspiler(IEnumerable<CodeInstruction> instructions) {
      return new CodeMatcher(instructions)
          .MatchForward(
              useEnd: false,
              new CodeMatch(OpCodes.Ldloca_S),
              new CodeMatch(OpCodes.Ldloc_S),
              new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(ParticleSystem.EmitParams), "set_velocity")))
          .Advance(offset: 2)
          .InsertAndAdvance(Transpilers.EmitDelegate<Func<Vector3, Vector3>>(EmitParamsVelocityDelegate))
          .InstructionEnumeration();
    }

    static Vector3 EmitParamsVelocityDelegate(Vector3 velocity) {
      if (IsModEnabled.Value && ParticleMistReduceMotion.Value) {
        return Vector3.zero;
      }

      return velocity;
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
      if (!IsModEnabled.Value) {
        return;
      }

      int count = _particleSystem.GetTriggerParticles(ParticleSystemTriggerEventType.Inside, _insideParticles);

      for (int i = 0; i < count; i++) {
        ParticleSystem.Particle particle = _insideParticles[i];
        Color color = particle.startColor;
        color.a *= 0.85f;
        particle.startColor = color;

        _insideParticles[i] = particle;
      }

      _particleSystem.SetTriggerParticles(ParticleSystemTriggerEventType.Inside, _insideParticles);
    }
  }
}
