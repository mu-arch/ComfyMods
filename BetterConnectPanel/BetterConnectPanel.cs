using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using Steamworks;
using System.Collections;
using System.Reflection;
using UnityEngine;

namespace BetterConnectPanel {
  [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
  public class BetterConnectPanel : BaseUnityPlugin {
    public const string PluginGUID = "redseiko.valheim.betterconnectpanel";
    public const string PluginName = "BetterConnectPanel";
    public const string PluginVersion = "1.0.0";

    static ConfigEntry<bool> _isModEnabled;
    static ConfigEntry<Vector2> _networkPanelPosition;
    static ConfigEntry<int> _networkPanelFontSize;
    static ConfigEntry<Color> _networkPanelBackgroundColor;

    Harmony _harmony;

    void Awake() {
      _isModEnabled =
          Config.Bind("_Global", "isModEnabled", true, "Globally enable or disable this mod.");

      _isModEnabled.SettingChanged += (sender, eventArgs) => TogglePanels(Hud.instance);

      _networkPanelPosition =
          Config.Bind(
              "NetworkPanel", "networkPanelPosition", new Vector2(10f, -150f), "Position of the NetworkPanel.");

      _networkPanelPosition.SettingChanged +=
          (sender, eventArgs) => _networkPanel?.SetPosition(_networkPanelPosition.Value);

      _networkPanelFontSize =
          Config.Bind(
              "NetworkPanel",
              "networkPanelFontSize",
              14,
              new ConfigDescription("Font size for the NetworkPanel.", new AcceptableValueRange<int>(6, 32)));

      _networkPanelFontSize.SettingChanged +=
          (sender, eventArgs) => _networkPanel?.SetTextFontSize(_networkPanelFontSize.Value);

      _networkPanelBackgroundColor =
          Config.Bind(
              "NetworkPanel",
              "networkPanelBackgroundColor",
              (Color) new Color32(0, 0, 0, 96),
              "Background color of the NetworkPanel.");

      _networkPanelBackgroundColor.SettingChanged +=
          (sender, eventArgs) => _networkPanel?.SetBackgroundColor(_networkPanelBackgroundColor.Value);

      _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
    }

    void OnDestroy() {
      _harmony?.UnpatchSelf();
    }

    [HarmonyPatch(typeof(ConnectPanel))]
    class ConnectPanelPatch {
      [HarmonyPostfix]
      [HarmonyPatch(nameof(ConnectPanel.Start))]
      static void StartPostfix(ref ConnectPanel __instance) {
        // Destroy(__instance);
      }
    }

    [HarmonyPatch(typeof(Hud))]
    class HudPatch {
      [HarmonyPostfix]
      [HarmonyPatch(nameof(Hud.Awake))]
      static void AwakePostfix(ref Hud __instance) {
        TogglePanels(__instance);
      }
    }

    static TwoColumnPanel _networkPanel;
    static TwoColumnPanel.PanelRow _serverFieldRow;
    static TwoColumnPanel.PanelRow _worldNameRow;
    static TwoColumnPanel.PanelRow _connectionPingRow;
    static TwoColumnPanel.PanelRow _connectionQualityRow;
    static TwoColumnPanel.PanelRow _connectionOutRateRow;
    static TwoColumnPanel.PanelRow _connectionInRateRow;
    static TwoColumnPanel.PanelRow _conectionPendingReliableRow;
    static TwoColumnPanel.PanelRow _conectionSentUnackedReliableRow;
    static TwoColumnPanel.PanelRow _connectionEstimatedSendQueueTimeRow;

    static void TogglePanels(Hud hud) {
      _networkPanel?.DestroyPanel();

      if (_isModEnabled.Value && hud) {
        CreatePanels(hud);
      }
    }

    static void CreatePanels(Hud hud) {
      _networkPanel =
          new TwoColumnPanel("NetworkPanel", hud.transform, hud.m_hoverName.font, _networkPanelFontSize.Value)
              .AddPanelRow("Server Host", "ServerAddress", out _serverFieldRow)
              .AddPanelRow("Server World", "WorldName", out _worldNameRow)
              .AddPanelRow("Ping", "ConnectionPing", out _connectionPingRow)
              .AddPanelRow("Quality (L/R)", "ConnectionQuality", out _connectionQualityRow)
              .AddPanelRow("Send Rate", "OutRate", out _connectionOutRateRow)
              .AddPanelRow("Receive Rate", "InRate", out _connectionInRateRow)
              .AddPanelRow("Pending Send", "PendingReliable", out _conectionPendingReliableRow)
              .AddPanelRow("Unacked Send", "SentUnackedReliable", out _conectionSentUnackedReliableRow)
              .AddPanelRow("Est. Send Queue Time", "EstimatedSendQueueTIme", out _connectionEstimatedSendQueueTimeRow)
              .SetPosition(_networkPanelPosition.Value)
              .SetBackgroundColor(_networkPanelBackgroundColor.Value)
              .SetActive(true);

      if (_networkPanel.Panel.TryGetComponent(out RectTransform transform)) {
        transform.anchorMin = new Vector2(0f, 0.5f);
        transform.anchorMax = new Vector2(0f, 0.5f);
        transform.pivot = new Vector2(0f, 0.5f);
      }

      hud.StartCoroutine(UpdateNetworkPanelCoroutine());
    }

    static IEnumerator UpdateNetworkPanelCoroutine() {
      WaitForSeconds waitInterval = new(seconds: 0.1f);
      ZSteamSocket serverSocket = null;

      while (serverSocket == null) {
        yield return waitInterval;
        serverSocket = (ZSteamSocket) ZNet.instance?.GetServerPeer()?.m_socket;
      }

      while (ZNet.m_world == null) {
        yield return waitInterval;
      }

      ZNet.m_serverIPAddr.ToString(out string serverAddress, true);
      _serverFieldRow.RightText.text = $"<color=#F7DC6F>{serverAddress}</color>";
      _worldNameRow.RightText.text = $"<color=#F7DC6F>{ZNet.instance.GetWorldName()}</color>";

      while (true) {
        yield return waitInterval;

        UpdateConnectionStatusProperties(serverSocket);
      }
    }

    static void UpdateConnectionStatusProperties(ZSteamSocket serverSocket) {
      if (!SteamNetworkingSockets.GetQuickConnectionStatus(
          serverSocket.m_con, out SteamNetworkingQuickConnectionStatus status)) {
        return;
      }

      _connectionPingRow.RightText.text = $"<color=#F7DC6F>{status.m_nPing:N0}</color> ms";

      _connectionQualityRow.RightText.text = string.Format(
          "<color={0}>{1:0%}</color> / <color={0}>{2:0%}</color>",
          "#F7DC6F",
          status.m_flConnectionQualityLocal,
          status.m_flConnectionQualityRemote);

      _connectionOutRateRow.RightText.text =
          $"<color=#F7DC6F>{status.m_flOutBytesPerSec / 1024:N2}</color> KB/s";

      _connectionInRateRow.RightText.text =
          $"<color=#F7DC6F>{status.m_flInBytesPerSec / 1024:N2}</color> KB/s";

      _conectionPendingReliableRow.RightText.text = $"<color=#F7DC6F>{status.m_cbPendingReliable:N0}</color> B";

      _conectionSentUnackedReliableRow.RightText.text =
          $"<color=#F7DC6F>{status.m_cbSentUnackedReliable:N0}</color> B";

      _connectionEstimatedSendQueueTimeRow.RightText.text =
          $"<color=#F7DC6F>{status.m_usecQueueTime.m_SteamNetworkingMicroseconds / 1024:N0}</color> ms";
    }
  }
}