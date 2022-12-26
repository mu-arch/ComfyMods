using System.Linq;
using System.Reflection;

using BepInEx;

using ComfyLib;

using HarmonyLib;

using UnityEngine;
using UnityEngine.UI;

using static SearsCatalog.PluginConfig;

namespace SearsCatalog {
  [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
  public class SearsCatalog : BaseUnityPlugin {
    public const string PluginGuid = "redseiko.valheim.searscatalog";
    public const string PluginName = "SearsCatalog";
    public const string PluginVersion = "1.0.0";

    Harmony _harmony;

    public void Awake() {
      BindConfig(Config);

      _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), harmonyInstanceId: PluginGuid);
    }

    public void OnDestroy() {
      _harmony?.UnpatchSelf();
    }

    public static int BuildHudColumns { get; set; } = 13;
    public static int BuildHudRows { get; set; } = 0;

    public static bool BuildHudNeedRefresh { get; set; } = false;
    public static bool BuildHudNeedIconLayoutRefresh { get; set; } = false;
    public static bool BuildHudNeedIconRecenter { get; set; } = false;

    public static RectTransform BuildHudPanelTransform { get; set; }
    public static Scrollbar BuildHudScrollbar { get; set; }
    public static ScrollRect BuildHudScrollRect { get; set; }

    public static void SetupBuildHudPanel() {
      if (Hud.m_instance && BuildHudPanelTransform) {
        BuildHudColumns = BuildHudPanelColumns.Value;
        BuildHudRows = 0;
         
        float spacing = Hud.m_instance.m_pieceIconSpacing;

        BuildHudPanelTransform.SetSizeDelta(
            new(BuildHudColumns * spacing + 35f, BuildHudPanelRows.Value * spacing + 70f));

        BuildHudNeedRefresh = true;

        SetupPieceSelectionWindow();
      }
    }

    static RectTransform _tabBorderRectTransform;
    static RectTransform _inputHelpLeftRectTransform;
    static RectTransform _inputHelpRightRectTransform;

    public static void SetupPieceSelectionWindow() {
      if (!Hud.m_instance || !BuildHudPanelTransform) {
        return;
      }

      float width = BuildHudPanelTransform.sizeDelta.x;
      float height = BuildHudPanelTransform.sizeDelta.y;

      Hud.m_instance.m_pieceCategoryRoot.Ref()?
          .GetComponent<RectTransform>()
          .SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width + CategoryRootSizeWidthOffset.Value);

      if (!_tabBorderRectTransform) {
        _tabBorderRectTransform =
            Hud.m_instance.m_pieceSelectionWindow.transform
                .Find("TabBorder").Ref()?
                .GetComponent<RectTransform>();
      }

      _tabBorderRectTransform.Ref()?
          .SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width + TabBorderSizeWidthOffset.Value);

      float xOffset = width + InputHelpSizeDeltaOffset.Value.x;
      float yOffset = (height / 2f) + InputHelpSizeDeltaOffset.Value.y;

      if (!_inputHelpLeftRectTransform) {
        _inputHelpLeftRectTransform =
             Hud.m_instance.m_pieceSelectionWindow.transform
                .Find("InputHelp/MK hints/Left").Ref()?
                .GetComponent<RectTransform>();
      }

      _inputHelpLeftRectTransform.Ref()?.SetPosition(new(xOffset / -2f, yOffset));

      if (!_inputHelpRightRectTransform) {
        _inputHelpRightRectTransform =
            Hud.m_instance.m_pieceSelectionWindow.transform
                .Find("InputHelp/MK hints/Right").Ref()?
                .GetComponent<RectTransform>();
      }

      _inputHelpRightRectTransform.Ref()?.SetPosition(new(xOffset / 2f, yOffset));
    }

    public static void CenterOnSelectedIndex() {
      if (!Player.m_localPlayer.Ref()?.m_buildPieces || !Hud.m_instance) {
        return;
      }

      Vector2Int gridIndex = Player.m_localPlayer.m_buildPieces.GetSelectedIndex();
      int index = (BuildHudColumns * gridIndex.y) + gridIndex.x;

      if (index >= Hud.m_instance.m_pieceIcons.Count) {
        return;
      }

      Hud.PieceIconData pieceIcon = Hud.m_instance.m_pieceIcons[index];

      if (!pieceIcon.m_go) {
        return;
      }

      BuildHudScrollRect.EnsureVisibility(pieceIcon.m_go.GetComponent<RectTransform>());
    }
  }
}