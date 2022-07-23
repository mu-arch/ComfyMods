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

      ZLog.Log($"Current Application.targetFrameRate: {Application.targetFrameRate}");
      Application.targetFrameRate = 60;
    }

    public void OnDestroy() {
      _harmony?.UnpatchSelf();
    }

    public static PinEditPanel PinEditPanel { get; private set; }

    public static void TogglePinEditPanel(Minimap.PinData pinToEdit = null) {
      if (!PinEditPanel?.Panel) {
        PinEditPanel = new(Minimap.m_instance.m_largeRoot.transform);
        PinEditPanel.Panel.RectTransform()
            .SetAnchorMin(new(0.5f, 0f))
            .SetAnchorMax(new(0.5f, 0f))
            .SetPivot(new(0.5f, 0f))
            .SetPosition(new(0f, 25f))
            .SetSizeDelta(new(200f, 200f));
      }

      if (pinToEdit == null) {
        PinEditPanel.SetActive(false);
      } else {
        CenterMapHelper.CenterMapOnPosition(pinToEdit.m_pos);

        PinEditPanel.SetTargetPin(pinToEdit);
        PinEditPanel.SetActive(true);
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
            .SetPosition(PinListPanelPosition.Value)
            .SetSizeDelta(PinListPanelSizeDelta.Value);

        PinListPanelPosition.OnSettingChanged(
            position => PinListPanel?.Panel.Ref()?.RectTransform().SetPosition(position));

        PinListPanelSizeDelta.OnSettingChanged(
            sizeDelta => {
              if (PinListPanel?.Panel) {
                PinListPanel.Panel.RectTransform().SetSizeDelta(sizeDelta);
                PinListPanel.SetTargetPins();
              }
            });

        PinListPanelBackgroundColor.OnSettingChanged(color => PinListPanel?.Panel.Ref()?.Image().SetColor(color));

        PinListPanel.PanelDragger.OnPanelEndDrag += (_, position) => PinListPanelPosition.Value = position;
        PinListPanel.PanelResizer.OnPanelEndResize += (_, sizeDelta) => PinListPanelSizeDelta.Value = sizeDelta;
      }

      if (PinListPanel.Panel.activeSelf) {
        PinListPanel.PinNameFilter.InputField.DeactivateInputField();
        PinListPanel.Panel.SetActive(false);
      } else {
        PinListPanel.Panel.SetActive(true);
        PinListPanel.SetTargetPins();
      }
    }

    public static void CenterMapOnOrTeleportTo(Minimap.PinData targetPin) {
      if (IsModEnabled.Value
          && Console.m_instance.IsCheatsEnabled()
          && Player.m_localPlayer
          && Input.GetKey(KeyCode.LeftShift)
          && targetPin != null) {
        TeleportTo(targetPin.m_pos);
      } else {
        TogglePinEditPanel(null); // TODO: make this an option to show PinEditPanel map on RowClick
        CenterMapHelper.CenterMapOnPosition(targetPin.m_pos);
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

      Minimap.m_instance.SetMapMode(Minimap.MapMode.Small);
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
  }
}