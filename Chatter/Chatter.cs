using BepInEx;

using HarmonyLib;

using System;
using System.Collections.Generic;
using System.Reflection;

using UnityEngine;
using UnityEngine.UI;

using static Chatter.PluginConfig;

namespace Chatter {
  [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
  public class Chatter : BaseUnityPlugin {
    public const string PluginGuid = "redseiko.valheim.chatter";
    public const string PluginName = "Chatter";
    public const string PluginVersion = "1.0.0";

    Harmony _harmony;

    public void Awake() {
      BindConfig(Config);

      _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), harmonyInstanceId: PluginGuid);
    }

    public void OnDestroy() {
      _harmony?.UnpatchSelf();
    }

    static readonly List<ChatMessage> MessageHistory = new();

    static GameObject _chatPanel;
    static GameObject _chatViewport;
    static GameObject _chatMessages;
    static GameObject _textPrefab;

    static ScrollRect _chatPanelScroll;
    static float _maxWidth = 0f;

    [HarmonyPatch(typeof(Chat))]
    class ChatPatch {
      [HarmonyPostfix]
      [HarmonyPatch(nameof(Chat.Awake))]
      static void AwakePostfix(ref Chat __instance) {
        if (!IsModEnabled.Value) {
          return;
        }

        __instance.m_maxVisibleBufferLength = 80;
        __instance.m_hideDelay = 600;
        __instance.m_chatWindow.GetComponent<RectMask2D>().enabled = false;
        __instance.m_output.gameObject.SetActive(false);

        _chatPanel = new("ChatPanel", typeof(RectTransform));
        _chatPanel.transform.SetParent(__instance.m_chatWindow.transform, worldPositionStays: false);

        RectTransform panelRectTransform = _chatPanel.GetComponent<RectTransform>();
        panelRectTransform.anchorMin = Vector2.zero;
        panelRectTransform.anchorMax = Vector2.zero;
        panelRectTransform.pivot = Vector2.zero;
        panelRectTransform.anchoredPosition = new Vector2(0f, 30f);

        Vector2 sizeDelta = __instance.m_chatWindow.sizeDelta;
        panelRectTransform.sizeDelta = new(sizeDelta.x - 30f, sizeDelta.y);
        _maxWidth = sizeDelta.x - 50f;

        _chatPanel.AddComponent<RectMask2D>();

        _chatViewport = new("ChatPanel.Viewport", typeof(RectTransform));
        _chatViewport.transform.SetParent(_chatPanel.transform, worldPositionStays: false);

        RectTransform viewportRectTransform = _chatViewport.GetComponent<RectTransform>();
        viewportRectTransform.anchorMin = Vector2.zero;
        viewportRectTransform.anchorMax = new(1f, 0f);
        viewportRectTransform.pivot = Vector2.zero;
        viewportRectTransform.anchoredPosition = Vector2.zero;

        _chatMessages = new("ChatPanel.Content", typeof(RectTransform));
        _chatMessages.transform.SetParent(_chatViewport.transform, worldPositionStays: false);

        RectTransform messagesRectTransform = _chatMessages.GetComponent<RectTransform>();
        messagesRectTransform.anchorMin = Vector2.zero;
        messagesRectTransform.anchorMax = new(1f, 0f);
        messagesRectTransform.pivot = Vector2.zero;
        messagesRectTransform.anchoredPosition = Vector2.zero;

        Image messagesImage = _chatMessages.AddComponent<Image>();
        messagesImage.color = Color.clear;

        VerticalLayoutGroup messagesLayoutGroup = _chatMessages.AddComponent<VerticalLayoutGroup>();
        messagesLayoutGroup.childControlWidth = true;
        messagesLayoutGroup.childControlHeight = true;
        messagesLayoutGroup.childForceExpandWidth = false;
        messagesLayoutGroup.childForceExpandHeight = false;
        messagesLayoutGroup.spacing = 10f;
        messagesLayoutGroup.padding = new(10, 10, 10, 10);

        ContentSizeFitter messagesFitter = _chatMessages.AddComponent<ContentSizeFitter>();
        messagesFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        messagesFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        _chatPanelScroll = _chatPanel.AddComponent<ScrollRect>();
        _chatPanelScroll.content = messagesRectTransform;
        _chatPanelScroll.viewport = viewportRectTransform;
        _chatPanelScroll.horizontal = false;
        _chatPanelScroll.vertical = true;
        _chatPanelScroll.scrollSensitivity = 30f;

        _textPrefab = CreateTextPrefab(__instance.m_output);
      }

      static GameObject CreateTextPrefab(Text sourceText) {
        GameObject textPrefab = new("Text", typeof(RectTransform));

        Text text = textPrefab.AddComponent<Text>();
        text.font = sourceText.font;
        text.fontSize = sourceText.fontSize;

        if (sourceText.TryGetComponent(out Outline sourceOutline)) {
          Outline outline = textPrefab.AddComponent<Outline>();
          outline.effectColor = sourceOutline.effectColor;
          outline.effectDistance = sourceOutline.effectDistance;
        }

        return textPrefab;
      }

      [HarmonyPrefix]
      [HarmonyPatch(nameof(Chat.OnNewChatMessage))]
      static void ChatPrefix(Chat __instance, long senderID, Vector3 pos, Talker.Type type, string user, string text) {
        if (!IsModEnabled.Value) {
          return;
        }

        ChatMessage message = new() {
            Timestamp = DateTime.Now, SenderId = senderID, Position = pos, Type = type, User = user, Text = text };

        MessageHistory.Add(message);

        if (_chatMessages) {
          GameObject messageText = CreateChatMessageText(message);
          messageText.transform.SetParent(_chatMessages.transform, worldPositionStays: false);
          messageText.name = "ChatPanel.Message.Text";

          // _chatPanelScroll.verticalNormalizedPosition = 0f;
        }
      }

      static GameObject CreateChatMessageText(ChatMessage message) {
        GameObject prefab = Instantiate(_textPrefab);
        Text text = prefab.GetComponent<Text>();

        switch (message.Type) {
          case Talker.Type.Normal:
            text.text = $"{message.User} > {message.Text}";
            break;

          case Talker.Type.Shout:
            text.text = $"{message.User} > <color=yellow>{message.Text}</color>";
            break;

          case Talker.Type.Whisper:
            text.text = $"{message.User} > <color=#FFFFFFC0>{message.Text}</color>";
            break;

          case Talker.Type.Ping:
            text.text = $"{message.User} > Ping! <color=cyan>{message.Position}</color>";
            break;
        }

        LayoutElement layout = prefab.AddComponent<LayoutElement>();
        layout.preferredWidth = _maxWidth;

        return prefab;
      }
    }

    [HarmonyPatch(typeof(Terminal))]
    class TerminalPatch {
      [HarmonyPostfix]
      [HarmonyPatch(nameof(Terminal.SendInput))]
      static void SendInputPostfix(ref Terminal __instance) {
        if (IsModEnabled.Value && __instance == Chat.m_instance && _chatPanelScroll) {
          _chatPanelScroll.verticalNormalizedPosition = 0f;
        }
      }
    }
  }
}