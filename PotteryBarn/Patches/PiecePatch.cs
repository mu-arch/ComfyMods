using System.Collections.Generic;
using System.Linq;

using HarmonyLib;

using UnityEngine;

using static PotteryBarn.PotteryBarn;

namespace PotteryBarn {
  [HarmonyPatch(typeof(Piece))]
  static class PiecePatch {
    [HarmonyPrefix]
    [HarmonyPatch(nameof(Piece.DropResources))]
    public static void DropResourcePrefix(Piece __instance) {
      // Should not need to check against cultivator creator shop items here because they do not pass the
      // Player.CanRemovePiece check.
      if (Requirements.hammerCreatorShopItems.Keys.Contains(__instance.m_description)) {
        IsDropTableDisabled = true;
      }

      LogMessage($"Piece description {__instance.m_description}");

      foreach (Piece.Requirement requirement in __instance.m_resources) {
        GameObject gameObject = requirement.m_resItem.gameObject;

        if (gameObject) {
          LogMessage($"Dropping {gameObject.name}");
        }
      }
    }
  }

  [HarmonyPatch(typeof(DropOnDestroyed))]
  static class DropOnDestroyedPatch {
    [HarmonyPrefix]
    [HarmonyPatch(nameof(DropOnDestroyed.OnDestroyed))]
    public static bool OnDestroyedPrefix(DropOnDestroyed __instance) {
      if (IsDropTableDisabled) {
        IsDropTableDisabled = false;
        return false;
      }

      LogMessage($"Item destroyed not player made from Pottery barn. Using normal drop table. Dropping items:");
      List<GameObject> dropList = __instance.m_dropWhenDestroyed.GetDropList();

      for (int i = 0; i < dropList.Count; i++) {
        LogMessage($"{dropList[i].name}");
      }

      return true;
    }
  }
}
