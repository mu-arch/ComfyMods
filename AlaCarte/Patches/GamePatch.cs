using HarmonyLib;

using static AlaCarte.PluginConfig;

namespace AlaCarte {
  [HarmonyPatch(typeof(Game))]
  static class GamePatch {
    [HarmonyPostfix]
    [HarmonyPatch(nameof(Game.Pause))]
    static void PausePostfix() {
      if (IsModEnabled.Value && DisableGamePauseOnMenu.Value) {
        Game.m_pause = false;
      }
    }
  }
}
