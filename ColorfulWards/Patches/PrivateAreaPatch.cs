using System.Collections.Generic;
using System.Reflection.Emit;

using ComfyLib;

using HarmonyLib;

using UnityEngine;

using static ColorfulWards.ColorfulWards;
using static ColorfulWards.PluginConfig;

namespace ColorfulWards {
  [HarmonyPatch(typeof(PrivateArea))]
  static class PrivateAreaPatch {
    [HarmonyEmitIL] // TODO REMOVE ME
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
    static void AwakePostfix(ref PrivateArea __instance) {
      if (!IsModEnabled.Value || !__instance) {
        return;
      }

      PrivateAreaDataCache.Add(__instance, new PrivateAreaData(__instance));
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(PrivateArea.OnDestroy))]
    static void OnDestroyPrefix(ref PrivateArea __instance) {
      PrivateAreaDataCache.Remove(__instance);
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(PrivateArea.UpdateStatus))]
    static void UpdateStatusPostfix(ref PrivateArea __instance) {
      if (!IsModEnabled.Value
          || !__instance
          || !__instance.m_nview
          || __instance.m_nview.m_zdo == null
          || !__instance.m_nview.m_zdo.TryGetVector3(PrivateAreaColorHashCode, out Vector3 colorVector3)
          || !PrivateAreaDataCache.TryGetValue(__instance, out PrivateAreaData privateAreaData)) {
        return;
      }

      Color wardColor = Utils.Vec3ToColor(colorVector3);
      wardColor.a = __instance.m_nview.m_zdo.GetFloat(PrivateAreaColorAlphaHashCode, defaultValue: 1f);

      if (privateAreaData.TargetColor == wardColor) {
        return;
      }

      privateAreaData.TargetColor = wardColor;
      SetPrivateAreaColors(__instance, privateAreaData);
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(PrivateArea.GetHoverText))]
    static void GetHoverTextPostfix(ref PrivateArea __instance, ref string __result) {
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
