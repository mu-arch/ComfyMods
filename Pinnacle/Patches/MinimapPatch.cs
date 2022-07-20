using HarmonyLib;

using System;
using System.Collections.Generic;
using System.Reflection.Emit;

using UnityEngine;

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

        Pinnacle.TogglePinEditPanel(pin: null);
        Pinnacle.TogglePinListPanel();
      }
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Minimap.OnMapLeftClick))]
    static bool OnMapLeftClickPrefix(ref Minimap __instance) {
      if (IsModEnabled.Value
          && Console.m_instance.IsCheatsEnabled()
          && Player.m_localPlayer
          && Input.GetKeyDown(KeyCode.LeftShift)) {
        Vector3 targetPosition = __instance.ScreenToWorldPoint(Input.mousePosition);

        __instance.SetMapMode(Minimap.MapMode.Small);
        __instance.m_smallRoot.SetActive(true);

        Pinnacle.TeleportTo(targetPosition);
        return false;
      }

      return true;
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
        Pinnacle.TogglePinEditPanel(closestPin);
        return null;
      }

      return closestPin;
    }

    [HarmonyTranspiler]
    [HarmonyPatch(nameof(Minimap.Update))]
    static IEnumerable<CodeInstruction> UpdateTranspiler(IEnumerable<CodeInstruction> instructions) {
      return new CodeMatcher(instructions)
          .MatchForward(
              useEnd: false,
              new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(Minimap), nameof(Minimap.InTextInput))))
          .InsertAndAdvance(Transpilers.EmitDelegate<Action>(InTextInputPreDelegate))
          .InstructionEnumeration();
    }

    static void InTextInputPreDelegate() {
      if (IsModEnabled.Value && PinListPanelToggleShortcut.Value.IsDown()) {
        Pinnacle.TogglePinListPanel();
      }
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(Minimap.InTextInput))]
    static void InTextInputPostfix(ref bool __result) {
      if (IsModEnabled.Value && !__result) {
        if (Pinnacle.PinEditPanel?.Panel
            && Pinnacle.PinEditPanel.Panel.activeSelf
            && Pinnacle.PinEditPanel.HasFocus()) {
          __result = true;
        } else if (Pinnacle.PinListPanel?.Panel && Pinnacle.PinListPanel.HasFocus()) {
          __result = true;
        }
      }
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Minimap.UpdateMap))]
    static void UpdateMapPrefix(ref bool takeInput) {
      if (IsModEnabled.Value && Pinnacle.PinListPanel?.Panel && Pinnacle.PinListPanel.HasFocus()) {
        takeInput = false;
      }
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Minimap.RemovePin), typeof(Minimap.PinData))]
    static void RemovePinPrefix(ref Minimap.PinData pin) {
      if (IsModEnabled.Value && Pinnacle.PinEditPanel?.TargetPin == pin) {
        Pinnacle.TogglePinEditPanel(null);
      }
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Minimap.SetMapMode))]
    static void SetMapModePrefix(ref Minimap __instance, ref Minimap.MapMode mode) {
      if (IsModEnabled.Value
          && Pinnacle.PinListPanel?.Panel
          && Pinnacle.PinListPanel.Panel.activeSelf
          && mode != __instance.m_mode
          && mode == Minimap.MapMode.Large) {
        Pinnacle.PinListPanel.SetTargetPins();
      }
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(Minimap.SetMapMode))]
    static void SetMapModePostfix(ref Minimap.MapMode mode) {
      if (IsModEnabled.Value && mode != Minimap.MapMode.Large && Pinnacle.PinEditPanel?.Panel) {
        Pinnacle.TogglePinEditPanel(null);
      }
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(Minimap.ShowPinNameInput))]
    static void ShowPinNameInputPostfix(ref Minimap __instance, ref Minimap.PinData pin) {
      if (IsModEnabled.Value) {
        __instance.m_namePin = null;

        Pinnacle.TogglePinEditPanel(pin);
        Pinnacle.PinEditPanel?.PinName?.Value?.InputField.Ref()?.ActivateInputField();
      }
    }
  }
}
