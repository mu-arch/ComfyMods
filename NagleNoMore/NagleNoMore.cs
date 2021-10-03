using BepInEx;

using HarmonyLib;

using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace NagleNoMore {
  [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
  public class NagleNoMore : BaseUnityPlugin {
    public const string PluginGUID = "redseiko.valheim.naglenomore";
    public const string PluginName = "NagleNoMore";
    public const string PluginVersion = "1.0.0";

    Harmony _harmony;

    public void Awake() {
      _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), harmonyInstanceId: PluginGUID);
    }

    public void OnDestroy() {
      _harmony?.UnpatchSelf();
    }

    [HarmonyPatch(typeof(ZSteamSocket))]
    class ZSteamSocketPatch {
      [HarmonyTranspiler]
      [HarmonyPatch(nameof(ZSteamSocket.SendQueuedPackages))]
      static IEnumerable<CodeInstruction> SendQueuedPackagesTranspiler(IEnumerable<CodeInstruction> instructions) {
        // nSendFlags:
        //   * k_nSteamNetworkingSend_NoNagle = 1
        //   * k_nSteamNetworkingSend_Reliable = 8
        return new CodeMatcher(instructions)
            .MatchForward(
                useEnd: false,
                new CodeMatch(OpCodes.Ldc_I4_8), // k_nSteamNetworkingSend_Reliable
                new CodeMatch(OpCodes.Ldloca_S), // out num
                new CodeMatch(OpCodes.Call),     // ... SteamNetworkingSockets.SendMessageToConnection(...)
                new CodeMatch(OpCodes.Stloc_3))  // EResult result = ...
            .SetAndAdvance(OpCodes.Ldc_I4, 9)    // k_nSteamNetworkingSend_NoNagle | k_nSteamNetworkingSend_Reliable
            .InstructionEnumeration();
      }
    }
  }
}