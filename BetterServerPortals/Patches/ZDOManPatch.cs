using System;
using System.Collections.Generic;
using System.Reflection.Emit;

using HarmonyLib;

namespace BetterServerPortals {
  [HarmonyPatch(typeof(ZDOMan))]
  static class ZDOManPatch {
    static readonly int _portalHashCode = "portal".GetStableHashCode(); // 'Stone' portal prefab.

    [HarmonyPostfix]
    [HarmonyPatch(nameof(ZDOMan.AddToSector))]
    static void AddToSectorPostfix(ref ZDOMan __instance, ZDO zdo) {
      if (zdo.m_prefab == _portalHashCode) {
        __instance.AddPortal(zdo);
      }
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(ZDOMan.RemoveFromSector))]
    static void RemoveFromSectorPostfix(ref ZDOMan __instance, ZDO zdo) {
      if (zdo.m_prefab == _portalHashCode) {
        __instance.m_portalObjects.Remove(zdo);
      }
    }

    [HarmonyTranspiler]
    [HarmonyPatch(nameof(ZDOMan.RPC_ZDOData))]
    static IEnumerable<CodeInstruction> RpcZdoDataTranspiler(IEnumerable<CodeInstruction> instructions) {
      return new CodeMatcher(instructions)
          .MatchForward(
              useEnd: false,
              new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(ZDO), nameof(ZDO.Deserialize))),
              new CodeMatch(OpCodes.Pop))
          .Advance(offset: 2)
          .InsertAndAdvance(
              new CodeInstruction(OpCodes.Ldarg_0),
              new CodeInstruction(OpCodes.Ldloc_S, Convert.ToByte(12)),
              Transpilers.EmitDelegate<Action<ZDOMan, ZDO>>(CacheStonePortal))
          .InstructionEnumeration();
    }

    static void CacheStonePortal(ZDOMan zdoMan, ZDO zdo) {
      if (zdo.m_prefab == _portalHashCode) {
        zdoMan.AddPortal(zdo);
      }
    }
  }
}
