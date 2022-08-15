using HarmonyLib;

using UnityEngine;

using static DyeHard.DyeHard;
using static DyeHard.PluginConfig;

namespace DyeHard.Patches {
  [HarmonyPatch(typeof(VisEquipment))]
  static class VisEquipmentPatch {
    [HarmonyPrefix]
    [HarmonyPatch(nameof(VisEquipment.SetHairColor))]
    static void SetHairColorPrefix(ref VisEquipment __instance, ref Vector3 color) {
      if (IsModEnabled.Value
          && OverridePlayerHairColor.Value
          && __instance.TryGetComponent(out Player player)
          && player == LocalPlayerCache) {
        color = GetPlayerHairColorVector();
      }
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(VisEquipment.SetHairItem))]
    static void SetHairItemPrefix(ref VisEquipment __instance, ref string name) {
      if (IsModEnabled.Value
          && OverridePlayerHairItem.Value
          && __instance.TryGetComponent(out Player player)
          && player == LocalPlayerCache) {
        name = PlayerHairItem.Value;
      }
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(VisEquipment.SetBeardItem))]
    static void SetBeardItemPrefix(ref VisEquipment __instance, ref string name) {
      if (IsModEnabled.Value
          && OverridePlayerBeardItem.Value
          && __instance.TryGetComponent(out Player player)
          && player == LocalPlayerCache) {
        name = PlayerBeardItem.Value;
      }
    }
  }
}
