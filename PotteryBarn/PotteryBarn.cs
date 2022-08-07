using BepInEx;
using BepInEx.Logging;

using HarmonyLib;

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using UnityEngine;

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

    static readonly Lazy<string> IconsFolderPath =
        new(() => Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "PotteryBarn"));

    public static readonly HashSet<string> CustomPiecePrefabNames = new();

    static GameObject GetPrefab(string prefabName) {
      return ZNetScene.m_instance.Ref()?.GetPrefab(prefabName);
    }

    static Piece GetExistingPiece(string prefabName) {
      return ZNetScene.m_instance.Ref()?.GetPrefab(prefabName).GetComponent<Piece>();
    }

    static Piece CreateCustomPiece(string prefabName) {
      GameObject prefab = ZNetScene.m_instance.m_prefabs.Where(p => p.name.StartsWith(prefabName)).FirstOrDefault();

      if (!prefab) {
        _logger.LogError($"Could not find prefab: {prefabName}");
        return null;
      }

      Piece piece = prefab.AddComponent<Piece>();
      piece.SetName(prefabName);

      piece.m_repairPiece = false;
      piece.m_groundOnly = false;
      piece.m_groundPiece = false;
      piece.m_cultivatedGroundOnly = false;
      piece.m_waterPiece = false;
      piece.m_noInWater = false;
      piece.m_notOnWood = false;
      piece.m_notOnTiltingSurface = false;
      piece.m_inCeilingOnly = false;
      piece.m_notOnFloor = false;
      piece.m_onlyInTeleportArea = false;
      piece.m_allowedInDungeons = false;
      piece.m_clipEverything = true;

      CustomPiecePrefabNames.Add(prefabName);

      return piece;
    }

    public static IEnumerator AddPieces() {
      yield return null;

      PieceTable pieceTable = GetPieceTable("_HammerPieceTable");

      if (!pieceTable) {
        _logger.LogError($"Could not find HammerPieceTable!");
        yield break;
      }

      yield return AddHammerPieces(pieceTable);

      Piece statueCorgi = CreateCustomPiece("StatueCorgi");

      if (!statueCorgi) {
        yield break;
      }

      statueCorgi.SetResources(CreateRequirement("Stone", 10, true));
      statueCorgi.m_craftingStation = GetPrefab("piece_workbench").GetComponent<CraftingStation>();

      if (TryGetPrefabIcon(statueCorgi.name, out Sprite icon)) {
        statueCorgi.m_icon = icon;
      }

      pieceTable.AddPiece(statueCorgi);
    }

    static IEnumerator AddHammerPieces(PieceTable pieceTable) {
      yield return null;

      pieceTable.AddPiece(GetExistingPiece("turf_roof").SetName("turf_roof"));
      pieceTable.AddPiece(GetExistingPiece("turf_roof_top").SetName("turf_roof_top"));
      pieceTable.AddPiece(GetExistingPiece("turf_roof_wall").SetName("turf_roof_wall"));
      pieceTable.AddPiece(GetExistingPiece("ArmorStand_Female").SetName("ArmorStand_Female"));
      pieceTable.AddPiece(GetExistingPiece("ArmorStand_Male").SetName("ArmorStand_Male"));

      pieceTable.AddPiece(
          GetExistingPiece("stone_floor")
              .SetResource("Stone", r => r.SetAmount(12).SetRecover(true)));
    }

    static bool TryGetPrefabIcon(string prefabName, out Sprite icon) {
      FileInfo file = new(Path.Combine(IconsFolderPath.Value, $"{prefabName}.png"));

      if (file.Exists) {
        Texture2D texture = new(1, 1);

        if (texture.LoadImage(File.ReadAllBytes(file.FullName))) {
          icon = Sprite.Create(texture, new(0, 0, texture.width, texture.height), new(0.5f, 0.5f));
          return true;
        }
      }

      icon = default;
      return false;
    }

    static Piece.Requirement CreateRequirement(string itemName, int amount, bool recover) {
      if (ObjectDB.m_instance.m_itemByHash.TryGetValue(itemName.GetStableHashCode(), out GameObject prefab)
          && prefab.TryGetComponent(out ItemDrop itemDrop)) {
        return new() {
          m_resItem = itemDrop,
          m_amount = amount,
          m_recover = recover
        };
      }

      return null;
    }

    static PieceTable GetPieceTable(string name) {
      return ObjectDB.m_instance?.m_items
          .Select(item => item.GetComponent<ItemDrop>()?.m_itemData?.m_shared?.m_buildPieces)
          .Where(table => table?.name == name)
          .FirstOrDefault();
    }
  }
}