using HarmonyLib;

using static ColorfulPieces.PluginConfig;

namespace ColorfulPieces.Patches {
  [HarmonyPatch(typeof(StaticPhysics))]
  static class StaticPhysicsPatch {
    [HarmonyPostfix]
    [HarmonyPatch(nameof(StaticPhysics.Awake))]
    static void StaticPhysicsAwake(ref StaticPhysics __instance) {
      if (IsModEnabled.Value) {
        __instance.gameObject.AddComponent<PieceColor>();
      }
    }
  }
}
