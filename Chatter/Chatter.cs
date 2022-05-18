using BepInEx;

using HarmonyLib;

using System;
using System.Collections.Concurrent;
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

      IsModEnabled.SettingChanged += (s, ea) => ToggleChatPanel(IsModEnabled.Value);

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

    internal static ChatPanel _chatPanel;
    internal static ChatMessage _lastMessage = null;
    internal static InputField _vanillaInputField;

    internal static bool _isCreatingChatMessage = false;

    internal static void ToggleChatPanel(bool toggle) {
      ToggleVanillaChat(Chat.m_instance, !toggle);

      if (_chatPanel == null || !_chatPanel.Panel) {
        _chatPanel = new(Chat.m_instance.m_chatWindow.transform.parent, Chat.m_instance.m_output);
        _chatPanel.Panel.GetComponent<RectTransform>().SetAsFirstSibling();
      }

      _chatPanel.Panel.SetActive(toggle);

      if (toggle) {
        SetChatPanelSize(ChatPanelSize.Value);
        SetChatMessageRowWidth(ChatMessageWidthOffset.Value);
      }

      if (!_chatPanel.Grabber.TryGetComponent(out PanelDragger panelDragger)) {
        ToggleGrabber(false);
        panelDragger = _chatPanel.Grabber.AddComponent<PanelDragger>();
        panelDragger.TargetTransform = _chatPanel.Panel.GetComponent<RectTransform>();
        panelDragger.EndDragAction =
            () => ChatWindowPositionOffset.Value = panelDragger.TargetTransform.anchoredPosition;
      }

      SetChatPanelPositionOffset(sender: null, args: null);
      Chat.m_instance.m_input = toggle ? _chatPanel.InputField : _vanillaInputField;
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
      if (IsModEnabled.Value && _chatPanel?.Panel) {
        if (hideTimer < HideChatPanelDelay || Menu.IsVisible()) {
          _chatPanel.CanvasGroup.alpha = 1f;
          _chatPanel.CanvasGroup.blocksRaycasts = true;
        } else {
          _chatPanel.CanvasGroup.alpha = HideChatPanelAlpha;
          _chatPanel.CanvasGroup.blocksRaycasts = false;
        }
      }
    }

    internal static void EnableChatPanelDelegate() {
      if (IsModEnabled.Value && _chatPanel?.Panel) {
        _chatPanel.InputField.enabled = true;
      }
    }

    internal static bool DisableChatPanelDelegate(bool active) {
      if (IsModEnabled.Value && _chatPanel?.Panel) {
        _chatPanel.InputField.enabled = false;
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

      ChatMessageFont.SettingChanged += (s, ea) => SetMessageFont(MessageFont, MessageFontSize);
      ChatMessageFontSize.SettingChanged += (s, ea) => SetMessageFont(MessageFont, MessageFontSize);

      ChatPanelBackgroundColor.SettingChanged += SetChatPanelBackgroundColor;
      ChatPanelRectMaskSoftness.SettingChanged += SetChatPanelRectMaskSoftness;

      ChatPanelSize.SettingChanged += (s, ea) => SetChatPanelSize(ChatPanelSize.Value);
      ChatMessageWidthOffset.SettingChanged += (s, ea) => SetChatMessageRowWidth(ChatMessageWidthOffset.Value);
      ChatWindowPositionOffset.SettingChanged += SetChatPanelPositionOffset;

      ChatMessageBlockSpacing.SettingChanged +=
          (s, ea) => {
            if (_chatPanel?.Panel) {
              _chatPanel.Content.GetComponent<VerticalLayoutGroup>().spacing = ChatMessageBlockSpacing.Value;
            }
          };

      _showChatPanelMessageDividers.SettingChanged +=
          (s, ea) => {
            foreach (MessageRow row in MessageRows.Where(row => row.RowType == MessageRow.MessageType.Divider)) {
              row.Row.SetActive(ShowChatPanelMessageDividers);
            }
          };
    }

    [HarmonyPatch(typeof(Terminal))]
    class TerminalPatch {
      [HarmonyTranspiler]
      [HarmonyPatch(nameof(Terminal.UpdateInput))]
      static IEnumerable<CodeInstruction> UpdateInputTranspiler(IEnumerable<CodeInstruction> instructions) {
        return new CodeMatcher(instructions)
            .MatchForward(
                useEnd: false,
                new CodeMatch(OpCodes.Ldarg_0),
                new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(Terminal), nameof(Terminal.m_input))),
                new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(Component), "get_gameObject")),
                new CodeMatch(OpCodes.Ldc_I4_0),
                new CodeMatch(OpCodes.Callvirt, typeof(GameObject).GetMethod(nameof(GameObject.SetActive))),
                new CodeMatch(OpCodes.Ret))
            .Advance(offset: 4)
            .InsertAndAdvance(Transpilers.EmitDelegate<Func<bool, bool>>(DisableChatPanelDelegate))
            .InstructionEnumeration();
      }

      [HarmonyPostfix]
      [HarmonyPatch(nameof(Terminal.SendInput))]
      static void SendInputPostfix(ref Terminal __instance) {
        if (IsModEnabled.Value && __instance == Chat.m_instance && _chatPanel?.ScrollRect) {
          _chatPanel.ScrollRect.verticalNormalizedPosition = 0f;
        }
      }

      [HarmonyPostfix]
      [HarmonyPatch(nameof(Terminal.AddString), typeof(string))]
      static void AddStringFinalPostfix(ref Terminal __instance, ref string text) {
        if (!IsModEnabled.Value || __instance != Chat.m_instance || !_chatPanel?.Panel || _isCreatingChatMessage) {
          return;
        }

        if (MessageRows.IsEmpty || MessageRows.LastItem.RowType != MessageRow.MessageType.Text) {
          AddDivider();

          MessageRows.EnqueueItem(
            new(MessageRow.MessageType.Text, _chatPanel.CreateChatMessageRow(_chatPanel.Content.transform)));
        }

        _chatPanel.CreateChatMessageRowBody(MessageRows.LastItem.Row.transform, text);
      }
    }

    [HarmonyPatch(typeof(Menu))]
    class MenuPatch {
      [HarmonyPostfix]
      [HarmonyPatch(nameof(Menu.Show))]
      static void ShowPostfix() {
        if (IsModEnabled.Value && _chatPanel != null && _chatPanel.Panel) {
          ToggleGrabber(true);
          _chatPanel.Panel.GetComponent<RectTransform>().sizeDelta += new Vector2(0, 200f);
        }
      }

      [HarmonyPostfix]
      [HarmonyPatch(nameof(Menu.Hide))]
      static void HidePostfix() {
        if (IsModEnabled.Value && _chatPanel != null && _chatPanel.Panel) {
          ToggleGrabber(false);
          _chatPanel.Panel.GetComponent<RectTransform>().sizeDelta = ChatPanelSize.Value;
          _chatPanel.ScrollRect.verticalNormalizedPosition = 0f;
        }
      }
    }

    static void ToggleGrabber(bool toggle) {
      if (_chatPanel != null && _chatPanel.Grabber && _chatPanel.Grabber.TryGetComponent(out Image image)) {
        image.raycastTarget = toggle;
        Color color = image.color;
        color.a = toggle ? 0.1f : 0;
        image.color = color;
      }
    }

    [HarmonyPatch(typeof(MessageHud))]
    class MessageHudPatch {
      [HarmonyPostfix]
      [HarmonyPatch(nameof(MessageHud.ShowMessage))]
      static void ShowMessagePostfix(ref MessageHud.MessageType type, ref string text) {
        if (!IsModEnabled.Value || type != MessageHud.MessageType.Center || !_chatPanel?.Panel) {
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
        divider.SetActive(ShowChatPanelMessageDividers);

        MessageRows.EnqueueItem(new(MessageRow.MessageType.Divider, divider));
      }
    }

    static void SetChatPanelSize(Vector2 sizeDelta) {
      RectTransform panelRectTransform = _chatPanel.Panel.GetComponent<RectTransform>();
      panelRectTransform.sizeDelta = sizeDelta;
      panelRectTransform.anchoredPosition = new(0, 30f);

      _chatPanel.Viewport.GetComponent<RectTransform>().sizeDelta = sizeDelta;
      SetChatMessageRowWidth(ChatMessageWidthOffset.Value);
    }

    static void SetChatMessageRowWidth(float widthoffset) {
      float preferredWidth = _chatPanel.Panel.GetComponent<RectTransform>().sizeDelta.x + widthoffset;

      foreach (
          LayoutElement layout
              in MessageRows
                  .SelectMany(row => row.Row.GetComponentsInChildren<LayoutElement>())
                  .Where(layout => layout.name == "Message.Row.Text")) {
        layout.preferredWidth = preferredWidth;
      }
    }

    static void SetChatPanelPositionOffset(object sender, EventArgs args) {
      if (_chatPanel == null
          || !_chatPanel.Panel
          || !_chatPanel.Panel.TryGetComponent(out RectTransform rectTransform)) {
        return;
      }

      rectTransform.anchoredPosition = ChatWindowPositionOffset.Value;
    }

    static void SetChatPanelBackgroundColor(object sender, EventArgs args) {
      if (_chatPanel == null || !_chatPanel.Panel || !_chatPanel.ViewportImage || !_chatPanel.InputFieldImage) {
        return;
      }

      _chatPanel.ViewportImage.color = ChatPanelBackgroundColor.Value;
      _chatPanel.InputFieldImage.color = ChatPanelBackgroundColor.Value;
    }

    static void SetChatPanelRectMaskSoftness(object sender, EventArgs args) {
      if (_chatPanel == null || !_chatPanel.Panel || !_chatPanel.Panel.TryGetComponent(out RectMask2D rectMask)) {
        return;
      }

      rectMask.softness = Vector2Int.RoundToInt(ChatPanelRectMaskSoftness.Value);
    }

    static void SetMessageFont(Font font, int fontSize) {
      if (_chatPanel == null || !_chatPanel.Panel) {
        return;
      }

      Text textPrefabText = _chatPanel.TextPrefab.GetComponent<Text>();
      int fontSizeDelta = fontSize - textPrefabText.fontSize;

      IEnumerable<Text> texts =
          _chatPanel.Panel.GetComponentsInChildren<Text>()
              .SelectMany(row => row.GetComponentsInChildren<Text>())
                  .Append(textPrefabText);

      foreach (Text text in texts) {
        text.font = font;
        text.fontSize += fontSizeDelta;
      }
    }
  }
}