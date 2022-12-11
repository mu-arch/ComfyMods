using HarmonyLib;

using static Dramamist.PluginConfig;

namespace Dramamist.Patches {
  [HarmonyPatch(typeof(Demister))]
  static class DemisterPatch {
    [HarmonyPostfix]
    [HarmonyPatch(nameof(Demister.Awake))]
    static void AwakePostfix(ref Demister __instance) {
      if (IsModEnabled.Value) {
        Dramamist.UpdateDemisterSettings();
      }
    }
  }
}
