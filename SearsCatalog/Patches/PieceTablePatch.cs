using System;
using System.Collections.Generic;
using System.Reflection.Emit;

using HarmonyLib;

using UnityEngine;

using static SearsCatalog.PluginConfig;

namespace SearsCatalog {
  [HarmonyPatch(typeof(PieceTable))]
  static class PieceTablePatch {
    [HarmonyPostfix]
    [HarmonyPatch(nameof(PieceTable.SetCategory))]
    static void SetCategoryPostfix() {
      if (IsModEnabled.Value) {
        SearsCatalog.BuildHudNeedRefresh = true;
      }
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(PieceTable.PrevCategory))]
    static void PrevCategoryPostfix() {
      if (IsModEnabled.Value) {
        SearsCatalog.BuildHudNeedRefresh = true;
      }
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(PieceTable.NextCategory))]
    static void NextCategoryPostfix() {
      if (IsModEnabled.Value) {
        SearsCatalog.BuildHudNeedRefresh = true;
      }
    }

    [HarmonyTranspiler]
    [HarmonyPatch(nameof(PieceTable.GetPiece), typeof(int), typeof(Vector2Int))]
    static IEnumerable<CodeInstruction> GetPieceTranspiler(IEnumerable<CodeInstruction> instructions) {
      return new CodeMatcher(instructions)
          .MatchForward(
              useEnd: false,
              new CodeMatch(OpCodes.Ldarga_S),
              new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(Vector2Int), "get_y")),
              new CodeMatch(OpCodes.Ldc_I4_S))
          .Advance(offset: 3)
          .InsertAndAdvance(Transpilers.EmitDelegate<Func<int, int>>(GetPieceGetYPostDelegate))
          .InstructionEnumeration();
    }

    static int GetPieceGetYPostDelegate(int value) {
      return IsModEnabled.Value ? SearsCatalog.BuildHudColumns : value;
    }
  }
}
