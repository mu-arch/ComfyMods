using BepInEx;

using HarmonyLib;

using System;
using System.Collections.Generic;
using System.Linq;
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

      IsModEnabled.OnSettingChanged(toggle => ToggleChatter(toggle));

      _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), harmonyInstanceId: PluginGuid);
    }

    public void OnDestroy() {
      _harmony?.UnpatchSelf();
    }

    public class MessageRow {
      public enum MessageType {
        Text,
        Chat,
        Divider,
        CenterText
      }

      public MessageType RowType { get; }
      public GameObject Row { get; }
      public ChatMessage ChatMessage { get; init; }

      public MessageRow(MessageType type, GameObject row, ChatMessage message = null) {
        RowType = type;
        Row = row;
        ChatMessage = message;
      }
    }

    internal static readonly List<ChatMessage> MessageHistory = new();
    internal static readonly CircularQueue<MessageRow> MessageRows = new(50, row => Destroy(row.Row));

    internal static bool _isPluginConfigBound = false;

    internal static ChatPanel _chatPanel = null;
    internal static ChatMessage _lastMessage = null;
    internal static InputField _chatInputField = null;

    internal static bool _isCreatingChatMessage = false;
    static bool _isChatPanelVisible = false;

    public static void ToggleChatter(bool toggle) {
      ToggleVanillaChat(Chat.m_instance, !toggle);
      ToggleChatPanel(Chat.m_instance, toggle);

      if (Chat.m_instance) {
        Chat.m_instance.m_input = toggle && ChatPanel?.Panel ? ChatPanel.InputField : _chatInputField;
      }
    }

    static void ToggleChatPanel(Chat chat, bool toggle) {
      if (_chatPanel == null || !_chatPanel.Panel) {
        _chatPanel = CreateChatPanel(chat);
      }

      ChatPanel?.Panel.SetActive(toggle);

      if (toggle) {
        ChatPanel?.SetPanelPosition(ChatPanelPosition.Value);
        ChatPanel?.SetPanelSize(ChatPanelSize.Value);
        ChatPanel?.SetContentWidthOffset(ChatContentWidthOffset.Value);
      }
    }

    static ChatPanel CreateChatPanel(Chat chat) {
      if (!chat) {
        return null;
      }

      ChatPanel chatPanel = new(chat.m_chatWindow.transform.parent, chat.m_output);
      RectTransform panelRectTransform = chatPanel.Panel.GetComponent<RectTransform>();
      Outline panelOutline = chatPanel.Panel.GetComponent<Outline>();

      PanelDragger dragger = chatPanel.Grabber.GetComponentInChildren<PanelDragger>();
      dragger.TargetRectTransform = panelRectTransform;
      dragger.TargetOutline = panelOutline;
      dragger.OnEndDragAction = position => ChatPanelPosition.Value = position;

      PanelResizer resizer = chatPanel.Grabber.GetComponentInChildren<PanelResizer>();
      resizer.TargetRectTransform = panelRectTransform;
      resizer.TargetOutline = panelOutline;
      resizer.OnEndDragAction =
          size => {
            ChatPanelSize.Value = size;
            ChatPanelPosition.Value = panelRectTransform.anchoredPosition;
          };

      return chatPanel;
    }

    static void ToggleVanillaChat(Chat chat, bool toggle) {
      if (chat) {
        chat.m_chatWindow.GetComponent<RectMask2D>().enabled = toggle;

        foreach (Image image in chat.m_chatWindow.GetComponentsInChildren<Image>(includeInactive: true)) {
          image.gameObject.SetActive(toggle);
        }

        chat.m_output.gameObject.SetActive(toggle);
      }
    }

    internal static void HideChatPanelDelegate(float hideTimer) {
      if (IsModEnabled.Value) {
        _isChatPanelVisible = hideTimer < HideChatPanelDelay.Value || Menu.IsVisible();

        if (_chatPanel?.CanvasGroup) {
          _chatPanel.CanvasGroup.alpha = _isChatPanelVisible ? 1f : HideChatPanelAlpha.Value;
          _chatPanel.CanvasGroup.blocksRaycasts = _isChatPanelVisible;
        }

        if (!_isChatPanelVisible) {
          _chatPanel.SetVerticalScrollPosition(0f);
        }
      }
    }

    internal static void EnableChatPanelDelegate() {
      if (IsModEnabled.Value && _chatPanel?.InputField) {
        _chatPanel.InputField.enabled = true;
      }
    }

    internal static bool DisableChatPanelDelegate(bool active) {
      if (IsModEnabled.Value && _chatPanel?.InputField) {
        _chatPanel.InputField.enabled = false;
        return true;
      }

      return active;
    }

    internal static void BindChatConfig(Chat chat, ChatPanel chatPanel) {
      if (_isPluginConfigBound) {
        return;
      }

      _isPluginConfigBound = true;

      BindChatMessageFont(chat.Ref()?.m_output.font);
      BindChatPanelSize(chat.Ref()?.m_chatWindow);

      ChatMessageFont.OnSettingChanged(font => ChatPanel?.SetFont(MessageFont));
      ChatMessageFontSize.OnSettingChanged(size => ChatPanel?.SetFontSize(size));

      ChatPanelBackgroundColor.OnSettingChanged(color => ChatPanel?.SetPanelBackgroundColor(color));
      ChatPanelRectMaskSoftness.OnSettingChanged(softness => ChatPanel?.SetPanelRectMaskSoftness(softness));

      ChatPanelPosition.OnSettingChanged(position => ChatPanel?.SetPanelPosition(position));
      ChatPanelSize.OnSettingChanged(size => ChatPanel?.SetPanelSize(size));
      ChatContentWidthOffset.OnSettingChanged(offset => ChatPanel?.SetContentWidthOffset(offset));

      ChatPanelContentSpacing.OnSettingChanged(spacing => ChatPanel?.SetContentSpacing(spacing));
      ShowChatPingMessages.OnSettingChanged(toggle => ToggleContentRows(toggle, Talker.Type.Ping));
      ShowChatPanelMessageDividers.OnSettingChanged(ToggleChatPanelMessageDividers);

      ChatMessageTextDefaultColor.OnSettingChanged(
          color => SetContentRowBodyTextColor(color, MessageRow.MessageType.Text));
      ChatMessageTextMessageHudColor.OnSettingChanged(
          color => SetContentRowBodyTextColor(color, MessageRow.MessageType.CenterText));
      ChatMessageTextSayColor.OnSettingChanged(color => SetContentRowBodyTextColor(color, Talker.Type.Normal));
      ChatMessageTextShoutColor.OnSettingChanged(color => SetContentRowBodyTextColor(color, Talker.Type.Shout));
      ChatMessageTextWhisperColor.OnSettingChanged(color => SetContentRowBodyTextColor(color, Talker.Type.Whisper));
      ChatMessageTextPingColor.OnSettingChanged(color => SetContentRowBodyTextColor(color, Talker.Type.Ping));
    }

    public static ChatPanel ChatPanel {
      get => _chatPanel?.Panel ? _chatPanel : null;
    }

    public static bool IsChatPanelVisible {
      get => _chatPanel?.Panel ? _isChatPanelVisible : false;
    }

    internal static void AddChatMessageText(ChatMessage message) {
      MessageHistory.Add(message);

      if (!_chatPanel?.Panel) {
        return;
      }

      if (MessageRows.IsEmpty
          || MessageRows.LastItem.RowType != MessageRow.MessageType.Chat
          || _lastMessage == null
          || _lastMessage.SenderId != message.SenderId
          || _lastMessage.Type != message.Type) {
        AddDivider();

        GameObject row = _chatPanel.CreateChatMessageRow(_chatPanel.Content.transform);
        MessageRows.EnqueueItem(new(MessageRow.MessageType.Chat, row, message));

        _chatPanel.CreateChatMessageRowHeader(row.transform, message.User, message.Timestamp.ToString("T"));

        row.SetActive(
            message.Type switch {
              Talker.Type.Ping => ShowChatPingMessages.Value,
              _ => true,
            });
      }

      CreateContentChatMessage(MessageRows.LastItem.Row.transform, message);
      _lastMessage = message;
    }

    static GameObject CreateContentChatMessage(Transform parentTransform, ChatMessage message) {
      string text = message.Type switch {
        Talker.Type.Ping => $"{message.Position}",
        _ => message.Text,
      };

      Color color = message.Type switch {
        Talker.Type.Normal => ChatMessageTextDefaultColor.Value,
        Talker.Type.Shout => ChatMessageTextShoutColor.Value,
        Talker.Type.Whisper => ChatMessageTextWhisperColor.Value,
        Talker.Type.Ping => ChatMessageTextPingColor.Value,
        _ => ChatMessageTextDefaultColor.Value,
      };

      GameObject body = _chatPanel.CreateChatMessageRowBody(parentTransform, text);
      body.GetComponent<Text>().SetColor(color);

      return body;
    }

    static void ToggleChatPanelMessageDividers(bool toggle) {
      foreach (MessageRow row in MessageRows.Where(row => row.RowType == MessageRow.MessageType.Divider)) {
        row.Row.Ref()?.SetActive(toggle);
      }
    }

    static void ToggleContentRows(bool toggle, Talker.Type talkerType) {
      foreach (
          MessageRow row in MessageRows
              .Where(row => row.RowType == MessageRow.MessageType.Chat && row.ChatMessage?.Type == talkerType)) {
        row.Row.Ref()?.SetActive(toggle);
      }
    }

    static void SetContentRowBodyTextColor(Color color, MessageRow.MessageType messageType) {             
      foreach (
          Text text in MessageRows
              .Where(row => row.RowType == messageType && row.Row)
              .SelectMany(row => row.Row.GetComponentsInChildren<Text>(includeInactive: true))
              .Where(text => text.name == ChatPanel.ContentRowBodyName)) {
        text.color = color;
      }
    }

    static void SetContentRowBodyTextColor(Color color, Talker.Type talkerType) {
      foreach (
          Text text in MessageRows
              .Where(
                  row => row.RowType == MessageRow.MessageType.Chat && row.ChatMessage?.Type == talkerType && row.Row)
              .SelectMany(row => row.Row.GetComponentsInChildren<Text>(includeInactive: true))
              .Where(text => text.name == ChatPanel.ContentRowBodyName)) {
        text.color = color;
      }
    }

    [HarmonyPatch(typeof(Menu))]
    class MenuPatch {
      [HarmonyPostfix]
      [HarmonyPatch(nameof(Menu.Show))]
      static void ShowPostfix() {
        if (IsModEnabled.Value) {
          ChatPanel?.ToggleGrabber(true);
          ChatPanel?.SetPanelSize(ChatPanelSize.Value);// + new Vector2(0, 400f));
        }
      }

      [HarmonyPostfix]
      [HarmonyPatch(nameof(Menu.Hide))]
      static void HidePostfix() {
        if (IsModEnabled.Value) {
          ChatPanel?.ToggleGrabber(false);
          ChatPanel?.SetPanelSize(ChatPanelSize.Value);
          ChatPanel?.SetVerticalScrollPosition(0f);
        }
      }
    }

    [HarmonyPatch(typeof(MessageHud))]
    class MessageHudPatch {
      [HarmonyPostfix]
      [HarmonyPatch(nameof(MessageHud.ShowMessage))]
      static void ShowMessagePostfix(ref MessageHud.MessageType type, ref string text) {
        if (!IsModEnabled.Value
            || type != MessageHud.MessageType.Center
            || !ChatPanel?.Panel
            || !ShowMessageHudCenterMessages.Value) {
          return;
        }

        if (MessageRows.IsEmpty || MessageRows.LastItem.RowType != MessageRow.MessageType.CenterText) {
          AddDivider();

          GameObject row = _chatPanel.CreateChatMessageRow(_chatPanel.Content.transform);
          MessageRows.EnqueueItem(new(MessageRow.MessageType.CenterText, row));

          _chatPanel.CreateChatMessageRowHeader(row.transform, string.Empty, DateTime.Now.ToString("T"));
        }

        GameObject body = _chatPanel.CreateChatMessageRowBody(MessageRows.LastItem.Row.transform, text);
        body.GetComponent<Text>().SetColor(ChatMessageTextMessageHudColor.Value);
      }
    }

    internal static void AddDivider() {
      if (_chatPanel?.Panel) {
        GameObject divider = _chatPanel.CreateMessageDivider(_chatPanel.Content.transform);
        divider.SetActive(ShowChatPanelMessageDividers.Value);

        MessageRows.EnqueueItem(new(MessageRow.MessageType.Divider, divider));
      }
    }
  }
}