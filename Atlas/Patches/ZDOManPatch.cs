using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection.Emit;

using HarmonyLib;

namespace Atlas {
  [HarmonyPatch(typeof(ZDOMan))]
  static class ZDOManPatch {
    [HarmonyTranspiler]
    [HarmonyPatch(nameof(ZDOMan.Load))]
    static IEnumerable<CodeInstruction> LoadTranspiler(IEnumerable<CodeInstruction> instructions) {
      return new CodeMatcher(instructions)
          .MatchForward(
              useEnd: false,
              new CodeMatch(OpCodes.Ldarg_0),
              new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(ZDOMan), nameof(ZDOMan.m_objectsByID))),
              new CodeMatch(OpCodes.Ldloc_S),
              new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(ZDO), nameof(ZDO.m_uid))),
              new CodeMatch(OpCodes.Ldloc_S),
              new CodeMatch(
                  OpCodes.Callvirt,
                  AccessTools.Method(typeof(Dictionary<ZDOID, ZDO>), nameof(Dictionary<ZDOID, ZDO>.Add))))
          .InsertAndAdvance(
              new CodeInstruction(OpCodes.Ldarg_0),
              new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(ZDOMan), nameof(ZDOMan.m_objectsByID))),
              new CodeInstruction(OpCodes.Ldloc_S, Convert.ToByte(10)),
              Transpilers.EmitDelegate<Action<Dictionary<ZDOID, ZDO>, ZDO>>(AddObjectsByIdPreDelegate))
          .InstructionEnumeration();
    }

    static void AddObjectsByIdPreDelegate(Dictionary<ZDOID, ZDO> objectsById, ZDO zdo) {
      if (objectsById.Remove(zdo.m_uid)) {
        ZLog.LogWarning($"Duplicate ZDO {zdo.m_uid} detected, overwriting.");
      }
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(ZDOMan.Load))]
    static void LoadPostfix(ref ZDOMan __instance) {
      ZLog.Log($"Loading ZDO.timeCreated for {__instance.m_objectsByID.Count} ZDOs.");
      Stopwatch stopwatch = Stopwatch.StartNew();

      foreach (ZDO zdo in __instance.m_objectsByID.Values) {
        if (ZDOExtraData.s_longs.TryGetValue(zdo.m_uid, out BinarySearchDictionary<int, long> values)
            && values.TryGetValue(Atlas.TimeCreatedHashCode, out long timeCreated)) {
          ZDOExtraData.s_tempTimeCreated[zdo.m_uid] = timeCreated;
        } else if (ZDOExtraData.s_tempTimeCreated.TryGetValue(zdo.m_uid, out timeCreated)) {
          zdo.Set(Atlas.TimeCreatedHashCode, timeCreated);
        } else {
          ZLog.LogWarning($"No ZDO.timeCreated found for ZDO {zdo.m_uid}, setting to 0.");
          ZDOExtraData.s_tempTimeCreated[zdo.m_uid] = 0L;
          zdo.Set(Atlas.TimeCreatedHashCode, 0L);
        }
      }

      stopwatch.Stop();
      ZLog.Log($"Finished loading ZDO.timeCreated, duration: {stopwatch.Elapsed}");
    }

    [HarmonyTranspiler]
    [HarmonyPatch(nameof(ZDOMan.RPC_ZDOData))]
    static IEnumerable<CodeInstruction> RpcZdoDataTranspiler(IEnumerable<CodeInstruction> instructions) {
      return new CodeMatcher(instructions)
          .MatchForward(
              useEnd: true,
              new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(ZDO), nameof(ZDO.Deserialize))),
              new CodeMatch(OpCodes.Pop))
        .InsertAndAdvance(
            new CodeInstruction(OpCodes.Ldloc_S, Convert.ToByte(12)),
            Transpilers.EmitDelegate<Action<ZDO>>(DeserializePostDelegate))
        .InstructionEnumeration();
    }

    static void DeserializePostDelegate(ZDO zdo) {
      if (ZDOExtraData.s_longs.TryGetValue(zdo.m_uid, out BinarySearchDictionary<int, long> values)
          && values.TryGetValue(Atlas.TimeCreatedHashCode, out long timeCreated)) {
        ZDOExtraData.s_tempTimeCreated[zdo.m_uid] = timeCreated;
      } else if (ZDOExtraData.s_tempTimeCreated.TryGetValue(zdo.m_uid, out timeCreated)) {
        zdo.Set(Atlas.TimeCreatedHashCode, timeCreated);
      } else {
        timeCreated = (long) (ZNet.m_instance.m_netTime * TimeSpan.TicksPerSecond);

        ZDOExtraData.s_tempTimeCreated[zdo.m_uid] = timeCreated;
        zdo.Set(Atlas.TimeCreatedHashCode, timeCreated);
      }
    }
  }
}
