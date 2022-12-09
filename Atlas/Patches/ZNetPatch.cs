using HarmonyLib;

namespace Atlas {
  [HarmonyPatch(typeof(ZNet))]
  static class ZNetPatch {
    [HarmonyPrefix]
    [HarmonyPatch(nameof(ZNet.SaveWorld))]
    static bool SaveWorldPrefix(ref ZNet __instance, ref bool sync) {
      if (sync) {
        return true;
      }

      __instance.StartCoroutine(Atlas.SaveWorldCoroutine(__instance));
      return false;
    }
  }
}
