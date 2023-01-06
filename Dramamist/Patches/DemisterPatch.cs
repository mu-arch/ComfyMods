using HarmonyLib;

using static Dramamist.PluginConfig;

namespace Dramamist {
  [HarmonyPatch(typeof(Demister))]
  static class DemisterPatch {
    [HarmonyPostfix]
    [HarmonyPatch(nameof(Demister.OnEnable))]
    static void OnEnablePostfix(ref Demister __instance) {
      if (IsModEnabled.Value) {
        Dramamist.UpdateDemisterSettings(__instance);
      }
    }
  }
}
