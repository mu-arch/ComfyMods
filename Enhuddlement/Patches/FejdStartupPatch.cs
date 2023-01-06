using System.Collections.Generic;
using System.Reflection;

using HarmonyLib;

namespace Enhuddlement {
  [HarmonyPatch(typeof(FejdStartup))]
  static class FejdStartupPatch {
    static readonly HashSet<string> _targetHarmonyIds = new() { "MK_BetterUI" };

    [HarmonyPostfix]
    [HarmonyPatch(nameof(FejdStartup.Awake))]
    [HarmonyPriority(Priority.Last)]
    static void AwakePostfix() {
      UnpatchIfPatched(typeof(EnemyHud));
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
            Enhuddlement.HarmonyInstance?.Unpatch(method, HarmonyPatchType.All, harmonyId);
          }
        }
      }
    }
  }
}
