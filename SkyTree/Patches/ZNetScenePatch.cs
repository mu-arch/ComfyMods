using HarmonyLib;

using static SkyTree.PluginConfig;

namespace SkyTree.Patches {
  [HarmonyPatch(typeof(ZNetScene))]
  static class ZNetScenePatch {
    [HarmonyPostfix]
    [HarmonyPatch(nameof(ZNetScene.Awake))]
    static void AwakePostfix(ZNetScene __instance) {
      if (IsModEnabled.Value) {
        __instance.StartCoroutine(SkyTree.FixYggdrasilBranchCoroutine());
      }
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(ZNetScene.OnDestroy))]
    static void OnDestroyPrefix() {
      if (IsModEnabled.Value) {
        SkyTree.YggdrasilBranches.Clear();
      }
    }
  }
}
