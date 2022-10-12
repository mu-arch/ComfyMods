using UnityEngine;

using static LicensePlate.LicensePlate;

namespace LicensePlate {
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
          Chat.m_instance.SetNpcText(gameObject, Vector3.up * 1f, 10f, 600f, string.Empty, vagonName, false);
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
