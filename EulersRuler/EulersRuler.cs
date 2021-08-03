using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System.Linq;
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

    private static ManualLogSource _logger;
    private Harmony _harmony;

    void Awake() {
      _isModEnabled = Config.Bind("_Global", "isModEnabled", true, "Globally enable or disable this mod.");

      _logger = Logger;
      _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
    }

    void OnDestroy() {
      _harmony?.UnpatchSelf();
    }

    private static GameObject _hudPanel;

    private static Text _healthTextLabel;
    private static Text _healthTextValue;
    private static Text _integrityTextLabel;
    private static Text _integrityTextValue;
    private static Text _rotationTextLabel;
    private static Text _rotationTextValue;

    private static Text _ghostNameTextLabel;
    private static Text _ghostNameTextValue;
    private static Text _ghostRotationTextLabel;
    private static Text _ghostRotationTextValue;

    private static readonly int _healthHashCode = "health".GetStableHashCode();

    [HarmonyPatch(typeof(Hud))]
    class HudPatch {

      [HarmonyPostfix]
      [HarmonyPatch(nameof(Hud.Awake))]
      static void AwakePostfix(ref Hud __instance) {
        CreateHudPanel(__instance);
      }

      [HarmonyPostfix]
      [HarmonyPatch(nameof(Hud.UpdateCrosshair))]
      static void UpdateCrosshairPostfix(ref Hud __instance, Player player) {
        if (!_isModEnabled.Value) {
          return;
        }

        if (player.m_hoveringPiece && player.m_hoveringPiece.TryGetComponent(out WearNTear wearNTear)) {
          SetActive(_healthTextLabel, true);
          SetActive(_healthTextValue, true);
          SetActive(_integrityTextLabel, true);
          SetActive(_integrityTextValue, true);
          SetActive(_rotationTextLabel, true);
          SetActive(_rotationTextValue, true);

          UpdateHoverPieceProperties(wearNTear);
        } else {
          SetActive(_healthTextLabel, false);
          SetActive(_healthTextValue, false);
          SetActive(_integrityTextLabel, false);
          SetActive(_integrityTextValue, false);
          SetActive(_rotationTextLabel, false);
          SetActive(_rotationTextValue, false);
        }

        if (player.m_placementGhost) {
          SetActive(_ghostNameTextLabel, true);
          SetActive(_ghostNameTextValue, true);
          SetActive(_ghostRotationTextLabel, true);
          SetActive(_ghostRotationTextValue, true);

          UpdatePlacementGhostProperties(player.m_placementGhost);
        } else {
          SetActive(_ghostNameTextLabel, false);
          SetActive(_ghostNameTextValue, false);
          SetActive(_ghostRotationTextLabel, false);
          SetActive(_ghostRotationTextValue, false);
        }
      }
    }

    private static void SetActive(Text text, bool active) {
      if (text.gameObject.activeSelf != active) {
        text.gameObject.SetActive(active);
      }
    }

    private static void UpdateHoverPieceProperties(WearNTear wearNTear) {
      float health = wearNTear.m_nview.m_zdo.GetFloat(_healthHashCode, wearNTear.m_health);

      _healthTextValue.text =
          string.Format(
              "<color={0}>{1}</color>/<color={2}>{3}</color> (<color={4}>{5:P2}</color>)",
              "#9CCC65",
              health,
              "#66BB6A",
              wearNTear.m_health,
              "#D4E157",
              Mathf.Clamp01(health / wearNTear.m_health));

      _integrityTextValue.text =
          string.Format(
            "<color={0}>{1:N0}</color>/<color={2}>{3:N0}</color>",
            "#29B6F6",
            wearNTear.GetSupport(),
            "#42A5F5",
            wearNTear.GetMaxSupport());

      _rotationTextValue.text = $"{wearNTear.transform.rotation.eulerAngles}";
    }

    private static void UpdatePlacementGhostProperties(GameObject placementGhost) {
      _ghostNameTextValue.text = $"<color=#FFCA28>{placementGhost.name}</color>";
      _ghostRotationTextValue.text = $"{placementGhost.transform.rotation.eulerAngles}";
    }

    class TwoColumnPanel {

    }

    static void CreateHudPanel(Hud hud) {
      _hudPanel = new("EulersRulerRoot", typeof(RectTransform));
      _hudPanel.transform.SetParent(hud.m_crosshair.transform);

      RectTransform transform = _hudPanel.GetComponent<RectTransform>();
      transform.anchorMin = new Vector2(1, 0);
      transform.anchorMax = new Vector2(1, 0);
      transform.pivot = Vector2.zero;
      transform.anchoredPosition = new Vector2(100, 100);

      HorizontalLayoutGroup panelLayout = _hudPanel.AddComponent<HorizontalLayoutGroup>();
      panelLayout.spacing = 8f;

      ContentSizeFitter panelFitter = _hudPanel.AddComponent<ContentSizeFitter>();
      panelFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
      panelFitter.verticalFit = ContentSizeFitter.FitMode.MinSize;

      GameObject leftColumn = new("LeftColumn", typeof(RectTransform));
      leftColumn.transform.SetParent(_hudPanel.transform);

      VerticalLayoutGroup leftLayout = leftColumn.AddComponent<VerticalLayoutGroup>();
      leftLayout.childControlWidth = true;
      leftLayout.childControlHeight = true;
      leftLayout.childForceExpandWidth = true;
      leftLayout.childForceExpandHeight = false;
      leftLayout.childAlignment = TextAnchor.LowerLeft;
      leftLayout.spacing = 5f;

      ContentSizeFitter leftFitter = leftColumn.AddComponent<ContentSizeFitter>();
      leftFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
      leftFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

      GameObject rightColumn = new("RightColumn", typeof(RectTransform));
      rightColumn.transform.SetParent(_hudPanel.transform);

      VerticalLayoutGroup rightLayout = rightColumn.AddComponent<VerticalLayoutGroup>();
      rightLayout.childControlWidth = true;
      rightLayout.childControlHeight = true;
      rightLayout.childForceExpandWidth = true;
      rightLayout.childForceExpandHeight = false;
      rightLayout.childAlignment = TextAnchor.LowerRight;
      rightLayout.spacing = 5f;

      ContentSizeFitter rightFitter = rightColumn.AddComponent<ContentSizeFitter>();
      rightFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
      rightFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

      AddPanelRow(hud, leftColumn, rightColumn, out _healthTextLabel, out _healthTextValue);
      _healthTextLabel.text = "\u2661";

      AddPanelRow(hud, leftColumn, rightColumn, out _integrityTextLabel, out _integrityTextValue);
      _integrityTextLabel.text = "\u2616";

      AddPanelRow(hud, leftColumn, rightColumn, out _rotationTextLabel, out _rotationTextValue);
      _rotationTextLabel.text = "\u29bf";

      AddPanelRow(hud, leftColumn, rightColumn, out _ghostNameTextLabel, out _ghostNameTextValue);
      _ghostNameTextLabel.text = "Name";

      AddPanelRow(hud, leftColumn, rightColumn, out _ghostRotationTextLabel, out _ghostRotationTextValue);
      _ghostRotationTextLabel.text = "\u29bf";

      _hudPanel.SetActive(true);
    }

    static void AddPanelRow(
        Hud hud, GameObject leftColumn, GameObject rightColumn, out Text leftText, out Text rightText) {
      GameObject leftSide = new("LeftSide", typeof(RectTransform));
      leftSide.transform.SetParent(leftColumn.transform);

      leftText = leftSide.AddComponent<Text>();
      leftText.alignment = TextAnchor.MiddleRight;
      leftText.horizontalOverflow = HorizontalWrapMode.Overflow;
      leftText.font = hud.m_hoverName.font;
      leftText.fontSize = 18;
      leftText.text = "LeftSide";

      Outline leftOutline = leftSide.AddComponent<Outline>();
      leftOutline.effectColor = Color.black;
      leftOutline.effectDistance = new Vector2(1, -1);

      GameObject rightSide = new("RightSide", typeof(RectTransform));
      rightSide.transform.SetParent(rightColumn.transform);

      rightText = rightSide.AddComponent<Text>();
      rightText.alignment = TextAnchor.MiddleLeft;
      rightText.horizontalOverflow = HorizontalWrapMode.Wrap;
      rightText.font = hud.m_hoverName.font;
      rightText.fontSize = 18;
      rightText.text = "RightSide";

      Outline rightOutline = rightSide.AddComponent<Outline>();
      rightOutline.effectColor = Color.black;
      rightOutline.effectDistance = new Vector2(1, -1);
    }
  }
}