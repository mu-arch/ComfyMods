using BepInEx;

using HarmonyLib;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using UnityEngine;
using UnityEngine.UI;

using static Chatter.PluginConfig;
using static UnityEngine.ColorUtility;

namespace Chatter {
  [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
  public class Chatter : BaseUnityPlugin {
    public const string PluginGuid = "redseiko.valheim.chatter";
    public const string PluginName = "Chatter";
    public const string PluginVersion = "1.1.0";

    Harmony _harmony;

    public void Awake() {
      BindConfig(Config);

      IsModEnabled.OnSettingChanged(toggle => ToggleChatter(toggle));

      _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), harmonyInstanceId: PluginGuid);
    }

    public void OnDestroy() {
      _harmony?.UnpatchSelf();
    }

    public enum MessageLayoutType {
      WithHeaderRow,
      SingleRow
    }

    public enum ContentRowType {
      Text,
      Chat,
      Divider,
      CenterText
    }

    public class ContentRow {
      public ContentRowType Type { get; }
      public GameObject Row { get; }
      public GameObject Divider { get; }

      public ChatMessage ChatMessage { get; init; }
      public ContentRow(
          ContentRowType type, GameObject row, ChatMessage message = default, GameObject divider = default) {
        Type = type;
        Row = row;
        ChatMessage = message;
        Divider = divider;
      }
    }

    internal static readonly List<ChatMessage> MessageHistory = new();
    internal static readonly CircularQueue<ContentRow> MessageRows = new(50, row => Destroy(row.Row));

    internal static bool _isPluginConfigBound = false;

    internal static ChatPanel _chatPanel = null;
    internal static ChatMessage _lastChatMessage = null;
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

      chatPanel.SayToggle.onValueChanged.AddListener(isOn => ToggleContentRows(isOn, Talker.Type.Normal));
      chatPanel.SayToggle.isOn = true;

      chatPanel.ShoutToggle.onValueChanged.AddListener(isOn => ToggleContentRows(isOn, Talker.Type.Shout));
      chatPanel.ShoutToggle.isOn = true;

      chatPanel.PingToggle.onValueChanged.AddListener(isOn => ToggleContentRows(isOn, Talker.Type.Ping));
      chatPanel.PingToggle.onValueChanged.Invoke(false);

      chatPanel.WhisperToggle.onValueChanged.AddListener(isOn => ToggleContentRows(isOn, Talker.Type.Whisper));
      chatPanel.WhisperToggle.isOn = true;

      chatPanel.MessageHudToggle.onValueChanged.AddListener(
          isOn => ToggleContentRows(isOn, ContentRowType.CenterText));
      chatPanel.MessageHudToggle.isOn = true;

      chatPanel.TextToggle.onValueChanged.AddListener(isOn => ToggleContentRows(isOn, ContentRowType.Text));
      chatPanel.TextToggle.isOn = true;

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
      ShowChatPanelMessageDividers.OnSettingChanged(ToggleChatPanelMessageDividers);

      ChatMessageTextDefaultColor.OnSettingChanged(color => SetContentRowBodyTextColor(color, ContentRowType.Text));
      ChatMessageTextMessageHudColor.OnSettingChanged(
          color => SetContentRowBodyTextColor(color, ContentRowType.CenterText));
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

    static void SetChatMessageLayoutType(MessageLayoutType layoutType) {
      if (layoutType == MessageLayoutType.SingleRow) {

      }
    }

    internal static void AddChatMessageText(ChatMessage message) {
      MessageHistory.Add(message);

      if (!_chatPanel?.Panel) {
        return;
      }

      AddChatMessage(message);

      _lastChatMessage = message;
    }

    static void AddChatMessageWithHeaderRow(ChatMessage message) {
      if (MessageRows.IsEmpty
          || MessageRows.LastItem?.Type != ContentRowType.Chat
          || _lastChatMessage == null
          || _lastChatMessage?.SenderId != message.SenderId
          || _lastChatMessage?.TalkerType != message.TalkerType) {
        GameObject divider = AddDivider();
        GameObject row = _chatPanel.CreateChatMessageRow(_chatPanel.Content.transform);
        MessageRows.EnqueueItem(new(ContentRowType.Chat, row, message, divider));

        _chatPanel.CreateChatMessageRowHeader(
            row.transform,
            $"{ChatMessageUsernamePrefix.Value}{message.Username}{ChatMessageUsernamePostfix.Value}",
            message.Timestamp.ToString("T"));

        bool active = IsTalkerTypeActive(message.TalkerType);
        divider.SetActive(active && ShowChatPanelMessageDividers.Value);
        row.SetActive(active);
      }

      _chatPanel
          .CreateChatMessageRowBody(MessageRows.LastItem.Row.transform, GetTalkerTypeText(message))
          .GetComponent<Text>()
          .SetColor(GetTalkerTypeColor(message.TalkerType));
    }

    static void AddChatMessageAsSingleRow(ChatMessage message) {
      GameObject row = _chatPanel.CreateChatMessageRow(_chatPanel.Content.transform);
      MessageRows.EnqueueItem(new(ContentRowType.Chat, row, message));

      _chatPanel.CreateChatMessageRowBody(
          row.transform,
          string.Format(
              "{0} [ {1}{2}{3} ] <color=#{4}>{5}</color>",
              GetTimestampText(message.Timestamp),
              ChatMessageUsernamePrefix.Value,
              message.Username,
              ChatMessageUsernamePostfix.Value,
              ToHtmlStringRGBA(GetTalkerTypeColor(message.TalkerType)),
              GetTalkerTypeText(message)));

      row.SetActive(IsTalkerTypeActive(message.TalkerType));
    }

    static bool IsTalkerTypeActive(Talker.Type talkerType) {
      return talkerType switch {
        Talker.Type.Normal => _chatPanel.SayToggle.isOn,
        Talker.Type.Shout => _chatPanel.ShoutToggle.isOn,
        Talker.Type.Whisper => _chatPanel.WhisperToggle.isOn,
        Talker.Type.Ping => _chatPanel.PingToggle.isOn,
        _ => true,
      };
    }

    static string GetTalkerTypeText(ChatMessage message) {
      return message.TalkerType switch {
        Talker.Type.Ping => $"Ping! {message.Position}",
        _ => message.Text,
      };
    }

    static Color GetTalkerTypeColor(Talker.Type talkerType) {
      return talkerType switch {
        Talker.Type.Normal => ChatMessageTextDefaultColor.Value,
        Talker.Type.Shout => ChatMessageTextShoutColor.Value,
        Talker.Type.Whisper => ChatMessageTextWhisperColor.Value,
        Talker.Type.Ping => ChatMessageTextPingColor.Value,
        _ => ChatMessageTextDefaultColor.Value,
      };
    }

    static void ToggleChatPanelMessageDividers(bool toggle) {
      foreach (ContentRow row in MessageRows.Where(row => row.Type == ContentRowType.Divider)) {
        row.Row.Ref()?.SetActive(toggle);
      }
    }

    static void ToggleContentRows(bool toggle, ContentRowType rowType) {
      foreach (ContentRow row in MessageRows.Where(row => row.Type == rowType)) {
        row.Row.Ref()?.SetActive(toggle);
        row.Divider.Ref()?.SetActive(toggle && ShowChatPanelMessageDividers.Value);
      }
    }

    static void ToggleContentRows(bool toggle, Talker.Type talkerType) {
      foreach (
          ContentRow row in MessageRows
              .Where(row => row.Type == ContentRowType.Chat && row.ChatMessage?.TalkerType == talkerType)) {
        row.Row.Ref()?.SetActive(toggle);
        row.Divider.Ref()?.SetActive(toggle && ShowChatPanelMessageDividers.Value);
      }
    }

    static void SetContentRowBodyTextColor(Color color, ContentRowType messageType) {             
      foreach (
          Text text in MessageRows
              .Where(row => row.Type == messageType && row.Row)
              .SelectMany(row => row.Row.GetComponentsInChildren<Text>(includeInactive: true))
              .Where(text => text.name == ChatPanel.ContentRowBodyName)) {
        text.color = color;
      }
    }

    static void SetContentRowBodyTextColor(Color color, Talker.Type talkerType) {
      foreach (
          Text text in MessageRows
              .Where(row => row.Type == ContentRowType.Chat && row.ChatMessage?.TalkerType == talkerType && row.Row)
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

        AddChatMessage(new() { MessageType = ChatMessageType.HudCenter, Timestamp = DateTime.Now, Text = text});
      }
    }

    public static void AddChatMessage(ChatMessage message) {
      if (ShouldCreateDivider(message)) {
        GameObject divider = _chatPanel.CreateMessageDivider(_chatPanel.Content.transform);
        GameObject row = _chatPanel.CreateChatMessageRow(_chatPanel.Content.transform);
        MessageRows.EnqueueItem(new(ContentRowType.Chat, row, message, divider));

        bool active = IsMessageTypeActive(message.MessageType);
        divider.SetActive(active && ShouldShowDivider());
        row.SetActive(active);

        if (ShouldCreateRowHeader()) {
          (_, GameObject leftCell, GameObject rightCell) = 
              _chatPanel.CreateChatMessageRowHeader(
                  row.transform, GetUsernameText(message.Username), GetTimestampText(message.Timestamp));

          leftCell.GetComponent<Text>().SetColor(ChatMessageTextDefaultColor.Value);

          rightCell.GetComponent<Text>().SetColor(ChatMessageTimestampColor.Value);
          rightCell.SetActive(ChatMessageShowTimestamp.Value);
        }
      }

      string bodyText = ChatMessageLayout.Value switch {
        MessageLayoutType.SingleRow =>
            string.Join(
                " ", GetTimestampText(message.Timestamp), GetUsernameText(message.Username), GetMessageText(message)),
        _ => GetMessageText(message)
      };

      _chatPanel.CreateChatMessageRowBody(MessageRows.LastItem.Row.transform, bodyText)
          .GetComponent<Text>()
          .SetColor(GetMessageTextColor(message.MessageType));
    }

    static bool ShouldCreateDivider(ChatMessage message) {
      return MessageRows.IsEmpty
          || MessageRows.LastItem?.ChatMessage?.MessageType != message.MessageType
          || MessageRows.LastItem?.ChatMessage?.SenderId != message.SenderId;
    }

    static bool ShouldCreateRowHeader() {
      return ChatMessageLayout.Value == MessageLayoutType.WithHeaderRow;
    }

    static bool ShouldShowDivider() {
      return ShowChatPanelMessageDividers.Value && ChatMessageLayout.Value == MessageLayoutType.WithHeaderRow;
    }

    static bool IsMessageTypeActive(ChatMessageType messageType) {
      return messageType switch {
        ChatMessageType.Text => _chatPanel.TextToggle.isOn,
        ChatMessageType.HudCenter => _chatPanel.MessageHudToggle.isOn,
        ChatMessageType.Say => _chatPanel.SayToggle.isOn,
        ChatMessageType.Shout => _chatPanel.ShoutToggle.isOn,
        ChatMessageType.Whisper => _chatPanel.WhisperToggle.isOn,
        ChatMessageType.Ping => _chatPanel.PingToggle.isOn,
        _ => true,
      };
    }

    static string GetUsernameText(string username) {
      if (username.Length == 0) {
        return string.Empty;
      }

      return ChatMessageLayout.Value switch {
        MessageLayoutType.WithHeaderRow =>
            $"{ChatMessageUsernamePrefix.Value}{username}{ChatMessageUsernamePostfix.Value}",
        MessageLayoutType.SingleRow =>
            $"<color=#{ToHtmlStringRGBA(ChatMessageTextDefaultColor.Value)}>[ {username} ]</color>",
        _ => username,
      };
    }

    static string GetTimestampText(DateTime timestamp) {
      return ChatMessageLayout.Value switch {
        MessageLayoutType.SingleRow => 
            ChatMessageShowTimestamp.Value
                ? string.Format(
                      "<color=#{0}>{1:t}</color>", ToHtmlStringRGBA(ChatMessageTimestampColor.Value), timestamp)
                : string.Empty,
        _ => timestamp.ToString("T"),
      };
    }

    static string GetMessageText(ChatMessage message) {
      string text =
          message.MessageType switch {
            ChatMessageType.Ping => $"Ping! {message.Position}",
            _ => message.Text
          };

      return ChatMessageLayout.Value switch {
        MessageLayoutType.SingleRow =>
            string.Format("<color=#{0}>{1}</color>", ToHtmlStringRGBA(GetMessageTextColor(message.MessageType)), text),
        _ => text,
      };
    }

    static Color GetMessageTextColor(ChatMessageType messageType) {
      return messageType switch {
        ChatMessageType.Text => ChatMessageTextDefaultColor.Value,
        ChatMessageType.HudCenter => ChatMessageTextMessageHudColor.Value,
        ChatMessageType.Say => ChatMessageTextSayColor.Value,
        ChatMessageType.Shout => ChatMessageTextShoutColor.Value,
        ChatMessageType.Whisper => ChatMessageTextWhisperColor.Value,
        ChatMessageType.Ping => ChatMessageTextPingColor.Value,
        _ => ChatMessageTextDefaultColor.Value,
      };
    }

    static void AddMessageHudText(string text) {
      if (ChatMessageLayout.Value == MessageLayoutType.WithHeaderRow) {
        if (MessageRows.IsEmpty || MessageRows.LastItem.Type != ContentRowType.CenterText) {
          GameObject divider = AddDivider();
          divider.SetActive(_chatPanel.MessageHudToggle.isOn && ShowChatPanelMessageDividers.Value);

          GameObject row = _chatPanel.CreateChatMessageRow(_chatPanel.Content.transform);
          MessageRows.EnqueueItem(new(ContentRowType.CenterText, row, divider: divider));

          _chatPanel.CreateChatMessageRowHeader(row.transform, string.Empty, DateTime.Now.ToString("T"));
          row.SetActive(_chatPanel.MessageHudToggle.isOn);
        }

        _chatPanel
            .CreateChatMessageRowBody(MessageRows.LastItem.Row.transform, text)
            .GetComponent<Text>()
            .SetColor(ChatMessageTextMessageHudColor.Value);
      } else if (ChatMessageLayout.Value == MessageLayoutType.SingleRow) {
        GameObject row = _chatPanel.CreateChatMessageRow(_chatPanel.Content.transform);
        MessageRows.EnqueueItem(new(ContentRowType.CenterText, row));

        _chatPanel.CreateChatMessageRowBody(
            row.transform,
            string.Format(
                "{0} <color=#{1}>{2}</color>",
                GetTimestampText(DateTime.Now),
                ToHtmlStringRGBA(ChatMessageTextMessageHudColor.Value),
                text));

        row.SetActive(_chatPanel.MessageHudToggle.isOn);
      }
    }

    public static void AddTerminalText(string text) {
      if (MessageRows.IsEmpty
          || MessageRows.LastItem.Type != ContentRowType.Text
          || ChatMessageLayout.Value == MessageLayoutType.SingleRow) {
        GameObject divider = ChatMessageLayout.Value == MessageLayoutType.WithHeaderRow ? AddDivider() : default;
        GameObject row = _chatPanel.CreateChatMessageRow(_chatPanel.Content.transform);
        MessageRows.EnqueueItem(new(ContentRowType.Text, row, divider: divider));

        row.SetActive(_chatPanel.TextToggle.isOn);
      }

      _chatPanel.CreateChatMessageRowBody(MessageRows.LastItem.Row.transform, text)
          .GetComponent<Text>()
          .SetColor(ChatMessageTextDefaultColor.Value);
    }

    internal static GameObject AddDivider() {
      if (!_chatPanel?.Panel) {
        return default;
      }

      GameObject divider = _chatPanel.CreateMessageDivider(_chatPanel.Content.transform);
      divider.SetActive(ShowChatPanelMessageDividers.Value);

      MessageRows.EnqueueItem(new(ContentRowType.Divider, divider));

      return divider;
    }
  }
}