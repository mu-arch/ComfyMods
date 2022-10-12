using System;
using System.Collections.Generic;
using System.Reflection.Emit;

using HarmonyLib;

using UnityEngine;

using static LicensePlate.PluginConfig;

namespace LicensePlate {
  [HarmonyPatch(typeof(ShipControlls))]
  static class ShipControllsPatch {
    [HarmonyPostfix]
    [HarmonyPatch(nameof(ShipControlls.Awake))]
    static void AwakePostfix(ref ShipControlls __instance) {
      if (IsModEnabled.Value) {
        __instance.gameObject.AddComponent<ShipName>();
      }
    }

    [HarmonyTranspiler]
    [HarmonyPatch(nameof(ShipControlls.Interact))]
    static IEnumerable<CodeInstruction> InteractTranspiler(IEnumerable<CodeInstruction> instructions) {
      return new CodeMatcher(instructions)
          .MatchForward(
              useEnd: false,
              new CodeMatch(OpCodes.Ldloc_0),
              new CodeMatch(
                  OpCodes.Callvirt, AccessTools.Method(typeof(Character), nameof(Character.GetStandingOnShip))),
              new CodeMatch(OpCodes.Ldarg_0),
              new CodeMatch(OpCodes.Ldfld, AccessTools.Method(typeof(ShipControlls), nameof(ShipControlls.m_ship))),
              new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(UnityEngine.Object), "op_Inequality")))
          .Advance(offset: 5)
          .InsertAndAdvance(
              new CodeInstruction(OpCodes.Ldarg_0),
              new CodeInstruction(OpCodes.Ldarg_3),
              Transpilers.EmitDelegate<Func<bool, ShipControlls, bool, bool>>(StandingOnShipInequalityDelegate))
          .InstructionEnumeration();
    }

    static bool StandingOnShipInequalityDelegate(bool isNotEqual, ShipControlls shipControls, bool alt) {
      ZLog.Log($"IsEqual: {isNotEqual}, alt: {alt}, keydown: {Input.GetKey(KeyCode.RightShift)}");

      if (!isNotEqual
          && (alt || Input.GetKey(KeyCode.RightShift))
          && IsModEnabled.Value
          && PrivateArea.CheckAccess(shipControls.transform.position)
          && shipControls.m_nview
          && shipControls.m_nview.IsValid()
          && shipControls.m_nview.IsOwner()
          && shipControls.TryGetComponent(out ShipName shipName)) {
        TextInput.m_instance.RequestText(shipName, "$hud_rename", 50);
        return true;
      }

      return isNotEqual;
    }
  }
}
