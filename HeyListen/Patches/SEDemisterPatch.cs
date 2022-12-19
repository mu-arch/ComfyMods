using HarmonyLib;

using static HeyListen.HeyListen;
using static HeyListen.PluginConfig;

namespace HeyListen {
  [HarmonyPatch(typeof(SE_Demister))]
  static class SEDemisterPatch {
    [HarmonyPrefix]
    [HarmonyPatch(nameof(SE_Demister.Setup))]
    static void SetupPrefix(ref SE_Demister __instance, ref Character character) {
      if (IsModEnabled.Value && !__instance.m_ballPrefab.GetComponent<DemisterBallControl>()) {

      }
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(SE_Demister.UpdateStatusEffect))]
    static void UpdateStatusEffectPrefix(ref SE_Demister __instance, ref bool __state) {
      if (IsModEnabled.Value) {
        __state = __instance.m_ballInstance;
      }
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(SE_Demister.UpdateStatusEffect))]
    static void UpdateStatusEffectPostfix(ref SE_Demister __instance, ref bool __state) {
      if (IsModEnabled.Value && __instance.m_ballInstance) {
        if (!__state && !__instance.m_ballInstance.TryGetComponent(out DemisterBallControl _)) {
          ZLog.Log($"Adding DemisterBallControl to m_ballInstance.");
          DemisterBallControl demisterBallControl = __instance.m_ballInstance.AddComponent<DemisterBallControl>();

          if (__instance.m_character == Player.m_localPlayer) {
            LocalPlayerDemisterBall = demisterBallControl;
            LocalPlayerDemisterBallNetView = __instance.m_ballInstance.GetComponent<ZNetView>();

            ZLog.Log($"Setting DemisterBallControl to local config.");
            UpdateLocalPlayerDemisterBall();
          }

          demisterBallControl.UpdateDemisterBall(forceUpdate: true);
        }

        if (DemisterBallLockPosition.Value && __instance.m_character) {
          __instance.m_ballInstance.transform.position =
              __instance.m_character.m_head.position + DemisterBallLockOffset.Value;
        }
      }
    }
  }
}
