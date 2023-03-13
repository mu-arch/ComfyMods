using System;
using System.Linq;

using BepInEx.Configuration;

using ComfyLib;

using UnityEngine;

namespace EulersRuler {
  class PluginConfig {
    [Flags]
    public enum HoverPiecePanelRow {
      None = 0,
      Name = 1,
      Health = 2,
      Stability = 4,
      Euler = 8,
      Quaternion = 16,
    }

    [Flags]
    public enum PlacementGhostPanelRow {
      None = 0,
      Name = 1,
      Euler = 2,
      Quaternion = 4,
    }

    public static ConfigEntry<bool> _isModEnabled;

    public static ConfigEntry<Vector2> _hoverPiecePanelPosition;
    public static ConfigEntry<HoverPiecePanelRow> _hoverPiecePanelEnabledRows;
    public static ConfigEntry<int> _hoverPiecePanelFontSize;
    public static ConfigEntry<bool> _showHoverPieceHealthBar;

    public static ConfigEntry<Vector2> _placementGhostPanelPosition;
    public static ConfigEntry<PlacementGhostPanelRow> _placementGhostPanelEnabledRows;
    public static ConfigEntry<int> _placementGhostPanelFontSize;

    public static void CreateConfig(ConfigFile config) {
      _isModEnabled = config.BindInOrder("_Global", "isModEnabled", true, "Globally enable or disable this mod.");

      _hoverPiecePanelPosition =
          config.BindInOrder(
              "HoverPiecePanel",
              "hoverPiecePanelPosition",
              new Vector2(0, 225),
              "Position of the HoverPiece properties panel.");

      _hoverPiecePanelEnabledRows =
          config.BindInOrder(
              "HoverPiecePanel",
              "hoverPiecePanelEnabledRows",
              HoverPiecePanelRow.Name | HoverPiecePanelRow.Health | HoverPiecePanelRow.Stability,
              "Which rows to display on the HoverPiece properties panel.");

      _hoverPiecePanelFontSize =
          config.BindInOrder(
              "HoverPiecePanel",
              "hoverPiecePanelFontSize",
              18,
              "Font size for the HoverPiece properties panel.",
              new AcceptableValueRange<int>(6, 32));

      _showHoverPieceHealthBar =
          config.BindInOrder(
              "HoverPiecePanel", "showHoverPieceHealthBar", true, "Show the vanilla hover piece HealthBar.");

      _placementGhostPanelPosition =
          config.BindInOrder(
              "PlacementGhostPanel",
              "placementGhostPanelPosition",
              new Vector2(100, 0),
              "Position of the PlacementGhost properties panel.");

      _placementGhostPanelEnabledRows =
          config.BindInOrder(
              "PlacementGhostPanel",
              "placementGhostPanelEnabledRows",
              (PlacementGhostPanelRow) Enum.GetValues(typeof(PlacementGhostPanelRow)).Cast<int>().Sum(),
              "Which rows to display on the PlacementGhost properties panel.");

      _placementGhostPanelFontSize =
          config.BindInOrder(
              "PlacementGhostPanel",
              "placementGhostPanelFontSize",
              18,
              "Font size for the PlacementGhost properties panel.",
              new AcceptableValueRange<int>(6, 32));
    }
  }
}
