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

        RectTransform panelRectTransform = _chatPanel.Panel.GetComponent<RectTransform>();
        panelRectTransform.sizeDelta = _chatPanelSize;
        panelRectTransform.anchoredPosition = new(0, 30f);

        _maxWidth = _chatPanelSize.x;
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

          GameObject row = _chatPanel.CreateChatMessageRow(_chatPanel.Content.transform);
          _chatPanel.CreateChatMessageRowHeader(row.transform, message);
          _chatPanel.CreateChatMessageRowBody(row.transform, ChatPanel.GetMessageText(message));

          MessageRows.Enqueue(row);

          _lastMessage = message;
          _lastMessageRow = row;
        } else if (_lastMessageRow) {
          _chatPanel.CreateChatMessageRowBody(_lastMessageRow.transform, ChatPanel.GetMessageText(message));
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