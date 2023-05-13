using System;
using System.Collections.Generic;
using System.Reflection.Emit;

using HarmonyLib;

using static BetterServerPortals.BetterServerPortals;
using static BetterServerPortals.PluginConfig;

namespace BetterServerPortals {
  [HarmonyPatch(typeof(ZDOMan))]
  static class ZDOManPatch {
    [HarmonyPrefix]
    [HarmonyPatch(nameof(ZDOMan.Load))]
    static void LoadPrefix() {
      CachedPortalZdos.Clear();
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(ZDOMan.ShutDown))]
    static void ShutDownPrefix() {
      CachedPortalZdos.Clear();
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(ZDOMan.AddToSector))]
    static void AddToSectorPostfix(ZDO zdo) {
      if (zdo != null && PortalPrefabHashCodes.Contains(zdo.m_prefab)) {
        CachedPortalZdos.Add(zdo);
      }
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(ZDOMan.RemoveFromSector))]
    static void RemoveFromSectorPostfix(ZDO zdo) {
      if (zdo != null) {
        CachedPortalZdos.Remove(zdo);
      }
    }

    [HarmonyTranspiler]
    [HarmonyPatch(nameof(ZDOMan.RPC_ZDOData))]
    static IEnumerable<CodeInstruction> RPC_ZDODataTranspiler(IEnumerable<CodeInstruction> instructions) {
      return new CodeMatcher(instructions)
        .MatchForward(
            useEnd: true,
            new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(ZDO), nameof(ZDO.Deserialize))))
        .InsertAndAdvance(
            new CodeInstruction(OpCodes.Ldloc_S, Convert.ToByte(13)),
            Transpilers.EmitDelegate<Action<ZDO>>(DeserializePostDelegate))
        .InstructionEnumeration();
    }

    static void DeserializePostDelegate(ZDO zdo) {
      if (PortalPrefabHashCodes.Contains(zdo.m_prefab)) {
        CachedPortalZdos.Add(zdo);
      }
    }
  }
}
