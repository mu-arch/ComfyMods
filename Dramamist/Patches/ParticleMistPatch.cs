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
      BindParticleMistConfig(ref __instance);

      if (IsModEnabled.Value) {
        Dramamist.UpdateParticleMistSettings();
      }
    }

    [HarmonyTranspiler]
    [HarmonyPatch(nameof(ParticleMist.Update))]
    static IEnumerable<CodeInstruction> UpdateTranspiler(IEnumerable<CodeInstruction> instructions) {
      return new CodeMatcher(instructions)
          .MatchForward(
              useEnd: false,
              new CodeMatch(OpCodes.Ldc_R4, 0f),
              new CodeMatch(OpCodes.Ldc_R4, 10f),
              new CodeMatch(
                  OpCodes.Call,
                  AccessTools.Method(
                      typeof(Mathf), nameof(Mathf.Clamp), new Type[] { typeof(float), typeof(float), typeof(float) })),
              new CodeMatch(OpCodes.Add),
              new CodeMatch(
                  OpCodes.Stfld, AccessTools.Field(typeof(ParticleMist), nameof(ParticleMist.m_combinedMovement))))
          .Advance(offset: 2)
          .InsertAndAdvance(Transpilers.EmitDelegate<Func<float, float>>(CombinedMovementUpperClampDelegate))
          .InstructionEnumeration();
    }

    static float CombinedMovementUpperClampDelegate(float upperClamp) {
      if (IsModEnabled.Value) {
        return UpdateCombinedMovementUpperClamp.Value;
      }

      return upperClamp;
    }
  }
}
