using HarmonyLib;

using static LetMePlay.PluginConfig;

namespace LetMePlay {
  [HarmonyPatch(typeof(Game))]
  static class GamePatch {
    [HarmonyPrefix]
    [HarmonyPatch(nameof(Game.canPause))]
    static bool CanPausePrefix(ref bool __result) {
      if (IsModEnabled.Value && DisableGameMenuPause.Value) {
        __result = false;
        return false;
      }

      return true;
    }
  }
}
