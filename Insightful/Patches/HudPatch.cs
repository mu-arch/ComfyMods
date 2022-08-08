using HarmonyLib;

using static Insightful.Insightful;
using static Insightful.PluginConfig;

namespace Insightful.Patches {
  [HarmonyPatch(typeof(Hud))]
  static class HudPatch {
    [HarmonyPostfix]
    [HarmonyPatch(nameof(Hud.UpdateCrosshair))]
    static void UpdateCrosshairPostfix(ref Hud __instance, ref Player player) {
      if (!IsModEnabled.Value || !Player.m_localPlayer.m_hovering) {
        return;
      }

      ZDO zdo = player.m_hovering.GetComponentInParent<ZNetView>().Ref()?.m_zdo;

      if (zdo?.m_strings != null
          && zdo.m_strings.ContainsKey(InscriptionTopicHashCode)
          && zdo.m_strings.ContainsKey(InscriptionTextHashCode)) {
        __instance.m_hoverName.Append(
            $"[<color=yellow><b>{ReadHiddenTextShortcut.Value}</b></color>] Read Inscription");
      }
    }
  }
}
