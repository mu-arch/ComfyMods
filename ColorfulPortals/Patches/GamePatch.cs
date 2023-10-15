using HarmonyLib;

using static ColorfulPortals.PluginConfig;

namespace ColorfulPortals {
  [HarmonyPatch(typeof(Game))]
  static class GamePatch {
    [HarmonyPostfix]
    [HarmonyPatch(nameof(Game.Start))]
    static void StartPostfix(ref Game __instance) {
      if (IsModEnabled.Value) {
        __instance.gameObject.AddComponent<TeleportWorldColorUpdater>();
      }
    }
  }
}
