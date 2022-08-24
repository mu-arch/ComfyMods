using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using BepInEx;

using ComfyLib;

using HarmonyLib;

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

    public static PieceListPanel PieceListPanel { get; private set; }

    public static void TogglePieceListPanel(bool toggleOn) {
      if (!PieceListPanel?.Panel) {
        PieceListPanel = new(Hud.m_instance.transform);

        PieceListPanel.Panel.RectTransform()
            .SetAnchorMin(new(0f, 0.5f))
            .SetAnchorMax(new(0f, 0.5f))
            .SetPivot(new(0f, 0.5f))
            .SetPosition(new(25f, 0f))
            .SetSizeDelta(new(300f, 400f));
      }

      PieceListPanel.Panel.SetActive(toggleOn);
    }

    static readonly List<PieceListRow> PieceListRows = new();

    public static void RefreshPieceListPanel() {
      foreach (PieceListRow row in PieceListRows) {
        Destroy(row.Row);
      }

      PieceListRows.Clear();

      foreach (Piece piece in Player.m_localPlayer.GetBuildPieces()) {
        PieceListRow row = new(PieceListPanel.Content.transform);
        row.SetContent(piece);

        PieceListRows.Add(row);
      }
    }
  }
}