using System.Collections;
using System.Reflection;

using BepInEx;
using BepInEx.Logging;

using HarmonyLib;

using UnityEngine;

using static Silence.PluginConfig;

namespace Silence {
  [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
  public class Silence : BaseUnityPlugin {
    public const string PluginGUID = "redseiko.valheim.silence";
    public const string PluginName = "Silence";
    public const string PluginVersion = "1.4.0";

    public static ManualLogSource _logger;
    Harmony _harmony;

    void Awake() {
      _logger = Logger;

      BindConfig(Config);

      if (IsModEnabled.Value) {
        _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), harmonyInstanceId: PluginGUID);
      }
    }

    void OnDestroy() {
      _harmony?.UnpatchSelf();
    }

    public static Chat ChatInstance { get; set; }

    public static bool EnableChatWindow { get; set; } = true;
    public static bool EnableInWorldTexts { get; set; } = true;

    static readonly WaitForEndOfFrame _endOfFrame = new();

    public static IEnumerator ToggleSilenceCoroutine() {
      yield return _endOfFrame;

      EnableChatWindow = !HideChatWindow.Value || !EnableChatWindow;
      EnableInWorldTexts = !HideInWorldTexts.Value || !EnableInWorldTexts;

      _logger.LogInfo($"ChatWindow: {EnableChatWindow}\nInWorldTexts: {EnableInWorldTexts}");

      MessageHud.instance.ShowMessage(
          MessageHud.MessageType.Center, $"ChatWindow: {EnableChatWindow}\nInWorldTexts: {EnableInWorldTexts}");

      if (!ChatInstance) {
        yield break;
      }

      if (!EnableChatWindow) {
        ChatInstance.m_hideTimer = ChatInstance.m_hideDelay;
        ChatInstance.m_focused = false;
        ChatInstance.m_wasFocused = false;
        ChatInstance.m_input.DeactivateInputField();
      }

      ChatInstance.m_output.gameObject.SetActive(EnableChatWindow);

      if (!EnableInWorldTexts) {
        foreach (Chat.WorldTextInstance worldText in ChatInstance.m_worldTexts) {
          Destroy(worldText.m_gui);
        }

        ChatInstance.m_worldTexts.Clear();
      }
    }
  }
}
