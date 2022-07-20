using BepInEx;
using BepInEx.Logging;

using HarmonyLib;

using System.Collections;
using System.Reflection;

using UnityEngine;

using static Pinnacle.PluginConfig;

namespace Pinnacle {
  [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
  public class Pinnacle : BaseUnityPlugin {
    public const string PluginGuid = "redseiko.valheim.pinnacle";
    public const string PluginName = "Pinnacle";
    public const string PluginVersion = "1.0.0";

    static ManualLogSource _logger;
    Harmony _harmony;

    public void Awake() {
      BindConfig(Config);

      _logger = Logger;
      _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), harmonyInstanceId: PluginGuid);
    }

    public void OnDestroy() {
      _harmony?.UnpatchSelf();
    }

    public static PinEditPanel PinEditPanel { get; private set; }

    public static void TogglePinEditPanel(Minimap.PinData pin) {
      if (!PinEditPanel?.Panel) {
        PinEditPanel = new(Minimap.m_instance.m_largeRoot.transform);
        PinEditPanel.Panel.RectTransform()
            .SetAnchorMin(new(0.5f, 0f))
            .SetAnchorMax(new(0.5f, 0f))
            .SetPivot(new(0.5f, 0f))
            .SetPosition(new(0f, 25f))
            .SetSizeDelta(new(200f, 200f));
      }

      if (pin == null) {
        PinEditPanel.Panel.SetActive(false);
      } else {
        CenterMapOnPinPosition(pin.m_pos);

        PinEditPanel.SetTargetPin(pin);
        PinEditPanel.Panel.SetActive(true);
      }
    }

    public static PinListPanel PinListPanel { get; private set; }

    public static void TogglePinListPanel() {
      if (!PinListPanel?.Panel) {
        PinListPanel = new(Minimap.m_instance.m_largeRoot.transform);
        PinListPanel.Panel.RectTransform()
            .SetAnchorMin(new(0f, 0.5f))
            .SetAnchorMax(new(0f, 0.5f))
            .SetPivot(new(0f, 0.5f))
            .SetPosition(new(25f, 0f))
            .SetSizeDelta(new(400f, 400f));
      }

      if (PinListPanel.Panel.activeSelf) {
        PinListPanel.Panel.SetActive(false);
      } else {
        PinListPanel.Panel.SetActive(true);
        PinListPanel.SetTargetPins();
      }
    }

    public static void CenterMapOnOrTeleportTo(Vector3 targetPosition) {
      if (IsModEnabled.Value
          && Console.m_instance.IsCheatsEnabled()
          && Player.m_localPlayer
          && Input.GetKeyDown(KeyCode.LeftShift)) {
        TeleportTo(targetPosition);
      } else {
        CenterMapOnPinPosition(targetPosition);
      }
    }

    public static void TeleportTo(Vector3 targetPosition) {
      Player player = Player.m_localPlayer;

      if (!player) {
        _logger.LogWarning($"No local Player found.");
        return;
      }

      targetPosition.y = GetHeight(targetPosition);

      _logger.LogInfo($"Teleporting player from {player.transform.position} to {targetPosition}.");
      player.TeleportTo(targetPosition, player.transform.rotation, distantTeleport: true);
    }

    public static float GetHeight(Vector3 targetPosition) {
      Heightmap.GetHeight(targetPosition, out float height);

      if (height == 0f) {
        height = GetHeightmapData(targetPosition).m_baseHeights[0];
      }

      return height;
    }

    public static HeightmapBuilder.HMBuildData GetHeightmapData(Vector3 targetPosition) {
      HeightmapBuilder.HMBuildData heightmapData =
          new(targetPosition, width: 1, scale: 1f, distantLod: false, WorldGenerator.m_instance);

      HeightmapBuilder.m_instance.Build(heightmapData);

      return heightmapData;
    }

    static Coroutine _centerMapCoroutine;

    public static void CenterMapOnPinPosition(Vector3 targetPosition) {
      if (_centerMapCoroutine != null) {
        Minimap.m_instance.StopCoroutine(_centerMapCoroutine);
      }

      PinEditPanel?.Panel.Ref()?.SetActive(false);

      _centerMapCoroutine =
          Minimap.m_instance.StartCoroutine(
              CenterMapCoroutine(
                  targetPosition - Player.m_localPlayer.transform.position, CenterMapLerpDuration.Value));
    }

    static IEnumerator CenterMapCoroutine(Vector3 targetPosition, float lerpDuration) {
      float timeElapsed = 0f;
      Vector3 startPosition = Minimap.m_instance.m_mapOffset;

      while (timeElapsed < lerpDuration) {
        float t = timeElapsed / lerpDuration;
        t = t * t * (3f - (2f * t));

        Minimap.m_instance.m_mapOffset = Vector3.Lerp(startPosition, targetPosition, t);
        timeElapsed += Time.deltaTime;

        yield return null;
      }

      Minimap.m_instance.m_mapOffset = targetPosition;
    }
  }
}