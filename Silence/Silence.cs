using BepInEx;
using BepInEx.Logging;

using HarmonyLib;

using System.Collections;
using System.Reflection;

using UnityEngine;

using static Silence.PluginConfig;

namespace Silence {
  [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
  public class Silence : BaseUnityPlugin {
    public const string PluginGUID = "redseiko.valheim.silence";
    public const string PluginName = "Silence";
    public const string PluginVersion = "1.3.0";

    public static ManualLogSource _logger;
    Harmony _harmony;

    public void Awake() {
      _logger = Logger;

      BindConfig(Config);

      if (IsModEnabled.Value) {
        _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), harmonyInstanceId: PluginGUID);
      }
    }

    public void OnDestroy() {
      _harmony?.UnpatchSelf();
    }

    public static Chat ChatInstance { get; set; }

    public static bool EnableChatWindow { get; set; } = true;
    public static bool EnableInWorldTexts { get; set; } = true;

    static readonly WaitForEndOfFrame WaitForEndOfFrame = new();

    public static IEnumerator ToggleSilenceCoroutine() {
      yield return WaitForEndOfFrame;

      EnableChatWindow = !HideChatWindow.Value || !EnableChatWindow;
      EnableInWorldTexts = !HideInWorldTexts.Value || !EnableInWorldTexts;

      _logger.LogInfo($"ChatWindow: {EnableChatWindow}\nInWorldTexts: {EnableInWorldTexts}");

      MessageHud.instance?.ShowMessage(
          MessageHud.MessageType.Center, $"ChatWindow: {EnableChatWindow}\nInWorldTexts: {EnableInWorldTexts}");

      if (ChatInstance && !EnableChatWindow) {
        ChatInstance.m_hideTimer = ChatInstance.m_hideDelay;
        ChatInstance.m_focused = false;
        ChatInstance.m_wasFocused = false;
        ChatInstance.m_input.DeactivateInputField();
      }

      if (ChatInstance && !EnableInWorldTexts) {
        foreach (Chat.WorldTextInstance worldText in ChatInstance.m_worldTexts) {
          Destroy(worldText.m_gui);
        }

        ChatInstance.m_worldTexts.Clear();
      }
    }
  }
}
