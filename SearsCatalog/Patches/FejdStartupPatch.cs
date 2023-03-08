using System.Collections.Generic;
using System.Reflection;

using HarmonyLib;

namespace SearsCatalog {
  [HarmonyPatch(typeof(FejdStartup))]
  static class FejdStartupPatch {
    static readonly HashSet<string> _targetHarmonyIds = new() { "mixone.valheimplus.buildexpansion" };

    [HarmonyPostfix]
    [HarmonyPatch(nameof(FejdStartup.Awake))]
    [HarmonyPriority(Priority.Last)]
    static void AwakePostfix() {
      UnpatchIfPatched(typeof(Hud));
      UnpatchIfPatched(typeof(PieceTable));
    }

    static void UnpatchIfPatched(System.Type type) {
      foreach (MethodInfo method in AccessTools.GetDeclaredMethods(type)) {
        Patches patches = Harmony.GetPatchInfo(method);

        if (patches == null) {
          continue;
        }

        foreach (string harmonyId in patches.Owners) {
          if (_targetHarmonyIds.Contains(harmonyId)) {
            ZLog.Log($"Unpatching all '{harmonyId}' patches on {type.FullName}.{method.Name}");
            SearsCatalog.HarmonyInstance?.Unpatch(method, HarmonyPatchType.All, harmonyId);
          }
        }
      }
    }
  }
}