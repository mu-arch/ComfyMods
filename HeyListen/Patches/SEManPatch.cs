using HarmonyLib;

using static HeyListen.PluginConfig;

namespace HeyListen {
  [HarmonyPatch(typeof(SEMan))]
  static class SEManPatch {
    [HarmonyPostfix]
    [HarmonyPatch(nameof(SEMan.GetStatusEffect), typeof(string))]
    static void GetStatusEffectPostfix(ref string name, ref StatusEffect __result) {
      if (IsModEnabled.Value && name == "Demister") {
        ZLog.Log("Returning null for GSE Demister.");
        __result = null;
      }
    }
  }
}
