using BepInEx;

using HarmonyLib;

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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

      IsModEnabled.SettingChanged += (_, _) => ToggleSectorInfoPanel();
      ShowSectorInfoPanel.SettingChanged += (_, _) => ToggleSectorInfoPanel();

      IsModEnabled.SettingChanged += (_, _) => ToggleSectorBoundaries();
      ShowSectorBoundaries.SettingChanged += (_, _) => ToggleSectorBoundaries();

      ShowSectorZdoCountGrid.SettingChanged += (_, _) => ToggleSectorZdoCountGrid();

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
    static SectorZdoCountGrid _sectorZdoCountGrid;
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
        _sectorZdoCountGrid = _sectorInfoPanel.SectorZdoCountGrid;

        _sectorInfoPanel.Panel.RectTransform()
            .SetAnchorMin(new(0.5f, 1f))
            .SetAnchorMax(new(0.5f, 1f))
            .SetPivot(new(0.5f, 1f))
            .SetPosition(SectorInfoPanelPosition.Value)
            .SetSizeDelta(new(200f, 200f))
            .SetAsFirstSibling();

        _sectorInfoPanel.Panel.SetActive(true);

        _updateSectorInfoPanelCoroutine = Hud.m_instance.StartCoroutine(UpdateSectorInfoPanelCoroutine());
      }
    }

    static void ToggleSectorZdoCountGrid() {
      if (IsModEnabled.Value && ShowSectorInfoPanel.Value && _sectorZdoCountGrid?.Grid) {
        _sectorZdoCountGrid.Grid.SetActive(ShowSectorZdoCountGrid.Value);
      }
    }

    static IEnumerator UpdateSectorInfoPanelCoroutine() {
      WaitForSeconds waitInterval = new(seconds: 0.25f);
      Vector3 lastPosition = Vector3.positiveInfinity;
      Vector2i lastSector = EmptySector;
      long lastZdoCount = long.MaxValue;

      int gridSize = SectorZdoCountGridSize.Value == PluginConfig.GridSize.ThreeByThree ? 3 : 5;
      int gridOffset = SectorZdoCountGridSize.Value == PluginConfig.GridSize.ThreeByThree ? 1 : 2;

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

        _sectorInfoPanel.SectorXY.Value.SetText($"({sector.x}, {sector.y})");
        _sectorInfoPanel.SectorZdoCount.Value.SetText($"{zdoCount}");

        if (!ShowSectorZdoCountGrid.Value) {
          continue;
        }

        for (int i = 0; i < gridSize; i++) {
          for (int j = 0; j < gridSize; j++) {
            Vector2i gridSector = new(sector.x + i - gridOffset, sector.y + j - gridOffset);
            SetSectorZdoCountCellText(_sectorZdoCountGrid.Cells[i, j], gridSector);
          }
        }
      }
    }

    static void SetSectorZdoCountCellText(SectorZdoCountCell cell, Vector2i sector) {
      cell.ZdoCount.SetText($"{GetSectorZdoCount(sector)}");
      cell.Sector.SetText($"({sector.x}, {sector.y})");
    }

    public static void ToggleSectorBoundaries() {
      _updateBoundaryCube = false;
      _lastBoundarySector = EmptySector;

      if (_updateBoundaryCubeCoroutine != null) {
        Hud.m_instance.Ref()?.StopCoroutine(_updateBoundaryCubeCoroutine);
        _updateBoundaryCubeCoroutine = null;
      }

      if (_boundaryCube) {
        Destroy(_boundaryCube);
        _boundaryCube = null;
      }

      if (IsModEnabled.Value && ShowSectorBoundaries.Value && Hud.m_instance) {
        if (!_updateBoundaryCube) {
          _boundaryCube = CreateBoundaryCube();
          _updateBoundaryCube = true;

          _updateBoundaryCubeCoroutine = Hud.m_instance.StartCoroutine(UpdateBoundaryCubeCoroutine());
        }
      }
    }

    static readonly Vector2i EmptySector = new(int.MinValue, int.MaxValue);

    static readonly Lazy<Shader> DistortionShader = new(() => Shader.Find("Custom/Distortion"));

    static GameObject _boundaryCube;

    static bool _updateBoundaryCube = false;
    static Vector2i _lastBoundarySector = EmptySector;
    static Coroutine _updateBoundaryCubeCoroutine;

    static IEnumerator UpdateBoundaryCubeCoroutine() {
      WaitForSeconds waitInterval = new(seconds: 1f);

      while (_updateBoundaryCube) {
        yield return waitInterval;

        if (!ZoneSystem.m_instance || !Player.m_localPlayer || !_boundaryCube) {
          _lastBoundarySector = EmptySector;
          continue;
        }

        Vector2i sector = ZoneSystem.m_instance.GetZone(Player.m_localPlayer.transform.position);

        if (sector == _lastBoundarySector) {
          continue;
        }

        _boundaryCube.transform.position = ZoneSystem.m_instance.GetZonePos(sector);
        _lastBoundarySector = sector;
      }
    }

    static GameObject CreateBoundaryCube() {
      ZLog.Log($"Creating BoundaryCube.");

      GameObject cube = new($"BoundaryCube");
      cube.transform.position = Vector3.zero;

      CreateBoundaryCubeWall(cube, new Vector3(32f, 256f, 0f), new Vector3(0.1f, 512f, 64f));
      CreateBoundaryCubeWall(cube, new Vector3(-32f, 256f, 0f), new Vector3(0.1f, 512f, 64f));
      CreateBoundaryCubeWall(cube, new Vector3(0f, 256f, 32f), new Vector3(64f, 512f, 0.1f));
      CreateBoundaryCubeWall(cube, new Vector3(0f, 256f, -32f), new Vector3(64f, 512f, 0.1f));

      return cube;
    }

    static void CreateBoundaryCubeWall(GameObject cube, Vector3 position, Vector3 scale) {
      GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
      wall.name = $"BoundaryCube.Wall.{position}";

      if (wall.TryGetComponent(out Transform transform)) {
        transform.SetParent(cube.transform, worldPositionStays: false);
        transform.localPosition = position;
        transform.localScale = scale;
      }

      if (wall.TryGetComponent(out MeshRenderer renderer)) {
        renderer.material.SetColor("_Color", SectorBoundaryColor.Value);
        renderer.material.shader = DistortionShader.Value;
      }

      Destroy(wall.GetComponentInChildren<Collider>());
    }
  }
}
