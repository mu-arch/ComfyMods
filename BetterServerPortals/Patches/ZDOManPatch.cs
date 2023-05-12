using System;
using System.Collections.Generic;
using System.Reflection.Emit;

using HarmonyLib;

using static BetterServerPortals.BetterServerPortals;

namespace BetterServerPortals {
  [HarmonyPatch(typeof(ZDOMan))]
  static class ZDOManPatch {
    [HarmonyPostfix]
    [HarmonyPatch(nameof(ZDOMan.ResetSectorArray))]
    static void ResetSectorArrayPostfix() {
      CachedPortalZdos.Clear();
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(ZDOMan.AddToSector))]
    static void AddToSectorPostfix(ZDO zdo) {
      if (ZNet.m_isServer && zdo != null && PortalPrefabHashCodes.Contains(zdo.m_prefab)) {
        CachedPortalZdos.Add(zdo);
      }
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(ZDOMan.RemoveFromSector))]
    static void RemoveFromSectorPostfix(ZDO zdo) {
      if (ZNet.m_isServer && zdo != null) {
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
      if (ZNet.m_isServer && PortalPrefabHashCodes.Contains(zdo.m_prefab)) {
        CachedPortalZdos.Add(zdo);
      }
    }
  }
}
