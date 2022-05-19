using BepInEx;

using HarmonyLib;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

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

      public MessageRow(MessageType type, GameObject row) {
        RowType = type;
        Row = row;
      }
    }

    internal static readonly List<ChatMessage> MessageHistory = new();
    internal static readonly CircularQueue<MessageRow> MessageRows = new(50, row => Destroy(row.Row));

    internal static bool _isPluginConfigBound = false;

    internal static ChatPanel _chatPanel = null;
    internal static ChatMessage _lastMessage = null;
    internal static InputField _chatInputField = null;

    internal static bool _isCreatingChatMessage = false;

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

      PanelDragger dragger = chatPanel.Grabber.AddComponent<PanelDragger>();
      dragger.TargetTransform = chatPanel.Panel.GetComponent<RectTransform>();
      dragger.OnEndDragAction = position => ChatPanelPosition.Value = position;

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
      if (IsModEnabled.Value && ChatPanel?.Panel) {
        if (hideTimer < HideChatPanelDelay.Value || Menu.IsVisible()) {
          _chatPanel.CanvasGroup.alpha = 1f;
          _chatPanel.CanvasGroup.blocksRaycasts = true;
        } else {
          _chatPanel.CanvasGroup.alpha = HideChatPanelAlpha.Value;
          _chatPanel.CanvasGroup.blocksRaycasts = false;
        }
      }
    }

    internal static void EnableChatPanelDelegate() {
      if (IsModEnabled.Value && ChatPanel?.InputField.Ref()) {
        ChatPanel.InputField.enabled = true;
      }
    }

    internal static bool DisableChatPanelDelegate(bool active) {
      if (IsModEnabled.Value && ChatPanel?.Panel) {
        ChatPanel.InputField.enabled = false;
        return true;
      }

      return active;
    }

    internal static void BindChatConfig(Chat chat, ChatPanel chatPanel) {
      if (_isPluginConfigBound) {
        ZLog.Log($"PluginConfig already bound, skipping.");
        return;
      }

      ZLog.Log($"Binding PluginConfig...");
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
    }

    public static ChatPanel ChatPanel {
      get => _chatPanel?.Panel.Ref() ? _chatPanel : null;
    }

    static void ToggleChatPanelMessageDividers(bool toggle) {
      foreach (MessageRow row in MessageRows.Where(row => row.RowType == MessageRow.MessageType.Divider)) {
        row.Row.Ref()?.SetActive(toggle);
      }
    }

    [HarmonyPatch(typeof(Menu))]
    class MenuPatch {
      [HarmonyPostfix]
      [HarmonyPatch(nameof(Menu.Show))]
      static void ShowPostfix() {
        if (IsModEnabled.Value) {
          ChatPanel?.ToggleGrabber(true);
          ChatPanel?.SetPanelSize(ChatPanelSize.Value + new Vector2(0, 400f));
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
          MessageRows.EnqueueItem(new(MessageRow.MessageType.Text, row));

          _chatPanel.CreateChatMessageRowHeader(row.transform, string.Empty, DateTime.Now.ToString("T"));
        }

        _chatPanel.CreateChatMessageRowBody(
            MessageRows.LastItem.Row.transform.transform, $"<color=orange>{text}</color>");
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