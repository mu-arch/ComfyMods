using BepInEx;

using HarmonyLib;

using System;
using System.Collections.Concurrent;
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

      IsModEnabled.SettingChanged += (s, ea) => ToggleChatPanel(IsModEnabled.Value);

      _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), harmonyInstanceId: PluginGuid);
    }

    public void OnDestroy() {
      _harmony?.UnpatchSelf();
    }

    static readonly List<ChatMessage> MessageHistory = new();
    static readonly CircularQueue<GameObject> MessageRows = new(50, row => Destroy(row));

    static Vector2 _chatPanelSize;
    static float _maxWidth = 0f;

    static ChatPanel _chatPanel;

    [HarmonyPatch(typeof(Menu))]
    class MenuPatch {
      [HarmonyPostfix]
      [HarmonyPatch(nameof(Menu.Show))]
      static void ShowPostfix() {
        if (IsModEnabled.Value && _chatPanel != null && _chatPanel.Panel.activeSelf) {
          _chatPanel.Panel.GetComponent<RectTransform>().sizeDelta = _chatPanelSize + new Vector2(0, 400f);
          Chat.m_instance.m_hideDelay = 600;
        }
      }

      [HarmonyPostfix]
      [HarmonyPatch(nameof(Menu.Hide))]
      static void HidePostfix() {
        if (IsModEnabled.Value && _chatPanel != null && _chatPanel.Panel.activeSelf) {
          _chatPanel.Panel.GetComponent<RectTransform>().sizeDelta = _chatPanelSize;
          _chatPanel.ScrollRect.verticalNormalizedPosition = 0f;
          Chat.m_instance.m_hideDelay = 8;
        }
      }
    }

    static void ToggleChatPanel(bool toggle) {
      if (!Chat.m_instance) {
        return;
      }

      Chat.m_instance.m_chatWindow.GetComponent<RectMask2D>().enabled = !toggle;
      Chat.m_instance.m_output.gameObject.SetActive(!toggle);

      _chatPanel ??= new(Chat.m_instance.m_chatWindow.transform, Chat.m_instance.m_output);
      _chatPanel.Panel.SetActive(toggle);

      if (toggle) {
        _chatPanelSize = Chat.m_instance.m_chatWindow.sizeDelta;
        _chatPanelSize.x -= 30f;

        _chatPanel.Panel.GetComponent<RectTransform>().sizeDelta = _chatPanelSize;
        _maxWidth = _chatPanelSize.x - 50f;
      }
    }

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
        __instance.m_chatWindow.SetAsFirstSibling();

        ToggleChatPanel(IsModEnabled.Value);
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

      static ChatMessage _lastMessage = null;
      static GameObject _lastMessageRow;

      [HarmonyPrefix]
      [HarmonyPatch(nameof(Chat.OnNewChatMessage))]
      static void ChatPrefix(Chat __instance, long senderID, Vector3 pos, Talker.Type type, string user, string text) {
        if (!IsModEnabled.Value) {
          return;
        }

        ChatMessage message = new() {
            Timestamp = DateTime.Now, SenderId = senderID, Position = pos, Type = type, User = user, Text = text };

        MessageHistory.Add(message);

        if (type == Talker.Type.Ping) {
          // Ignore pings.
          return;
        }

        if (_lastMessage?.SenderId != message.SenderId || _lastMessage?.Type != message.Type) {
          GameObject divider = CreateDivider();
          divider.transform.SetParent(_chatPanel.Content.transform, worldPositionStays: false);
          MessageRows.Enqueue(divider);

          GameObject messageRow = CreateChatMessageRow(message);
          messageRow.transform.SetParent(_chatPanel.Content.transform, worldPositionStays: false);
          MessageRows.Enqueue(messageRow);

          _lastMessage = message;
          _lastMessageRow = messageRow;
        } else if (_lastMessageRow) {
          GameObject rowText = CreateChatMessageText(message);
          rowText.name = "Message.Row.Text";
          rowText.transform.SetParent(_lastMessageRow.transform, worldPositionStays: false);
        }
      }

      static GameObject CreateDivider() {
        GameObject divider = new("Message.Divider", typeof(RectTransform));

        Image image = divider.AddComponent<Image>();
        image.color = new Color32(255, 255, 255, 16);
        image.raycastTarget = true;
        image.maskable = true;

        LayoutElement layout = divider.AddComponent<LayoutElement>();
        layout.flexibleWidth = 0.5f;
        layout.preferredHeight = 1;

        return divider;
      }

      static GameObject CreateChatMessageRow(ChatMessage message) {
        GameObject row = new("Message.Row", typeof(RectTransform));

        VerticalLayoutGroup rowLayoutGroup = row.AddComponent<VerticalLayoutGroup>();
        rowLayoutGroup.childControlWidth = true;
        rowLayoutGroup.childControlHeight = true;
        rowLayoutGroup.childForceExpandWidth = false;
        rowLayoutGroup.childForceExpandHeight = false;
        rowLayoutGroup.padding = new(0, 0, 0, 0);
        rowLayoutGroup.spacing = 5f;

        ContentSizeFitter rowFitter = row.AddComponent<ContentSizeFitter>();
        rowFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        rowFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        GameObject rowHeader = CreateMessageRowHeader(message);
        rowHeader.transform.SetParent(row.transform, worldPositionStays: false);

        GameObject rowText = CreateChatMessageText(message);
        rowText.name = "Message.Row.Text";
        rowText.transform.SetParent(row.transform, worldPositionStays: false);

        return row;
      }

      static GameObject CreateMessageRowHeader(ChatMessage message) {
        GameObject header = new("Message.Row.Header", typeof(RectTransform));

        HorizontalLayoutGroup headerLayoutGroup = header.AddComponent<HorizontalLayoutGroup>();
        headerLayoutGroup.childControlWidth = true;
        headerLayoutGroup.childControlHeight = true;
        headerLayoutGroup.childForceExpandWidth = false;
        headerLayoutGroup.childForceExpandHeight = false;
        headerLayoutGroup.padding = new(left: 0, right: 0, top: 0, bottom: -5); // Balance out the row spacing.

        GameObject username = Instantiate(_chatPanel.TextPrefab.gameObject);
        username.name = "Username";
        username.transform.SetParent(header.transform, worldPositionStays: false);
        username.GetComponent<Text>().text = message.User;
        username.GetComponent<Text>().alignment = TextAnchor.MiddleLeft;
        username.GetComponent<Text>().fontSize -= 2;
        username.AddComponent<LayoutElement>();

        GameObject spacer = new("Spacer", typeof(RectTransform));
        spacer.transform.SetParent(header.transform, worldPositionStays: false);

        spacer.AddComponent<LayoutElement>().flexibleWidth = 1f;

        GameObject timestamp = Instantiate(_chatPanel.TextPrefab.gameObject);
        timestamp.name = "Timestamp";
        timestamp.transform.SetParent(header.transform, worldPositionStays: false);
        timestamp.GetComponent<Text>().text = message.Timestamp.ToShortTimeString();
        timestamp.GetComponent<Text>().alignment = TextAnchor.MiddleRight;
        timestamp.GetComponent<Text>().fontSize -= 2;
        timestamp.AddComponent<LayoutElement>();

        return header;
      }

      static GameObject CreateChatMessageText(ChatMessage message) {
        GameObject prefab = Instantiate(_chatPanel.TextPrefab.gameObject);
        Text text = prefab.GetComponent<Text>();

        switch (message.Type) {
          case Talker.Type.Normal:
            text.text = $"{message.Text}";
            break;

          case Talker.Type.Shout:
            text.text = $"<color=yellow>{message.Text}</color>";
            break;

          case Talker.Type.Whisper:
            text.text = $"<color=purple>{message.Text}</color>";
            break;

          case Talker.Type.Ping:
            text.text = $"Ping! <color=cyan>{message.Position}</color>";
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
        if (IsModEnabled.Value && __instance == Chat.m_instance && _chatPanel?.ScrollRect) {
          _chatPanel.ScrollRect.verticalNormalizedPosition = 0f;
        }
      }
    }
  }

  public class CircularQueue<T> {
    readonly ConcurrentQueue<T> _queue = new();
    readonly Action<T> _dequeueFunc;
    readonly int _capacity;

    public CircularQueue(int capacity, Action<T> dequeueFunc) {
      _capacity = capacity;
      _dequeueFunc = dequeueFunc;
    }

    public void Enqueue(T item) {
      while (_queue.Count + 1 > _capacity) {
        if (!_queue.TryDequeue(out T itemToDequeue)) {
          throw new Exception("Unable to dequeue!");
        }

        _dequeueFunc(itemToDequeue);
      }
    }

    public T Dequeue() {
      return _queue.TryDequeue(out T result) ? result : default;
    }
  }
}