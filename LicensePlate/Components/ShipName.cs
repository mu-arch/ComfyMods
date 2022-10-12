using UnityEngine;

using static LicensePlate.LicensePlate;
using static LicensePlate.PluginConfig;

namespace LicensePlate {
  public class ShipName : MonoBehaviour, TextReceiver {
    private ZNetView _netView;
    private Ship _ship;
    private Chat.NpcText _npcText;

    public void Awake() {
      ShipControlls shipControls = GetComponent<ShipControlls>();
      _netView = shipControls.Ref()?.m_nview;
      _ship = shipControls.Ref()?.m_ship;

      if (!_netView || !_netView.IsValid() || !_ship) {
        return;
      }

      ZLog.Log($"ShipName awake for: {_netView.m_zdo.m_uid}");
      InvokeRepeating(nameof(UpdateShipName), 0f, 2f);
    }

    private void UpdateShipName() {
      if (!_netView || !_netView.IsValid()) {
        CancelInvoke(nameof(UpdateShipName));
        return;
      }

      string shipName = _netView.m_zdo.GetString(ShipNameHashCode, string.Empty);

      if (_npcText?.m_gui) {
        if (shipName.Length > 0) {
          _npcText.m_textField.text = shipName;
        } else {
          Chat.m_instance.ClearNpcText(_npcText);
          _npcText = null;
        }
      } else {
        if (_npcText != null) {
          Chat.m_instance.ClearNpcText(_npcText);
          _npcText = null;
        }

        float cutoff = ShipNameCutoffDistance.Value;

        if (shipName.Length > 0
            && Player.m_localPlayer
            && Vector3.Distance(Player.m_localPlayer.transform.position, gameObject.transform.position) < cutoff) {
          Chat.m_instance.SetNpcText(_ship.gameObject, Vector3.up * 1f, cutoff, 600f, string.Empty, shipName, false);
          _npcText = Chat.m_instance.FindNpcText(_ship.gameObject);

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
          ? _netView.m_zdo.GetString(ShipNameHashCode, string.Empty)
          : string.Empty;
    }

    public void SetText(string text) {
      if (_netView && _netView.IsValid()) {
        ZLog.Log($"Setting Ship ({_netView.m_zdo.m_uid}) name to: {text}");
        _netView.m_zdo.Set(ShipNameHashCode, text);
      }
    }
  }
}
