using BepInEx;
using BepInEx.Logging;

using HarmonyLib;

using System.Collections;
using System.Linq;
using System.Reflection;

using static PotteryBarn.PluginConfig;

namespace PotteryBarn {
  [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
  public class PotteryBarn : BaseUnityPlugin {
    public const string PluginGuid = "redseiko.valheim.potterybarn";
    public const string PluginName = "PotteryBarn";
    public const string PluginVersion = "1.2.0";

    static ManualLogSource _logger;
    Harmony _harmony;

    public void Awake() {
      _logger = Logger;

      BindConfig(Config);

      _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), harmonyInstanceId: PluginGuid);
    }

    public void OnDestroy() {
      _harmony?.UnpatchSelf();
    }

    public static IEnumerator AddPieces() {
      yield return AddHammerPieces(GetPieceTable("_HammerPieceTable"));
    }

    static IEnumerator AddHammerPieces(PieceTable pieceTable) {
      yield return null;

      if (!pieceTable) {
        _logger.LogError($"Could not find HammerPieceTable!");
        yield break;
      }

      pieceTable.AddPiece(GetExistingPiece("turf_roof").SetName("turf_roof"));
      pieceTable.AddPiece(GetExistingPiece("turf_roof_top").SetName("turf_roof_top"));
      pieceTable.AddPiece(GetExistingPiece("turf_roof_wall").SetName("turf_roof_wall"));
      pieceTable.AddPiece(GetExistingPiece("ArmorStand_Female").SetName("ArmorStand_Female"));
      pieceTable.AddPiece(GetExistingPiece("ArmorStand_Male").SetName("ArmorStand_Male"));

      pieceTable.AddPiece(
          GetExistingPiece("stone_floor")
              .SetResource("Stone", r => r.SetAmount(12).SetRecover(true)));
    }

    static PieceTable GetPieceTable(string pieceTableName) {
      return ObjectDB.m_instance.Ref()?.m_items
          .Select(item => item.GetComponent<ItemDrop>().Ref()?.m_itemData?.m_shared?.m_buildPieces)
          .Where(table => table.Ref()?.name == pieceTableName)
          .FirstOrDefault();
    }

    static Piece GetExistingPiece(string prefabName) {
      return ZNetScene.m_instance.Ref()?.GetPrefab(prefabName).Ref()?.GetComponent<Piece>();
    }
  }
}