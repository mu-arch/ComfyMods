using HarmonyLib;

using static DyeHard.DyeHard;

namespace DyeHard.Patches {
  [HarmonyPatch(typeof(Player))]
  static class PlayerPatch {
    [HarmonyPostfix]
    [HarmonyPatch(nameof(Player.SetLocalPlayer))]
    static void SetLocalPlayerPostfix(ref Player __instance) {
      LocalPlayerCache = __instance;
      SetPlayerHairItem();
      SetPlayerZdoHairColor();
      SetPlayerBeardItem();
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(Player.OnSpawned))]
    static void OnSpawnedPostfix(ref Player __instance) {
      LocalPlayerCache = __instance;
      SetPlayerHairItem();
      SetPlayerZdoHairColor();
      SetPlayerBeardItem();
    }
  }
}
