using HarmonyLib;

using static LetMePlay.PluginConfig;

namespace LetMePlay {
  [HarmonyPatch(typeof(PrivateArea))]
  public class PrivateAreaPatch {
    [HarmonyPrefix]
    [HarmonyPatch(nameof(PrivateArea.RPC_FlashShield))]
    static bool PrivateAreaRpcFlashShield() {
      if (IsModEnabled.Value && DisableWardShieldFlash.Value) {
        return false;
      }

      return true;
    }
  }
}
