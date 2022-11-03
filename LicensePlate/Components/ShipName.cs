using UnityEngine;
using UnityEngine.UI;

using static LicensePlate.LicensePlate;
using static LicensePlate.PluginConfig;

namespace LicensePlate {
  public class ShipName : MonoBehaviour, TextReceiver {
    private ZNetView _netView;
    private Ship _ship;
    private Chat.NpcText _npcText;

    private string _shipName = string.Empty;
    private string _shipNameCache = string.Empty;

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
      if (!_netView || !_netView.IsValid() || !_ship || !IsModEnabled.Value || !ShowShipNames.Value) {
        ClearNpcText();
        CancelInvoke(nameof(UpdateShipName));
        return;
      }

      _shipName = _netView.m_zdo.GetString(ShipLicensePlateHashCode, string.Empty);

      if (_npcText?.m_gui && _shipName.Length > 0) {
        UpdateNpcTextValue(_shipName);
      } else {
        ClearNpcText();

        if (_shipName.Length > 0
            && Player.m_localPlayer
            && Vector3.Distance(Player.m_localPlayer.transform.position, gameObject.transform.position)
                < ShipNameCutoffDistance.Value) {
          SetNpcText(_shipName);
        }
      }
    }

    private void ClearNpcText() {
      if (_npcText != null) {
        Chat.m_instance.ClearNpcText(_npcText);
        _npcText = null;
      }
    }

    private void SetNpcText(string shipName) {
      Chat.m_instance.SetNpcText(
          _ship.gameObject,
          ShipNameDisplayOffset.Value,
          ShipNameCutoffDistance.Value,
          600f,
          string.Empty,
          GetSanitizedShipName(shipName),
          false); ;

      _shipNameCache = shipName;
      _npcText = Chat.m_instance.FindNpcText(_ship.gameObject);

      if (_npcText?.m_gui) {
        CustomizeNpcText();
      }
    }

    public void UpdateNpcTextValue(string shipName) {
      if (shipName == _shipNameCache) {
        return;
      }

      _shipNameCache = shipName;
      _npcText.m_textField.text = GetSanitizedShipName(shipName);
    }

    private string GetSanitizedShipName(string shipName) {
      if (shipName.Length > 64) {
        shipName = shipName.Substring(0, 64);
      }

      if (ShipNameStripHtmlTags.Value) {
        shipName = HtmlTagsRegex.Replace(shipName, string.Empty);
      }

      return shipName;
    }

    private void CustomizeNpcText() {
      _npcText.m_textField.resizeTextForBestFit = false;
      _npcText.m_textField.horizontalOverflow = HorizontalWrapMode.Overflow;
      _npcText.m_textField.verticalOverflow = VerticalWrapMode.Overflow;
      _npcText.m_textField.fontSize = ShipNameFontSize.Value;

      Destroy(_npcText.m_textField.GetComponent<Outline>());

      Shadow textShadow = _npcText.m_textField.gameObject.AddComponent<Shadow>();
      textShadow.effectDistance = new(2f, -2f);
      textShadow.effectColor = Color.black;

      CustomizeNpcTextBackground(_npcText.m_gui.transform.Find("Image").gameObject);
    }

    private void CustomizeNpcTextBackground(GameObject background) {
      RectTransform rectTransform = background.GetComponent<RectTransform>();
      rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 60f);

      Image image = background.GetComponent<Image>();
      Color color = image.color;
      color.a = 0.5f;

      image.color = color;
    }

    public string GetText() {
      return _netView && _netView.IsValid()
          ? _netView.m_zdo.GetString(ShipLicensePlateHashCode, string.Empty)
          : string.Empty;
    }

    public void SetText(string text) {
      if (_netView && _netView.IsValid() && Player.m_localPlayer) {
        ZLog.Log($"Setting Ship ({_netView.m_zdo.m_uid}) name to: {text}");
        _netView.m_zdo.Set(ShipLicensePlateHashCode, text);
        _netView.m_zdo.Set(LicensePlateLastSetByHashCode, Player.m_localPlayer.GetPlayerID());

        UpdateShipName();
      }
    }
  }
}
