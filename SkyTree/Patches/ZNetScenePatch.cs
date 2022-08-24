using HarmonyLib;

using static SkyTree.PluginConfig;
using static SkyTree.SkyTree;

namespace SkyTree.Patches {
  [HarmonyPatch(typeof(ZNetScene))]
  public class ZNetScenePatch {
    [HarmonyPostfix]
    [HarmonyPatch(nameof(ZNetScene.Awake))]
    static void AwakePostfix(ZNetScene __instance) {
      if (IsModEnabled.Value) {
        __instance.StartCoroutine(FixYggdrasilBranchCoroutine());
      }
    }
  }
}
