using System;
using System.Collections.Generic;
using System.Reflection.Emit;

using HarmonyLib;

using Steamworks;

using static YellowPages.PluginConfig;

namespace YellowPages.Patches {
  [HarmonyPatch(typeof(ZSteamMatchmaking))]
  static class ZSteamMatchmakingPatch {
    [HarmonyTranspiler]
    [HarmonyPatch(nameof(ZSteamMatchmaking.OnServerResponded))]
    static IEnumerable<CodeInstruction> OnServerRespondedTranspiler(IEnumerable<CodeInstruction> instructions) {
      return new CodeMatcher(instructions)
          .MatchForward(
              useEnd: false,
              new CodeMatch(
                  OpCodes.Callvirt, AccessTools.Method(typeof(ServerStatus), nameof(ServerStatus.UpdateStatus))))
          .Advance(offset: 1)
          .InsertAndAdvance(
              new CodeInstruction(OpCodes.Ldloc_0),
              new CodeInstruction(OpCodes.Ldloc_3),
              Transpilers.EmitDelegate<Action<gameserveritem_t, ServerStatus>>(UpdateStatusPostDelegate))
          .InstructionEnumeration();
    }

    static void UpdateStatusPostDelegate(gameserveritem_t serverDetails, ServerStatus serverStatus) {
      if (IsModEnabled.Value) {
        serverStatus.m_joinData.m_serverName += ":maxPlayers=" + serverDetails.m_nMaxPlayers;
      }
    }
  }
}
