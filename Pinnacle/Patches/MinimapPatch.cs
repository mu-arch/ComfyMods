using BepInEx.Configuration;

using HarmonyLib;

using System;
using System.Collections.Generic;
using System.Reflection.Emit;

using UnityEngine;

using static Pinnacle.PluginConfig;

namespace Pinnacle {
  [HarmonyPatch(typeof(Minimap))]
  public class MinimapPatch {
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
        Pinnacle.TogglePinEditPanel(Input.GetKey(KeyCode.RightShift) ? closestPin : null);
        return Input.GetKey(KeyCode.RightShift) ? null : closestPin;
      }

      return closestPin;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(Minimap.InTextInput))]
    static void InTextInputPostfix(ref bool __result) {
      if (IsModEnabled.Value && !__result && Pinnacle.PinEditPanel?.Panel && Pinnacle.PinEditPanel.Panel.activeSelf) {
        __result = true;
      }
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(Minimap.Awake))]
    static void AwakePostfix(ref Minimap __instance) {
      if (IsModEnabled.Value) {
        MinimapConfig.BindConfig(Config);
        MinimapConfig.SetMinimapPinFont();
      }
    }
  }
}
