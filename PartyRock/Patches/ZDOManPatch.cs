using HarmonyLib;

using System;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace PartyRock.Patches {
  [HarmonyPatch(typeof(ZDOMan))]
  public class ZDOManPatch {
    public static HashSet<ZDO> PlayerZdos { get; } = new();

    [HarmonyPostfix]
    [HarmonyPatch(
        nameof(ZDOMan.FindSectorObjects),
        typeof(Vector2i), typeof(int), typeof(int), typeof(List<ZDO>), typeof(List<ZDO>))]
    static void FindSectorObjectsPostfix(ref List<ZDO> sectorObjects) {
      sectorObjects.AddRange(PlayerZdos);
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(ZDOMan.DestroyZDO))]
    static void DestroyZDOPrefix(ref ZDO zdo) {
      if (PlayerZdos.Remove(zdo)) {
        ZLog.Log($"Destroying Player ZDO: {zdo?.m_uid}");
      }
    }

    [HarmonyTranspiler]
    [HarmonyPatch(nameof(ZDOMan.RPC_ZDOData))]
    static IEnumerable<CodeInstruction> RPC_ZDODataTranspiler(IEnumerable<CodeInstruction> instructions) {
      return new CodeMatcher(instructions)
        .MatchForward(useEnd: true, new CodeMatch(OpCodes.Callvirt, typeof(ZDO).GetMethod(nameof(ZDO.Deserialize))))
        .InsertAndAdvance(
            new CodeInstruction(OpCodes.Ldloc_S, Convert.ToByte(13)),
            Transpilers.EmitDelegate<Action<ZDO>>(DeserializeZdoDelegate))
        .InstructionEnumeration();
    }

    static readonly int PlayerHashCode = "Player".GetStableHashCode();

    static void DeserializeZdoDelegate(ZDO zdo) {
      if (zdo.m_prefab == PlayerHashCode) {
        PlayerZdos.Add(zdo);
      }
    }
  }
}
