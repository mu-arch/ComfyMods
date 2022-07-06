using BepInEx;

using HarmonyLib;

using System.Collections;
using System.Collections.Generic;
using System.Reflection;

using UnityEngine;

using static ZoneScouter.PluginConfig;

namespace ZoneScouter {
  [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
  public class ZoneScouter : BaseUnityPlugin {
    public const string PluginGuid = "redseiko.valheim.zonescouter";
    public const string PluginName = "ZoneScouter";
    public const string PluginVersion = "1.0.0";

    Harmony _harmony;

    public void Awake() {
      BindConfig(Config);

      IsModEnabled.OnSettingChanged(ToggleSectorInfoPanel);
      ShowSectorInfoPanel.OnSettingChanged(ToggleSectorInfoPanel);

      SectorInfoPanelBackgroundColor.OnSettingChanged(color => _sectorInfoPanel?.Panel.Ref()?.Image().SetColor(color));
      SectorInfoPanelPosition.OnSettingChanged(
          position => _sectorInfoPanel.Panel.Ref()?.RectTransform().SetPosition(position));

      SectorInfoPanelFontSize.OnSettingChanged(ToggleSectorInfoPanel);

      ShowSectorZdoCountGrid.OnSettingChanged(ToggleSectorZdoCountGrid);
      SectorZdoCountGridSize.OnSettingChanged(ToggleSectorZdoCountGrid);

      CellZdoCountBackgroundImageColor.OnSettingChanged(() => _sectorZdoCountGrid?.SetCellStyle());
      CellZdoCountTextColor.OnSettingChanged(() => _sectorZdoCountGrid?.SetCellStyle());
      CellZdoCountTextFontSize.OnSettingChanged(() => _sectorZdoCountGrid?.SetCellStyle());

      CellSectorBackgroundImageColor.OnSettingChanged(() => _sectorZdoCountGrid?.SetCellStyle());
      CellSectorTextColor.OnSettingChanged(() => _sectorZdoCountGrid?.SetCellStyle());
      CellSectorTextFontSize.OnSettingChanged(() => _sectorZdoCountGrid?.SetCellStyle());

      IsModEnabled.OnSettingChanged(SectorBoundaries.ToggleSectorBoundaries);
      ShowSectorBoundaries.OnSettingChanged(SectorBoundaries.ToggleSectorBoundaries);
      SectorBoundaryColor.OnSettingChanged(SectorBoundaries.SetBoundaryColor);

      _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), harmonyInstanceId: PluginGuid);
    }

    public void OnDestroy() {
      _harmony?.UnpatchSelf();
    }

    static readonly Dictionary<Vector2i, int> SectorToIndexCache = new();

    static long GetSectorZdoCount(Vector2i sector) {
      if (!SectorToIndexCache.TryGetValue(sector, out int index)) {
        index = ZDOMan.m_instance.SectorToIndex(sector);
        SectorToIndexCache[sector] = index;
      }

      return index >= 0
          ? ZDOMan.m_instance.m_objectsBySector[index]?.Count ?? 0L
          : 0L;
    }

    static SectorInfoPanel _sectorInfoPanel;
    static Coroutine _updateSectorInfoPanelCoroutine;

    public static void ToggleSectorInfoPanel() {
      if (_updateSectorInfoPanelCoroutine != null) {
        Hud.m_instance.Ref().StopCoroutine(_updateSectorInfoPanelCoroutine);
        _updateSectorInfoPanelCoroutine = null;
      }

      if (_sectorInfoPanel?.Panel) {
        Destroy(_sectorInfoPanel.Panel);
        _sectorInfoPanel = null;
      }

      if (IsModEnabled.Value && ShowSectorInfoPanel.Value && Hud.m_instance) {
        _sectorInfoPanel = new(Hud.m_instance.transform);

        _sectorInfoPanel.Panel.RectTransform()
            .SetAnchorMin(new(0.5f, 1f))
            .SetAnchorMax(new(0.5f, 1f))
            .SetPivot(new(0.5f, 1f))
            .SetPosition(SectorInfoPanelPosition.Value)
            .SetSizeDelta(new(200f, 200f))
            .SetAsFirstSibling();

        _sectorInfoPanel.PanelDragger.OnEndDragAction = position => SectorInfoPanelPosition.Value = position;

        _sectorInfoPanel.Panel.SetActive(true);
        _updateSectorInfoPanelCoroutine = Hud.m_instance.StartCoroutine(UpdateSectorInfoPanelCoroutine());
      }

      ToggleSectorZdoCountGrid();
    }

    static IEnumerator UpdateSectorInfoPanelCoroutine() {
      WaitForSeconds waitInterval = new(seconds: 0.25f);
      Vector3 lastPosition = Vector3.positiveInfinity;
      Vector2i lastSector = new(int.MinValue, int.MaxValue);
      long lastZdoCount = long.MaxValue;

      while (true) {
        yield return waitInterval;

        if (!ZoneSystem.m_instance || !Player.m_localPlayer || !_sectorInfoPanel?.Panel) {
          continue;
        }

        Vector3 position = Player.m_localPlayer.transform.position;

        if (position != lastPosition) {
          lastPosition = position;
          _sectorInfoPanel.PositionX.Value.SetText($"{position.x:F0}");
          _sectorInfoPanel.PositionY.Value.SetText($"{position.y:F0}");
          _sectorInfoPanel.PositionZ.Value.SetText($"{position.z:F0}");
        }

        Vector2i sector = ZoneSystem.m_instance.GetZone(position);
        long zdoCount = GetSectorZdoCount(sector);

        if (sector == lastSector && zdoCount == lastZdoCount) {
          continue;
        }

        lastSector = sector;
        lastZdoCount = zdoCount;

        _sectorInfoPanel.SectorXY.Value.SetText($"{sector.x},{sector.y}");
        _sectorInfoPanel.SectorZdoCount.Value.SetText($"{zdoCount}");
      }
    }

    static SectorZdoCountGrid _sectorZdoCountGrid;
    static Coroutine _updateSectorZdoCountGridCoroutine;

    public static void ToggleSectorZdoCountGrid() {
      if (_updateSectorZdoCountGridCoroutine != null && Hud.m_instance) {
        Hud.m_instance.StopCoroutine(_updateSectorZdoCountGridCoroutine);
        _updateSectorZdoCountGridCoroutine = null;
      }

      if (_sectorZdoCountGrid?.Grid) {
        Destroy(_sectorZdoCountGrid.Grid);
        _sectorZdoCountGrid = null;
      }

      if (IsModEnabled.Value && ShowSectorInfoPanel.Value && ShowSectorZdoCountGrid.Value && _sectorInfoPanel?.Panel) {
        _sectorZdoCountGrid = new(_sectorInfoPanel.Panel.transform, SectorZdoCountGridSize.Value);
        _sectorZdoCountGrid.Grid.SetActive(true);

        _updateSectorZdoCountGridCoroutine = Hud.m_instance.StartCoroutine(UpdateSectorZdoCountGrid());
      }
    }

    static IEnumerator UpdateSectorZdoCountGrid() {
      if (!_sectorZdoCountGrid?.Grid) {
        yield break;
      }

      WaitForSeconds waitInterval = new(seconds: 1f);

      int size = _sectorZdoCountGrid.Size;
      int offset = Mathf.FloorToInt(size / 2f);

      while (_sectorZdoCountGrid?.Grid) {
        if (!Player.m_localPlayer) {
          yield return waitInterval;
          continue;
        }

        Vector2i sector = ZoneSystem.m_instance.GetZone(Player.m_localPlayer.transform.position);

        for (int i = 0; i < size; i++) {
          for (int j = 0; j < size; j++) {
            Vector2i cellSector = new(sector.x + i - offset, sector.y + j - offset);
            SectorZdoCountCell cell = _sectorZdoCountGrid.Cells[i, j];

            cell.ZdoCount.SetText($"{GetSectorZdoCount(cellSector)}");
            cell.Sector.SetText($"{cellSector.x},{cellSector.y}");
          }
        }

        yield return waitInterval;
      }
    }
  }
}
