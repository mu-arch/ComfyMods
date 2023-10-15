using ComfyLib;

using HarmonyLib;

using static ColorfulPortals.PluginConfig;

namespace ColorfulPortals {
  [HarmonyPatch(typeof(TeleportWorld))]
  static class TeleportWorldPatch {
    static readonly int _portalWoodHashCode = "portal_wood".GetStableHashCode();

    [HarmonyPostfix]
    [HarmonyPatch(nameof(TeleportWorld.Awake))]
    static void AwakePostfix(ref TeleportWorld __instance) {
      if (IsModEnabled.Value
          && __instance
          && __instance.m_nview
          && __instance.m_nview.IsValid()
          && __instance.m_nview.m_zdo.m_prefab == _portalWoodHashCode) {
        __instance.gameObject.AddComponent<TeleportWorldColor>();
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
              "{0}\n[<color={1}>{2}</color>] Change color to: <color=#{3}>#{3}</color>",
              __result,
              "#FFA726",
              ChangePortalColorShortcut.Value,
              TargetPortalColor.Value.GetColorHtmlString());
    }
  }
}
