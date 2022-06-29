using HarmonyLib;

using static LetMePlay.PluginConfig;

namespace LetMePlay {
  [HarmonyPatch(typeof(SpawnArea))]
  public class SpawnareaPatch {
    [HarmonyPostfix]
    [HarmonyPatch(nameof(SpawnArea.Awake))]
    static void AwakePostfix(ref SpawnArea __instance) {
      if (!IsModEnabled.Value) {
        return;
      }

      __instance.m_prefabs?.RemoveAll(spawnData => !spawnData?.m_prefab);
    }
  }
}
