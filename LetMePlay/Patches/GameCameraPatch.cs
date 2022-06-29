using HarmonyLib;

using UnityEngine;

using static LetMePlay.PluginConfig;

namespace LetMePlay {
  [HarmonyPatch(typeof(GameCamera))]
  public class GameCameraPatch {
    [HarmonyPostfix]
    [HarmonyPatch(nameof(GameCamera.GetCameraBaseOffset))]
    static void GetCameraBaseOffsetPostfix(ref Vector3 __result, Player player) {
      if (IsModEnabled.Value && DisableCameraSwayWhileSitting.Value) {
        __result = player.m_eye.transform.position - player.transform.position;
      }
    }
  }
}
