using UnityEngine;
using UnityEngine.UI;

using static LicensePlate.LicensePlate;
using static LicensePlate.PluginConfig;

namespace LicensePlate {
  public class VagonName : MonoBehaviour, TextReceiver {
    private ZNetView _netView;
    private Chat.NpcText _npcText;

    private string _vagonName = string.Empty;
    private string _vagonNameCache = string.Empty;

    public void Awake() {
      _netView = GetComponent<ZNetView>();

      if (!_netView || !_netView.IsValid()) {
        return;
      }

      ZLog.Log($"VagonName awake for: {_netView.m_zdo.m_uid}");
      InvokeRepeating(nameof(UpdateVagonName), 0f, 2f);
    }

    private void UpdateVagonName() {
      if (!_netView || !_netView.IsValid() || !IsModEnabled.Value || !ShowCartNames.Value) {
        ClearNpcText();
        CancelInvoke(nameof(UpdateVagonName));
        return;
      }

      _vagonName = _netView.m_zdo.GetString(VagonLicensePlateHashCode, string.Empty);

      if (_npcText?.m_gui && _vagonName.Length > 0) {
        UpdateNpcTextValue(_vagonName);
      } else {
        ClearNpcText();

        if (_vagonName.Length > 0
            && Player.m_localPlayer
            && Vector3.Distance(Player.m_localPlayer.transform.position, gameObject.transform.position)
                < CartNameCutoffDistance.Value) {
          SetNpcText(_vagonName);
        }
      }
    }

    private void ClearNpcText() {
      if (_npcText != null) {
        Chat.m_instance.ClearNpcText(_npcText);
        _npcText = null;
      }

      _vagonNameCache = string.Empty;
    }

    private void SetNpcText(string vagonName) {
      Chat.m_instance.SetNpcText(
          gameObject,
          CartNameDisplayOffset.Value,
          CartNameCutoffDistance.Value,
          600f,
          string.Empty,
          GetSanitizedVagonName(vagonName),
          false);

      _vagonNameCache = vagonName;
      _npcText = Chat.m_instance.FindNpcText(gameObject);

      if (_npcText?.m_gui) {
        CustomizeNpcText();
      }
    }

    private void UpdateNpcTextValue(string vagonName) {
      if (vagonName == _vagonNameCache) {
        return;
      }

      _vagonNameCache = vagonName;
      _npcText.m_textField.text = GetSanitizedVagonName(vagonName);
    }

    private string GetSanitizedVagonName(string vagonName) {
      if (vagonName.Length > 64) {
        vagonName = vagonName.Substring(0, 64);
      }

      if (CartNameStripHtmlTags.Value) {
        vagonName = HtmlTagsRegex.Replace(vagonName, string.Empty);
      }

      return vagonName;
    }

    private void CustomizeNpcText() {
      _npcText.m_textField.resizeTextForBestFit = false;
      _npcText.m_textField.horizontalOverflow = HorizontalWrapMode.Overflow;
      _npcText.m_textField.verticalOverflow = VerticalWrapMode.Overflow;
      _npcText.m_textField.fontSize = CartNameFontSize.Value;

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
          ? _netView.m_zdo.GetString(VagonLicensePlateHashCode, string.Empty)
          : string.Empty;
    }

    public void SetText(string text) {
      if (_netView && _netView.IsValid() && Player.m_localPlayer) {
        ZLog.Log($"Setting Vagon ({_netView.m_zdo.m_uid}) name to: {text}");
        _netView.m_zdo.Set(VagonLicensePlateHashCode, text);
        _netView.m_zdo.Set(LicensePlateLastSetByHashCode, Player.m_localPlayer.GetPlayerID());

        UpdateVagonName();
      }
    }
  }
}
