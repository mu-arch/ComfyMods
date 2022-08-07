using HarmonyLib;

using static PotteryBarn.PluginConfig;

namespace PotteryBarn.Patches {
  [HarmonyPatch(typeof(ArmorStand))]
  public static class ArmorStandPatch {
    [HarmonyPrefix]
    [HarmonyPatch(nameof(ArmorStand.SetPose))]
    static void SetPosePrefix(ref bool effect) {
      if (IsModEnabled.Value) {
        effect = false;
      }
    }
  }
}
