using BepInEx;

using HarmonyLib;

using System;
using System.Collections;
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

      _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), harmonyInstanceId: PluginGuid);
    }

    public void OnDestroy() {
      _harmony?.UnpatchSelf();
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
            .SetSizeDelta(new(SectorInfoPanelWidth.Value, 200f));

        _sectorInfoPanel.Panel.SetActive(true);

        _updateSectorInfoPanelCoroutine = Hud.m_instance.StartCoroutine(UpdateSectorInfoPanelCoroutine());
      }
    }

    static IEnumerator UpdateSectorInfoPanelCoroutine() {
      WaitForSeconds waitInterval = new(seconds: 0.25f);
      Vector3 lastPosition = Vector3.positiveInfinity;

      while (true) {
        yield return waitInterval;

        if (!ZoneSystem.m_instance || !Player.m_localPlayer || !_sectorInfoPanel?.Panel) {
          continue;
        }

        Vector3 position = Player.m_localPlayer.transform.position;

        if (position != lastPosition) {
          lastPosition = position;
          _sectorInfoPanel.PositionX.SetText($"{position.x:F0}");
          _sectorInfoPanel.PositionY.SetText($"{position.y:F0}");
          _sectorInfoPanel.PositionZ.SetText($"{position.z:F0}");
        }
      }
    }

    public static void ToggleSectorBoundaries() {
      if (IsModEnabled.Value && ShowSectorBoundaries.Value && Hud.m_instance) {
        if (!_updateBoundaryCube) {
          _updateBoundaryCube = true;
          Hud.m_instance.StartCoroutine(UpdateBoundaryCubeCoroutine());
        }
      } else {
        _updateBoundaryCube = false;
        _lastSector = EmptySector;

        if (_boundaryCube) {
          Destroy(_boundaryCube);
          _boundaryCube = null;
        }
      }
    }

    static readonly Vector2i EmptySector = new(int.MinValue, int.MaxValue);
    static readonly Lazy<Shader> DistortionShader = new(() => Shader.Find("Custom/Distortion"));

    static GameObject _boundaryCube;

    static bool _updateBoundaryCube = false;
    static Vector2i _lastSector = EmptySector;

    static IEnumerator UpdateBoundaryCubeCoroutine() {
      WaitForSeconds waitInterval = new(seconds: 1f);

      while (_updateBoundaryCube) {
        yield return waitInterval;

        if (!ZoneSystem.m_instance || !Player.m_localPlayer) {
          _lastSector = EmptySector;
          continue;
        }

        if (!_boundaryCube) {
          _boundaryCube = CreateBoundaryCube();
        }

        Vector2i sector = ZoneSystem.m_instance.GetZone(Player.m_localPlayer.transform.position);

        if (sector == _lastSector) {
          continue;
        }

        _boundaryCube.transform.position = ZoneSystem.m_instance.GetZonePos(sector);
        _lastSector = sector;
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

  public static class ObjectExtensions {
    public static T Ref<T>(this T o) where T : UnityEngine.Object {
      return o ? o : null;
    }
  }
}
