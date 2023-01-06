using System;
using System.Collections.Generic;
using System.Reflection.Emit;

using HarmonyLib;

namespace Atlas {
  [HarmonyPatch(typeof(ZoneSystem))]
  static class ZoneSystemPatch {
    [HarmonyPrefix]
    [HarmonyPatch(nameof(ZoneSystem.GenerateLocationsIfNeeded))]
    static bool GenerateLocationsIfNeededPrefix() {
      ZLog.Log($"Skipping call to GenerateLocationsIfNeeded...");
      return false;
    }

    [HarmonyTranspiler]
    [HarmonyPatch(nameof(ZoneSystem.Load))]
    static IEnumerable<CodeInstruction> LoadTranspiler(IEnumerable<CodeInstruction> instructions) {
      return new CodeMatcher(instructions)
          .MatchForward(
              useEnd: false,
              new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(ZLog), nameof(ZLog.Log))),
              new CodeMatch(OpCodes.Ldloc_3),
              new CodeMatch(OpCodes.Ldarg_0),
              new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(ZoneSystem), nameof(ZoneSystem.m_pgwVersion))))
          .Advance(offset: 2)
          .InsertAndAdvance(
              new CodeInstruction(OpCodes.Ldarg_0),
              Transpilers.EmitDelegate<Func<int, ZoneSystem, int>>(CheckPgwVersionDelegate))
          .MatchForward(
              useEnd: false,
              new CodeMatch(OpCodes.Ldloc_S),
              new CodeMatch(OpCodes.Ldarg_0),
              new CodeMatch(
                  OpCodes.Ldfld, AccessTools.Field(typeof(ZoneSystem), nameof(ZoneSystem.m_locationVersion))))
          .Advance(offset: 1)
          .InsertAndAdvance(
              new CodeInstruction(OpCodes.Ldarg_0),
              Transpilers.EmitDelegate<Func<int, ZoneSystem, int>>(CheckLocationVersionDelegate))
          .InstructionEnumeration();
    }

    static int CheckPgwVersionDelegate(int pgwVersion, ZoneSystem zoneSystem) {
      ZLog.Log($"File pgwVersion is: {pgwVersion}, override to: {zoneSystem.m_pgwVersion}");
      return zoneSystem.m_pgwVersion;
    }

    static int CheckLocationVersionDelegate(int locationVersion, ZoneSystem zoneSystem) {
      ZLog.Log($"File locationVersion is: {locationVersion}, override to: {zoneSystem.m_locationVersion}");
      return zoneSystem.m_locationVersion;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(ZoneSystem.Load))]
    static void LoadPostfix(ref ZoneSystem __instance) {
      ZLog.Log($"Verify locationInstances count: {__instance.m_locationInstances.Count}");
    }
  }
}
