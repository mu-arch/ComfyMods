using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;

using HarmonyLib;

using System.Linq;
using System.Reflection;

using UnityEngine;

namespace PotteryBarn {
  [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
  public class PotteryBarn : BaseUnityPlugin {
    public const string PluginGuid = "redseiko.valheim.potterybarn";
    public const string PluginName = "PotteryBarn";
    public const string PluginVersion = "1.1.0";

    static ConfigEntry<bool> _isModEnabled;

    static ManualLogSource _logger;
    Harmony _harmony;

    public void Awake() {
      _logger = Logger;

      _isModEnabled =
          Config.Bind("_Global", "isModEnabled", true, "Globally enable or disable this mod (restart required).");

      if (_isModEnabled.Value) {
        _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), harmonyInstanceId: PluginGuid);
      }
    }

    public void OnDestroy() {
      _harmony?.UnpatchSelf();
    }

    static PieceTable _hammerPieceTable;

    [HarmonyPatch(typeof(ZNetScene))]
    class ZNetScenePatch {
      [HarmonyPostfix]
      [HarmonyPatch(nameof(ZNetScene.Awake))]
      static void AwakePostfix(ref ZNetScene __instance) {
        AddHammerPieces();
      }
    }

    static void AddHammerPieces() {
      _hammerPieceTable ??= GetPieceTable("_HammerPieceTable");

      if (!_hammerPieceTable) {
        _logger.LogError("Could not find the Hammer PieceTable.");
        return;
      }

      AddStoneFloorPiece(_hammerPieceTable);
    }

    static void AddStoneFloorPiece(PieceTable pieceTable) {
      GameObject prefab = GetExistingPrefab("stone_floor");

      if (!prefab) {
        _logger.LogError("Could not find prefab: stone_floor");
        return;
      }

      Piece piece = prefab.GetComponent<Piece>();
      Piece.Requirement stoneReq = piece.GetRequirement("Stone");

      if (stoneReq == null) {
        _logger.LogError("Could not find Stone requirement for stone_floor Piece.");
        return;
      }

      stoneReq.m_amount = 12;
      stoneReq.m_recover = true;

      if (pieceTable.AddPiece(piece)) {
        _logger.LogInfo($"Added stone_floor prefab to {pieceTable.name} PieceTable.");
      }
    }

    static GameObject GetExistingPrefab(string name) {
      return ZNetScene.m_instance?.m_prefabs
          .Where(prefab => prefab?.name == name)
          .FirstOrDefault();
    }

    static PieceTable GetPieceTable(string name) {
      return ObjectDB.m_instance?.m_items
          .Select(item => item.GetComponent<ItemDrop>()?.m_itemData?.m_shared?.m_buildPieces)
          .Where(table => table?.name == name)
          .FirstOrDefault();
    }
  }

  static class HelperExtensions {
    public static bool AddPiece(this PieceTable pieceTable, Piece piece) {
      if (!piece || !pieceTable || pieceTable.m_pieces == null || pieceTable.m_pieces.Contains(piece.gameObject)) {
        return false;
      }

      pieceTable.m_pieces.Add(piece.gameObject);
      return true;
    }

    public static Piece.Requirement GetRequirement(this Piece piece, string name) {
      return piece?.m_resources?.Where(req => req?.m_resItem?.name == name).FirstOrDefault();
    }
  }
}