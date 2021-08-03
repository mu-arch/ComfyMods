using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace EulersRuler {
  [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
  public class EulersRuler : BaseUnityPlugin {
    public const string PluginGUID = "redseiko.valheim.eulersruler";
    public const string PluginName = "EulersRuler";
    public const string PluginVersion = "1.0.0";

    private static ConfigEntry<bool> _isModEnabled;

    private static ConfigEntry<Vector2> _hoverPiecePanelPosition;
    private static ConfigEntry<Vector2> _placementGhostPanelPosition;

    private Harmony _harmony;

    void Awake() {
      _isModEnabled = Config.Bind("_Global", "isModEnabled", true, "Globally enable or disable this mod.");

      _isModEnabled.SettingChanged += (sender, eventArgs) => {
        DestroyPanels();

        if (_isModEnabled.Value && Hud.instance) {
          CreatePanels(Hud.instance);
        }
      };

      _hoverPiecePanelPosition =
          Config.Bind(
              "Hud",
              "hoverPiecePanelPosition",
              new Vector2(-100, 200),
              "Position of the HoverPiece properties panel.");

      _hoverPiecePanelPosition.SettingChanged +=
          (sender, eventArgs) => _hoverPiecePanel.SetPosition(_hoverPiecePanelPosition.Value);

      _placementGhostPanelPosition =
          Config.Bind(
              "Hud",
              "placementGhostPanelPosition",
              new Vector2(100, 15),
              "Position of the PlacementGhost properties panel.");

      _placementGhostPanelPosition.SettingChanged +=
          (sender, eventArgs) => _placementGhostPanel.SetPosition(_placementGhostPanelPosition.Value);

      _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
    }

    void OnDestroy() {
      _harmony?.UnpatchSelf();
    }

    private static readonly Gradient _percentGradient = CreatePercentGradient();
    private static readonly int _healthHashCode = "health".GetStableHashCode();

    private static TwoColumnPanel _hoverPiecePanel;
    private static Text _pieceNameTextLabel;
    private static Text _pieceNameTextValue;
    private static Text _pieceHealthTextLabel;
    private static Text _pieceHealthTextValue;
    private static Text _pieceStabilityTextLabel;
    private static Text _pieceStabilityTextValue;
    private static Text _pieceRotationTextLabel;
    private static Text _pieceRotationTextValue;

    private static TwoColumnPanel _placementGhostPanel;
    private static Text _ghostNameTextLabel;
    private static Text _ghostNameTextValue;
    private static Text _ghostRotationTextLabel;
    private static Text _ghostRotationTextValue;

    [HarmonyPatch(typeof(Hud))]
    class HudPatch {

      [HarmonyPostfix]
      [HarmonyPatch(nameof(Hud.Awake))]
      static void AwakePostfix(ref Hud __instance) {
        if (_isModEnabled.Value) {
          CreatePanels(__instance);
        }
      }

      [HarmonyPostfix]
      [HarmonyPatch(nameof(Hud.UpdateCrosshair))]
      static void UpdateCrosshairPostfix(ref Hud __instance, Player player) {
        if (_isModEnabled.Value) {
          UpdateHoverPieceProperties(player.m_hoveringPiece);
          UpdatePlacementGhostProperties(player.m_placementGhost);

          __instance.m_pieceHealthRoot.gameObject.SetActive(false);
        }
      }
    }

    private static void CreatePanels(Hud hud) {
      _hoverPiecePanel =
          new TwoColumnPanel(hud.m_crosshair.transform, hud.m_hoverName.font)
              .SetPosition(_hoverPiecePanelPosition.Value)
              .AddPanelRow(out _pieceNameTextLabel, out _pieceNameTextValue)
              .AddPanelRow(out _pieceHealthTextLabel, out _pieceHealthTextValue)
              .AddPanelRow(out _pieceStabilityTextLabel, out _pieceStabilityTextValue)
              .AddPanelRow(out _pieceRotationTextLabel, out _pieceRotationTextValue);

      _pieceNameTextLabel.text = "Piece \u25c8";
      _pieceHealthTextLabel.text = "Health \u2661";
      _pieceStabilityTextLabel.text = "Stability \u2616";
      _pieceRotationTextLabel.text = "Rotation \u29bf";

      _placementGhostPanel =
          new TwoColumnPanel(hud.m_crosshair.transform, hud.m_hoverName.font)
              .SetPosition(_placementGhostPanelPosition.Value)
              .AddPanelRow(out _ghostNameTextLabel, out _ghostNameTextValue)
              .AddPanelRow(out _ghostRotationTextLabel, out _ghostRotationTextValue);

      _ghostNameTextLabel.text = "Placing \u25a5";
      _ghostRotationTextLabel.text = "Rotation \u29bf";
    }

    private static void DestroyPanels() {
      _hoverPiecePanel.DestroyPanel();
      _placementGhostPanel.DestroyPanel();
    }

    private static void UpdateHoverPieceProperties(Piece piece) {
      if (!piece || !piece.TryGetComponent(out WearNTear wearNTear)) {
        _hoverPiecePanel.SetActive(false);
        return;
      }

      _hoverPiecePanel.SetActive(true);

      _pieceNameTextValue.text = $"<color=#FFCA28>{Localization.instance.Localize(piece.m_name)}</color>";

      float health = wearNTear.m_nview.m_zdo.GetFloat(_healthHashCode, wearNTear.m_health);
      float healthPercent = Mathf.Clamp01(health / wearNTear.m_health);

      _pieceHealthTextValue.text =
          string.Format(
              "<color={0}>{1:N0}</color> /<color={2}>{3}</color> (<color=#{4}>{5:P0}</color>)",
              "#9CCC65",
              health,
              "#FAFAFA",
              wearNTear.m_health,
              ColorUtility.ToHtmlStringRGB(_percentGradient.Evaluate(healthPercent)),
              healthPercent);

      float support = wearNTear.GetSupport();
      float maxSupport = wearNTear.GetMaxSupport();
      float supportPrecent = Mathf.Clamp01(support / maxSupport);

      _pieceStabilityTextValue.text =
          string.Format(
            "<color={0}>{1:N0}</color> /<color={2}>{3}</color> (<color=#{4}>{5:P0}</color>)",
            "#4FC3F7",
            support,
            "#FAFAFA",
            maxSupport,
            ColorUtility.ToHtmlStringRGB(_percentGradient.Evaluate(supportPrecent)),
            supportPrecent);

      _pieceRotationTextValue.text = $"{wearNTear.transform.rotation.eulerAngles}";
    }

    private static void UpdatePlacementGhostProperties(GameObject placementGhost) {
      if (!placementGhost || !placementGhost.TryGetComponent(out Piece piece)) {
        _placementGhostPanel.SetActive(false);
        return;
      }

      _placementGhostPanel.SetActive(true);

      _ghostNameTextValue.text = $"<color=#FFCA28>{Localization.instance.Localize(piece.m_name)}</color>";
      _ghostRotationTextValue.text = $"{placementGhost.transform.rotation.eulerAngles}";
    }

    private static Gradient CreatePercentGradient() {
      Gradient gradient = new();

      gradient.SetKeys(
          new GradientColorKey[] {
            new GradientColorKey(new Color32(239, 83, 80, 255), 0f),
            new GradientColorKey(new Color32(255, 238, 88, 255), 0.5f),
            new GradientColorKey(new Color32(156, 204, 101, 255), 1f),
          },
          new GradientAlphaKey[] {
            new GradientAlphaKey(1f, 0f),
            new GradientAlphaKey(1f, 1f),
          });

      return gradient;
    }

    class TwoColumnPanel {
      private GameObject _panel = null;
      private GameObject _leftColumn = null;
      private GameObject _rightColumn = null;

      private readonly Font _font = null;

      public TwoColumnPanel(Transform parent, Font font) {
        CreatePanel(parent);
        _font = font;
      }

      public void DestroyPanel() {
        Destroy(_panel);
      }

      public void SetActive(bool active) {
        if (_panel.activeSelf != active) {
          _panel.SetActive(active);
        }
      }

      public TwoColumnPanel SetPosition(Vector2 position) {
        _panel.GetComponent<RectTransform>().anchoredPosition = position;
        return this;
      }

      void CreatePanel(Transform parent) {
        _panel = new("EulersRulerPanel", typeof(RectTransform));
        _panel.transform.SetParent(parent);

        RectTransform transform = _panel.GetComponent<RectTransform>();
        transform.anchorMin = new Vector2(1, 0);
        transform.anchorMax = new Vector2(1, 0);
        transform.pivot = Vector2.zero;
        transform.anchoredPosition = Vector2.zero;
        transform.localScale = Vector3.one;

        HorizontalLayoutGroup panelLayout = _panel.AddComponent<HorizontalLayoutGroup>();
        panelLayout.spacing = 8f;

        ContentSizeFitter panelFitter = _panel.AddComponent<ContentSizeFitter>();
        panelFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        panelFitter.verticalFit = ContentSizeFitter.FitMode.MinSize;

        _leftColumn = new("LeftColumn", typeof(RectTransform));
        _leftColumn.transform.SetParent(_panel.transform);

        VerticalLayoutGroup leftLayout = _leftColumn.AddComponent<VerticalLayoutGroup>();
        leftLayout.childControlWidth = true;
        leftLayout.childControlHeight = true;
        leftLayout.childForceExpandWidth = true;
        leftLayout.childForceExpandHeight = false;
        leftLayout.childAlignment = TextAnchor.LowerLeft;
        leftLayout.spacing = 5f;

        ContentSizeFitter leftFitter = _leftColumn.AddComponent<ContentSizeFitter>();
        leftFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        leftFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        _rightColumn = new("RightColumn", typeof(RectTransform));
        _rightColumn.transform.SetParent(_panel.transform);

        VerticalLayoutGroup rightLayout = _rightColumn.AddComponent<VerticalLayoutGroup>();
        rightLayout.childControlWidth = true;
        rightLayout.childControlHeight = true;
        rightLayout.childForceExpandWidth = true;
        rightLayout.childForceExpandHeight = false;
        rightLayout.childAlignment = TextAnchor.LowerRight;
        rightLayout.spacing = 5f;

        ContentSizeFitter rightFitter = _rightColumn.AddComponent<ContentSizeFitter>();
        rightFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        rightFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
      }

      public TwoColumnPanel AddPanelRow(out Text leftText, out Text rightText) {
        GameObject leftSide = new("Label", typeof(RectTransform));
        leftSide.transform.SetParent(_leftColumn.transform);

        leftText = leftSide.AddComponent<Text>();
        leftText.alignment = TextAnchor.MiddleRight;
        leftText.horizontalOverflow = HorizontalWrapMode.Overflow;
        leftText.font = _font;
        leftText.fontSize = 18;
        leftText.text = "Label";

        Outline leftOutline = leftSide.AddComponent<Outline>();
        leftOutline.effectColor = Color.black;
        leftOutline.effectDistance = new Vector2(1, -1);

        GameObject rightSide = new("Value", typeof(RectTransform));
        rightSide.transform.SetParent(_rightColumn.transform);

        rightText = rightSide.AddComponent<Text>();
        rightText.alignment = TextAnchor.MiddleLeft;
        rightText.horizontalOverflow = HorizontalWrapMode.Wrap;
        rightText.font = _font;
        rightText.fontSize = 18;
        rightText.text = "Value";

        Outline rightOutline = rightSide.AddComponent<Outline>();
        rightOutline.effectColor = Color.black;
        rightOutline.effectDistance = new Vector2(1, -1);

        return this;
      }
    }
  }
}