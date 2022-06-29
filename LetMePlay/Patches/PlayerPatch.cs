using HarmonyLib;

using static LetMePlay.PluginConfig;

namespace LetMePlay {
  [HarmonyPatch(typeof(Player))]
  public class PlayerPatch {
    [HarmonyPostfix]
    [HarmonyPatch(nameof(Player.UpdatePlacementGhost))]
    static void UpdatePlacementGhostPostfix(ref Player __instance) {
      if (__instance
          && __instance.m_placementMarkerInstance
          && __instance.m_placementMarkerInstance.activeSelf
          && IsModEnabled.Value
          && DisableBuildPlacementMarker.Value) {
        __instance.m_placementMarkerInstance.SetActive(false);
      }
    }
  }
}
