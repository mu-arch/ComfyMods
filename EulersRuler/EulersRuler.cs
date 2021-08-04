using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System;
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

    [Flags]
    private enum HoverPiecePanelRow {
      None        = 0,
      Name        = 1,
      Health      = 2,
      Stability   = 4,
      Euler       = 8,
      Quaternion  = 16,
    }

    [Flags]
    private enum PlacementGhostPanelRow {
      None        = 0,
      Name        = 1,
      Euler       = 2,
      Quaternion  = 4,
    }

    private static ConfigEntry<bool> _isModEnabled;

    private static ConfigEntry<Vector2> _hoverPiecePanelPosition;
    private static ConfigEntry<HoverPiecePanelRow> _hoverPiecePanelEnabledRows;

    private static ConfigEntry<Vector2> _placementGhostPanelPosition;
    private static ConfigEntry<PlacementGhostPanelRow> _placementGhostPanelEnabledRows;

    private static ManualLogSource _logger;
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
              "HoverPiecePanel",
              "hoverPiecePanelPosition",
              new Vector2(0, 225),
              "Position of the HoverPiece properties panel.");

      _hoverPiecePanelPosition.SettingChanged +=
          (sender, eventArgs) => _hoverPiecePanel.SetPosition(_hoverPiecePanelPosition.Value);

      _hoverPiecePanelEnabledRows =
          Config.Bind(
              "HoverPiecePanel",
              "hoverPiecePanelEnabledRows",
              HoverPiecePanelRow.Name | HoverPiecePanelRow.Health | HoverPiecePanelRow.Stability,
              "Which rows to display on the HoverPiece properties panel.");

      _placementGhostPanelPosition =
          Config.Bind(
              "PlacementGhostPanel",
              "placementGhostPanelPosition",
              new Vector2(100, 0),
              "Position of the PlacementGhost properties panel.");

      _placementGhostPanelPosition.SettingChanged +=
          (sender, eventArgs) => _placementGhostPanel.SetPosition(_placementGhostPanelPosition.Value);

      _placementGhostPanelEnabledRows =
          Config.Bind(
              "PlacementGhostPanel",
              "placementGhostPanelEnabledRows",
              (PlacementGhostPanelRow) Enum.GetValues(typeof(PlacementGhostPanelRow)).Cast<int>().Sum(),
              "Which rows to display on the PlacementGhost properties panel.");

      _logger = Logger;
      _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
    }

    void OnDestroy() {
      _harmony?.UnpatchSelf();
    }

    private static readonly Gradient _healthPercentGradient = CreateHealthPercentGradient();
    private static readonly Gradient _stabilityPercentGradient = CreateStabilityPercentGradient();

    private static readonly int _healthHashCode = "health".GetStableHashCode();

    private static TwoColumnPanel _hoverPiecePanel;
    private static Text _pieceNameTextLabel;
    private static Text _pieceNameTextValue;
    private static Text _pieceHealthTextLabel;
    private static Text _pieceHealthTextValue;
    private static Text _pieceStabilityTextLabel;
    private static Text _pieceStabilityTextValue;
    private static Text _pieceEulerTextLabel;
    private static Text _pieceEulerTextValue;
    private static Text _pieceQuaternionTextLabel;
    private static Text _pieceQuaternionTextValue;

    private static TwoColumnPanel _placementGhostPanel;
    private static Text _placementGhostNameTextLabel;
    private static Text _placementGhostNameTextValue;
    private static Text _placementGhostEulerTextLabel;
    private static Text _placementGhostEulerTextValue;
    private static Text _placementGhostQuaternionTextLabel;
    private static Text _placementGhostQuaternionTextValue;

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
          UpdateHoverPieceProperties(player.m_hoveringPiece, _hoverPiecePanelEnabledRows.Value);
          UpdatePlacementGhostProperties(player.m_placementGhost, _placementGhostPanelEnabledRows.Value);

          __instance.m_pieceHealthRoot.gameObject.SetActive(false);
        }
      }
    }

    private static void CreatePanels(Hud hud) {
      _hoverPiecePanel =
          new TwoColumnPanel(hud.m_crosshair.transform, hud.m_hoverName.font)
              .SetPosition(_hoverPiecePanelPosition.Value)
              .SetAnchors(new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1))
              .AddPanelRow(out _pieceNameTextLabel, out _pieceNameTextValue)
              .AddPanelRow(out _pieceHealthTextLabel, out _pieceHealthTextValue)
              .AddPanelRow(out _pieceStabilityTextLabel, out _pieceStabilityTextValue)
              .AddPanelRow(out _pieceEulerTextLabel, out _pieceEulerTextValue)
              .AddPanelRow(out _pieceQuaternionTextLabel, out _pieceQuaternionTextValue);

      _pieceNameTextLabel.text = "Piece \u25c8";
      _pieceHealthTextLabel.text = "Health \u2661";
      _pieceStabilityTextLabel.text = "Stability \u2616";
      _pieceEulerTextLabel.text = "Euler \u29bf";
      _pieceQuaternionTextLabel.text = "Quaternion \u2318";

      _placementGhostPanel =
          new TwoColumnPanel(hud.m_crosshair.transform, hud.m_hoverName.font)
              .SetPosition(_placementGhostPanelPosition.Value)
              .SetAnchors(new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f))
              .AddPanelRow(out _placementGhostNameTextLabel, out _placementGhostNameTextValue)
              .AddPanelRow(out _placementGhostEulerTextLabel, out _placementGhostEulerTextValue)
              .AddPanelRow(out _placementGhostQuaternionTextLabel, out _placementGhostQuaternionTextValue);

      _placementGhostNameTextLabel.text = "Placing \u25a5";
      _placementGhostEulerTextLabel.text = "Euler \u29bf";
      _placementGhostQuaternionTextLabel.text = "Quaternion \u2318";
    }

    private static void DestroyPanels() {
      _hoverPiecePanel?.DestroyPanel();
      _placementGhostPanel?.DestroyPanel();
    }

    private static void UpdateHoverPieceProperties(Piece piece, HoverPiecePanelRow enabledRows) {
      if (!piece || enabledRows == HoverPiecePanelRow.None || !piece.TryGetComponent(out WearNTear wearNTear)) {
        _hoverPiecePanel?.SetActive(false);
        return;
      }

      _hoverPiecePanel.SetActive(true);

      UpdateHoverPieceNameRow(piece, enabledRows.HasFlag(HoverPiecePanelRow.Name));
      UpdateHoverPieceHealthRow(wearNTear, enabledRows.HasFlag(HoverPiecePanelRow.Health));
      UpdateHoverPieceStabilityRow(wearNTear, enabledRows.HasFlag(HoverPiecePanelRow.Stability));
      UpdateHoverPieceEulerRow(wearNTear, enabledRows.HasFlag(HoverPiecePanelRow.Euler));
      UpdateHoverPieceQuaternionRow(wearNTear, enabledRows.HasFlag(HoverPiecePanelRow.Quaternion));
    }

    private static void UpdateHoverPieceNameRow(Piece piece, bool isRowEnabled) {
      _pieceNameTextLabel.gameObject.SetActive(isRowEnabled);
      _pieceNameTextValue.gameObject.SetActive(isRowEnabled);

      if (isRowEnabled) {
        _pieceNameTextValue.text = $"<color=#FFCA28>{Localization.instance.Localize(piece.m_name)}</color>";
      }
    }

    private static void UpdateHoverPieceHealthRow(WearNTear wearNTear, bool isRowEnabled) {
      _pieceHealthTextLabel.gameObject.SetActive(isRowEnabled);
      _pieceHealthTextValue.gameObject.SetActive(isRowEnabled);

      if (isRowEnabled) {
        float health = wearNTear.m_nview.m_zdo.GetFloat(_healthHashCode, wearNTear.m_health);
        float healthPercent = Mathf.Clamp01(health / wearNTear.m_health);

        _pieceHealthTextValue.text =
            string.Format(
                "<color={0}>{1}</color> /<color={2}>{3}</color> (<color=#{4}>{5:0%}</color>)",
                "#9CCC65",
                Mathf.Abs(health) > 1E9 ? health.ToString("g5") : health.ToString("N0"),
                "#FAFAFA",
                wearNTear.m_health,
                ColorUtility.ToHtmlStringRGB(_healthPercentGradient.Evaluate(healthPercent)),
                healthPercent);
      }
    }

    private static void UpdateHoverPieceStabilityRow(WearNTear wearNTear, bool isRowEnabled) {
      _pieceStabilityTextLabel.gameObject.SetActive(isRowEnabled);
      _pieceStabilityTextValue.gameObject.SetActive(isRowEnabled);

      if (isRowEnabled) {
        float support = wearNTear.GetSupport();
        float maxSupport = wearNTear.GetMaxSupport();
        float supportPrecent = Mathf.Clamp01(support / maxSupport);

        _pieceStabilityTextValue.text =
            string.Format(
              "<color={0}>{1:N0}</color> /<color={2}>{3}</color> (<color=#{4}>{5:0%}</color>)",
              "#64B5F6",
              support,
              "#FAFAFA",
              maxSupport,
              ColorUtility.ToHtmlStringRGB(_stabilityPercentGradient.Evaluate(supportPrecent)),
              supportPrecent);
      }
    }

    private static void UpdateHoverPieceEulerRow(WearNTear wearNTear, bool isRowEnabled) {
      _pieceEulerTextLabel.gameObject.SetActive(isRowEnabled);
      _pieceEulerTextValue.gameObject.SetActive(isRowEnabled);

      if (isRowEnabled) {
        _pieceEulerTextValue.text = $"<color=#CFD8DC>{wearNTear.transform.rotation.eulerAngles}</color>";
      }
    }

    private static void UpdateHoverPieceQuaternionRow(WearNTear wearNTear, bool isRowEnabled) {
      _pieceQuaternionTextLabel.gameObject.SetActive(isRowEnabled);
      _pieceQuaternionTextValue.gameObject.SetActive(isRowEnabled);

      if (isRowEnabled) {
        _pieceQuaternionTextValue.text = $"<color=#D7CCC8>{wearNTear.transform.rotation}</color>";
      }
    }

    private static void UpdatePlacementGhostProperties(GameObject placementGhost, PlacementGhostPanelRow enabledRows) {
      if (!placementGhost
          || enabledRows == PlacementGhostPanelRow.None
          || !placementGhost.TryGetComponent(out Piece piece)) {
        _placementGhostPanel?.SetActive(false);
        return;
      }

      _placementGhostPanel.SetActive(true);

      UpdatePlacementGhostNameRow(piece, enabledRows.HasFlag(PlacementGhostPanelRow.Name));
      UpdatePlacementGhostEulerRow(placementGhost, enabledRows.HasFlag(PlacementGhostPanelRow.Euler));
      UpdatePlacementGhostQuaternionRow(placementGhost, enabledRows.HasFlag(PlacementGhostPanelRow.Quaternion));
    }

    private static void UpdatePlacementGhostNameRow(Piece piece, bool isRowEnabled) {
      _placementGhostNameTextLabel.gameObject.SetActive(isRowEnabled);
      _placementGhostNameTextValue.gameObject.SetActive(isRowEnabled);

      if (isRowEnabled) {
        _placementGhostNameTextValue.text = $"<color=#FFCA28>{Localization.instance.Localize(piece.m_name)}</color>";
      }
    }

    private static void UpdatePlacementGhostEulerRow(GameObject placementGhost, bool isRowEnabled) {
      _placementGhostEulerTextLabel.gameObject.SetActive(isRowEnabled);
      _placementGhostEulerTextValue.gameObject.SetActive(isRowEnabled);

      if (isRowEnabled) {
        _placementGhostEulerTextValue.text = $"<color=#CFD8DC>{placementGhost.transform.rotation.eulerAngles}</color>";
      }
    }

    private static void UpdatePlacementGhostQuaternionRow(GameObject placementGhost, bool isRowEnabled) {
      _placementGhostQuaternionTextLabel.gameObject.SetActive(isRowEnabled);
      _placementGhostQuaternionTextValue.gameObject.SetActive(isRowEnabled);

      if (isRowEnabled) {
        _placementGhostQuaternionTextValue.text = $"<color=#D7CCC8>{placementGhost.transform.rotation}</color>";
      }
    }

    private static Gradient CreateHealthPercentGradient() {
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

    private static Gradient CreateStabilityPercentGradient() {
      Gradient gradient = new();

      gradient.SetKeys(
          new GradientColorKey[] {
            new GradientColorKey(new Color32(239, 83, 80, 255), 0f),
            new GradientColorKey(new Color32(255, 238, 88, 255), 0.5f),
            new GradientColorKey(new Color32(100, 181, 246, 255), 1f),
          },
          new GradientAlphaKey[] {
            new GradientAlphaKey(1f, 0f),
            new GradientAlphaKey(1f, 1f),
          });

      return gradient;
    }
  }
}