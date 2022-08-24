using HarmonyLib;

using static GetOffMyLawn.GetOffMyLawn;
using static GetOffMyLawn.PluginConfig;

namespace GetOffMyLawn {
  [HarmonyPatch(typeof(Piece))]
  public class PiecePatch {
    [HarmonyPostfix]
    [HarmonyPatch(nameof(Piece.SetCreator))]
    static void SetCreatorPostfix(ref Piece __instance) {
      if (!IsModEnabled.Value || !__instance || !__instance.m_nview || __instance.GetComponent<Plant>()) {
        return;
      }

      PluginLogger.LogInfo(
          $"Creating piece '{Localization.instance.Localize(__instance.m_name)}' with health: {PieceHealth.Value}");

      __instance.m_nview.GetZDO().Set(HealthHashCode, PieceHealth.Value);
    }
  }
}
