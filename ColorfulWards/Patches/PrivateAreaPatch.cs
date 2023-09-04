using System.Collections.Generic;
using System.Reflection.Emit;

using ComfyLib;

using HarmonyLib;

using UnityEngine;

using static ColorfulWards.PluginConfig;

namespace ColorfulWards {
  [HarmonyPatch(typeof(PrivateArea))]
  static class PrivateAreaPatch {
    [HarmonyTranspiler]
    [HarmonyPatch(nameof(PrivateArea.IsInside))]
    static IEnumerable<CodeInstruction> IsInsideTranspiler(IEnumerable<CodeInstruction> instructions) {
      return new CodeMatcher(instructions)
          .MatchForward(
              useEnd: false,
              new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(Utils), nameof(Utils.DistanceXZ))))
          .SetOperandAndAdvance(AccessTools.Method(typeof(PrivateAreaPatch), nameof(DistanceDelegate)))
          .InstructionEnumeration();
    }

    static float DistanceDelegate(Vector3 point0, Vector3 point1) {
      if (IsModEnabled.Value && UseRadiusForVerticalCheck.Value) {
        return Vector3.Distance(point0, point1);
      }

      return Utils.DistanceXZ(point0, point1);
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(PrivateArea.Awake))]
    static void AwakePostfix(PrivateArea __instance) {
      if (IsModEnabled.Value && __instance && __instance.m_nview && __instance.m_nview.IsValid()) {
        __instance.gameObject.AddComponent<PrivateAreaColor>();
      }
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(PrivateArea.UpdateStatus))]
    static void UpdateStatusPostfix(PrivateArea __instance) {
      if (IsModEnabled.Value && __instance && __instance.TryGetComponent(out PrivateAreaColor privateAreaColor)) {
        privateAreaColor.UpdateColors();
      }
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(PrivateArea.GetHoverText))]
    static void GetHoverTextPostfix(PrivateArea __instance, ref string __result) {
      if (!IsModEnabled.Value
          || !ShowChangeColorHoverText.Value
          || !__instance
          || !__instance.m_piece
          || !__instance.m_piece.IsCreator()) {
        return;
      }

      __result =
          string.Format(
              "{0}\n[<color={1}>{2}</color>] Change ward color to: <color=#{3}>#{3}</color>",
              __result,
              "#FFA726",
              ChangeWardColorShortcut.Value,
              TargetWardColor.Value.GetColorHtmlString());
    }
  }
}
