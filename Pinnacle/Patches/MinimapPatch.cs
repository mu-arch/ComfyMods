using HarmonyLib;

using System;
using System.Collections.Generic;
using System.Reflection.Emit;

using static Pinnacle.PluginConfig;

namespace Pinnacle {
  [HarmonyPatch(typeof(Minimap))]
  public class MinimapPatch {
    [HarmonyPostfix]
    [HarmonyPatch(nameof(Minimap.Awake))]
    static void AwakePostfix(ref Minimap __instance) {
      if (IsModEnabled.Value) {
        MinimapConfig.BindConfig(Config);
        MinimapConfig.SetMinimapPinFont();
      }
    }

    [HarmonyTranspiler]
    [HarmonyPatch(nameof(Minimap.OnMapLeftClick))]
    static IEnumerable<CodeInstruction> OnMapLeftClickTranspiler(IEnumerable<CodeInstruction> instructions) {
      return new CodeMatcher(instructions)
          .MatchForward(
              useEnd: false,
              new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(Minimap), nameof(Minimap.GetClosestPin))),
              new CodeMatch(OpCodes.Stloc_1))
          .Advance(offset: 1)
          .InsertAndAdvance(Transpilers.EmitDelegate<Func<Minimap.PinData, Minimap.PinData>>(GetClosestPinDelegate))
          .InstructionEnumeration();
    }

    static Minimap.PinData GetClosestPinDelegate(Minimap.PinData closestPin) {
      if (IsModEnabled.Value) {
        ZLog.Log($"Toggling the PinEditPanel.");
        Pinnacle.TogglePinEditPanel(closestPin);
        return null;
      }

      ZLog.Log($"I did not toggle the PinEditPanel.");
      return closestPin;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(Minimap.InTextInput))]
    static void InTextInputPostfix(ref bool __result) {
      if (IsModEnabled.Value
          && !__result
          && Pinnacle.PinEditPanel?.Panel
          && Pinnacle.PinEditPanel.Panel.activeSelf
          && Pinnacle.PinEditPanel.HasFocus()) {
        __result = true;
      }
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(Minimap.SetMapMode))]
    static void SetMapModePostfix(ref Minimap.MapMode mode) {
      if (IsModEnabled.Value && mode != Minimap.MapMode.Large && Pinnacle.PinEditPanel?.Panel) {
        Pinnacle.PinEditPanel.Panel.SetActive(false);
      }
    }
  }
}
