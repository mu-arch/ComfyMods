using ComfyLib;

using HarmonyLib;

using System;

using UnityEngine;

using static AlaCarte.PluginConfig;

namespace AlaCarte {
  [HarmonyPatch(typeof(Menu))]
  static class MenuPatch {
    static RectTransform _menuDialogVanilla;
    static RectTransform _menuDialogOld;

    [HarmonyPostfix]
    [HarmonyPatch(nameof(Menu.Start))]
    static void StartPostfix(ref Menu __instance) {
      _menuDialogVanilla = __instance.m_menuDialog.RectTransform();
      _menuDialogOld = __instance.m_root.Find("OLD_menu").RectTransform();

      SetupMenuDialogVanilla(_menuDialogVanilla);
      SetupMenuDialogOld(_menuDialogOld);
    }

    static void SetupMenuDialogVanilla(RectTransform menuTransform) {
      menuTransform.Find("darken").Ref()?.Image()
          .SetRaycastTarget(false);

      PanelDragger dragger = menuTransform.Find("ornament").Ref()?.gameObject.AddComponent<PanelDragger>();
      dragger.TargetRectTransform = menuTransform;
      dragger.OnPanelEndDrag += (_, position) => MenuDialogPosition.Value = position;
    }

    static void SetupMenuDialogOld(RectTransform menuTransform) {
      PanelDragger dragger = menuTransform.gameObject.AddComponent<PanelDragger>();
      dragger.TargetRectTransform = menuTransform;
      dragger.OnPanelEndDrag += (_, position) => MenuDialogPosition.Value = position;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Menu.Show))]
    static void ShowPrefix(ref Menu __instance) {
      RectTransform menuDialogByType =
          MenuDialogType.Value switch {
            DialogType.Vanilla => _menuDialogVanilla,
            DialogType.Old => _menuDialogOld,
            _ => throw new NotImplementedException(),
          };

      if (__instance.m_menuDialog != menuDialogByType) {
        __instance.m_menuDialog.gameObject.SetActive(false);
        __instance.m_menuDialog = menuDialogByType;
      }

      menuDialogByType.SetPosition(MenuDialogPosition.Value);
    }
  }
}
