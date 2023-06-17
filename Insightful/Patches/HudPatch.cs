using HarmonyLib;

using static Insightful.Insightful;
using static Insightful.PluginConfig;

namespace Insightful {
  [HarmonyPatch(typeof(Hud))]
  static class HudPatch {
    [HarmonyPostfix]
    [HarmonyPatch(nameof(Hud.UpdateCrosshair))]
    static void UpdateCrosshairPostfix(ref Hud __instance) {
      if (!IsModEnabled.Value || !Player.m_localPlayer || !Player.m_localPlayer.m_hovering) {
        return;
      }

      ZDO zdo = Player.m_localPlayer.m_hovering.GetComponentInParent<ZNetView>().Ref()?.m_zdo;

      if (zdo != null
          && zdo.TryGetString(InscriptionTopicHashCode, out _)
          && zdo.TryGetString(InscriptionTextHashCode, out _)) {
        __instance.m_hoverName.Append(
            $"[<color=yellow><b>{ReadHiddenTextShortcut.Value}</b></color>] Read Inscription");
      }
    }
  }
}
