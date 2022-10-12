using HarmonyLib;

using UnityEngine;

using static LicensePlate.LicensePlate;
using static LicensePlate.PluginConfig;

namespace LicensePlate {
  [HarmonyPatch(typeof(Vagon))]
  static class VagonPatch {
    [HarmonyPostfix]
    [HarmonyPatch(nameof(Vagon.Awake))]
    static void AwakePostfix(ref Vagon __instance) {
      if (IsModEnabled.Value) {
        __instance.gameObject.AddComponent<VagonName>();
      }
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Vagon.Interact))]
    static bool InteractPrefix(ref Vagon __instance, ref bool __result, bool hold, bool alt) {
      if (alt && IsModEnabled.Value) {
        if (__instance.m_nview
            && __instance.m_nview.IsValid()
            && __instance.m_nview.IsOwner()
            && __instance.TryGetComponent(out VagonName vagonName)) {
          TextInput.m_instance.RequestText(vagonName, "$hud_rename", 50);

          __result = true;
          return false;
        }
      }

      return true;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(Vagon.GetHoverText))]
    static void GetHoverTextPostfix(ref Vagon __instance, ref string __result) {
      if (IsModEnabled.Value && __instance.m_nview && __instance.m_nview.IsValid()) {
        __result +=
            Localization.m_instance.Localize("\n[<color=yellow><b>$KEY_AltPlace + $KEY_Use</b></color>] $hud_rename");
      }
    }

    public class VagonName : MonoBehaviour, TextReceiver {
      private ZNetView _netView;
      private Chat.NpcText _npcText;

      public void Awake() {
        _netView = GetComponent<ZNetView>();

        if (!_netView || !_netView.IsValid()) {
          return;
        }

        ZLog.Log($"VagonName awake for: {_netView.m_zdo.m_uid}");
        InvokeRepeating(nameof(UpdateVagonName), 0f, 2f);
      }

      private void UpdateVagonName() {
        if (!_netView || !_netView.IsValid()) {
          CancelInvoke(nameof(UpdateVagonName));
          return;
        }

        string vagonName = _netView.m_zdo.GetString(VagonNameHashCode, string.Empty);

        if (_npcText?.m_gui) {
          if (vagonName.Length > 0) {
            _npcText.m_textField.text = vagonName;
          } else {
            Chat.m_instance.ClearNpcText(_npcText);
            _npcText = null;
          }
        } else {
          if (_npcText != null) {
            Chat.m_instance.ClearNpcText(_npcText);
            _npcText = null;
          }

          if (vagonName.Length > 0
              && Player.m_localPlayer
              && Vector3.Distance(Player.m_localPlayer.transform.position, gameObject.transform.position) < 10f) {
            Chat.m_instance.SetNpcText(gameObject, Vector3.up * 1f, 10f, 600f, vagonName, string.Empty, false);
            _npcText = Chat.m_instance.FindNpcText(gameObject);

            if (_npcText?.m_gui) {
              SetupNpcTextUI(_npcText.m_gui);
            }
          }
        }
      }

      private void SetupNpcTextUI(GameObject gui) {
        RectTransform rectTransform = gui.transform.Find("Image").Ref()?.GetComponent<RectTransform>();
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 60f);
      }

      public string GetText() {
        return _netView && _netView.IsValid() 
            ? _netView.m_zdo.GetString(VagonNameHashCode, string.Empty)
            : string.Empty;
      }

      public void SetText(string text) {
        if (_netView && _netView.IsValid()) {
          ZLog.Log($"Setting Vagon ({_netView.m_zdo.m_uid}) name to: {text}");
          _netView.m_zdo.Set(VagonNameHashCode, text);

          UpdateVagonName();
        }
      }
    }
  }
}
