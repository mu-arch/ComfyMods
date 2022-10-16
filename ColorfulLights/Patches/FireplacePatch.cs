using System;
using System.Collections.Generic;
using System.Reflection.Emit;

using HarmonyLib;

using UnityEngine;

using static ColorfulLights.ColorfulLights;
using static ColorfulLights.PluginConfig;

namespace ColorfulLights {
  [HarmonyPatch(typeof(Fireplace))]
  static class FireplacePatch {
    [HarmonyPostfix]
    [HarmonyPatch(nameof(Fireplace.Awake))]
    static void AwakePostfix(ref Fireplace __instance) {
      if (IsModEnabled.Value
          && __instance
          && __instance.m_nview
          && __instance.m_nview.IsValid()
          && !__instance.TryGetComponent(out FireplaceColor _)) {
        __instance.gameObject.AddComponent<FireplaceColor>();
      }
    }

    static readonly string _changeColorHoverTextTemplate =
        "{0}\n<size={4}>[<color={1}>{2}</color>] Change fire color to: <color=#{3}>#{3}</color></size>";

    [HarmonyPostfix]
    [HarmonyPatch(nameof(Fireplace.GetHoverText))]
    static void GetHoverTextPostfix(ref Fireplace __instance, ref string __result) {
      if (!IsModEnabled.Value || !ShowChangeColorHoverText.Value || !__instance) {
        return;
      }

      __result =
          Localization.instance.Localize(
              string.Format(
                  _changeColorHoverTextTemplate,
                  __result,
                  "#FFA726",
                  ChangeColorActionShortcut.Value,
                  TargetFireplaceColor.Value.GetColorHtmlString(),
                  ColorPromptFontSize.Value));
    }

    [HarmonyTranspiler]
    [HarmonyPatch(nameof(Fireplace.UseItem))]
    static IEnumerable<CodeInstruction> UseItemTranspiler(IEnumerable<CodeInstruction> instructions) {
      return new CodeMatcher(instructions)
      .MatchForward(
              useEnd: false,
              new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(Component), "get_transform")),
              new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(Transform), "get_position")),
              new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(Quaternion), "get_identity")),
              new CodeMatch(OpCodes.Ldarg_0),
              new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(Fireplace), nameof(Fireplace.m_fireworks))),
              new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(ZNetScene), nameof(ZNetScene.SpawnObject))))
          .Advance(offset: 3)
          .InsertAndAdvance(
              new CodeInstruction(OpCodes.Ldarg_0),
              Transpilers.EmitDelegate<Func<Quaternion, Fireplace, Quaternion>>(SpawnObjectQuaternionDelegate))
          .InstructionEnumeration();
    }

    static Quaternion SpawnObjectQuaternionDelegate(Quaternion rotation, Fireplace fireplace) {
      if (IsModEnabled.Value
          && fireplace.TryGetComponent(out FireplaceColor fireplaceColor)
          && fireplaceColor.TargetColor != Color.clear) {
        Color color = fireplaceColor.TargetColor.SetAlpha(1f);

        rotation.x = color.r;
        rotation.y = color.g;
        rotation.z = color.b;

        PluginLogger.LogInfo($"Sending fireworks with color: {color}, rotation: {rotation}");
      }

      return rotation;
    }
  }
}
