using System.Reflection;

using BepInEx;

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

    public static Scrollbar BuildHudScrollbar { get; set; }
    public static ScrollRect BuildHudScrollRect { get; set; }

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