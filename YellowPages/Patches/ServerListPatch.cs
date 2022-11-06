using System;
using System.Collections.Generic;
using System.Reflection.Emit;

using HarmonyLib;

using static YellowPages.PluginConfig;

namespace YellowPages {
  [HarmonyPatch(typeof(ServerList))]
  static class ServerListPatch {
    static readonly string[] _maxSeparator = new string[] { "//max:" };

    [HarmonyTranspiler]
    [HarmonyPatch(nameof(ServerList.UpdateServerListGui))]
    static IEnumerable<CodeInstruction> UpdateServerListGuiTranspiler(IEnumerable<CodeInstruction> instructions) {
      return new CodeMatcher(instructions)
          .MatchForward(
              useEnd: false,
              new CodeMatch(
                  OpCodes.Ldfld,
                  AccessTools.Method(
                      typeof(ServerList.ServerListElement), nameof(ServerList.ServerListElement.m_serverStatus))),
              new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(ServerStatus), "get_m_joinData")),
              new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(object), nameof(object.ToString))))
          .Advance(offset: 2)
          .InsertAndAdvance(Transpilers.EmitDelegate<Func<ServerJoinData, ServerJoinData>>(DuplicateElementDelegate))
          .MatchForward(
              useEnd: false,
              new CodeMatch(OpCodes.Ldarg_0),
              new CodeMatch(
                  OpCodes.Ldflda, AccessTools.Field(typeof(ServerList), nameof(ServerList.m_serverPlayerLimit))),
              new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(int), nameof(int.ToString))))
          .Advance(offset: 3)
          .InsertAndAdvance(
              new CodeInstruction(OpCodes.Ldloc_S, 7),
              Transpilers.EmitDelegate<Func<string, ServerStatus, string>>(ServerPlayerLimitDelegate))
          .InstructionEnumeration();
    }

    static ServerJoinData DuplicateElementDelegate(ServerJoinData joinData) {
      if (IsModEnabled.Value) {
        ZLog.LogWarning($"??? {joinData.m_serverName} -- {joinData.GetType()} -- {joinData}");
      }

      return joinData;
    }

    static string ServerPlayerLimitDelegate(string limitString, ServerStatus serverStatus) {
      if (IsModEnabled.Value) {
        int index = serverStatus.m_joinData.m_serverName.IndexOf(":maxPlayers=", StringComparison.Ordinal);

        if (index >= 0) {
          return serverStatus.m_joinData.m_serverName.Substring(index + 12);
        }
      }

      return limitString;
    }
  }

  [HarmonyPatch(typeof(CensorShittyWords))]
  static class CensorShittyWordsPatch {
    [HarmonyPrefix]
    [HarmonyPatch(nameof(CensorShittyWords.UGCServerName))]
    static void UGCServerNamePostfix(ref string serverName) {
      if (IsModEnabled.Value) {
        int index = serverName.IndexOf(":maxPlayers=", StringComparison.Ordinal);

        if (index >= 0) {
          serverName = serverName.Substring(0, index);
        }
      }
    }
  }
}
