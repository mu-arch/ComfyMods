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
        return new CodeMatcher(instructions)
            .MatchForward(
                useEnd: false,
                new CodeMatch(OpCodes.Ldc_I4_8),
                new CodeMatch(OpCodes.Ldloca_S),
                new CodeMatch(OpCodes.Call),
                new CodeMatch(OpCodes.Stloc_3))
            .SetAndAdvance(OpCodes.Ldc_I4, 9)
            .InstructionEnumeration();
      }
    }
  }
}