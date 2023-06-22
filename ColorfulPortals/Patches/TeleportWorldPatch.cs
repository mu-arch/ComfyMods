using HarmonyLib;

using UnityEngine;

using static ColorfulPortals.PluginConfig;

namespace ColorfulPortals {
  [HarmonyPatch(typeof(TeleportWorld))]
  static class TeleportWorldPatch {
    [HarmonyPostfix]
    [HarmonyPatch(nameof(TeleportWorld.Awake))]
    static void AwakePostfix(ref TeleportWorld __instance) {
      if (!IsModEnabled.Value || !__instance) {
        return;
      }

      if (__instance.m_proximityRoot) {
        __instance.gameObject.AddComponent<TeleportWorldColor>();
      } else {
        // Stone 'portal' prefab does not set this property.
        __instance.m_proximityRoot = __instance.transform;
      }

      // Stone 'portal' prefab does not set this property.
      if (!__instance.m_target_found) {
        // The prefab does not have '_target_found_red' but instead '_target_found'.
        GameObject targetFoundObject = __instance.transform.Find("_target_found").gameObject;

        // Disable the GameObject first, as adding component EffectFade calls its Awake() before being attached.
        targetFoundObject.SetActive(false);
        __instance.m_target_found = targetFoundObject.AddComponent<EffectFade>();
        targetFoundObject.SetActive(true);
      }
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(TeleportWorld.GetHoverText))]
    static void GetHoverTextPostfix(ref TeleportWorld __instance, ref string __result) {
      if (!IsModEnabled.Value || !ShowChangeColorHoverText.Value || !__instance) {
        return;
      }

      __result =
          string.Format(
              "{0}\n[<color={1}>{2}</color>] Change color to: <color={3}>{3}</color>",
              __result,
              "#FFA726",
              ChangePortalColorShortcut.Value,
              TargetPortalColorHex.Value);
    }
  }
}
