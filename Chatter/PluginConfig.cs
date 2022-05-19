using BepInEx.Configuration;

using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace Chatter {
  public class PluginConfig {
    public static ConfigFile Config { get; private set; }

    public static ConfigEntry<bool> IsModEnabled { get; private set; }

    // Behaviour

    static ConfigEntry<int> _hideChatPanelDelay;
    public static int HideChatPanelDelay {
      get => _hideChatPanelDelay.Value;
    }

    static ConfigEntry<float> _hideChatPanelAlpha;
    public static float HideChatPanelAlpha {
      get => _hideChatPanelAlpha.Value;
    }

    // Content
    static ConfigEntry<bool> _showMessageHudCenterMessages;
    public static bool ShowMessageHudCenterMessages {
      get => _showMessageHudCenterMessages.Value;
    }

    public static ConfigEntry<bool> ShowChatPanelMessageDividers { get; private set; }

    // Style
    public static ConfigEntry<string> ChatMessageFont { get; private set; }
    public static ConfigEntry<int> ChatMessageFontSize { get; private set; }

    public static ConfigEntry<float> ChatPanelContentSpacing { get; private set; }

    public static ConfigEntry<Color> ChatPanelBackgroundColor { get; private set; }
    public static ConfigEntry<Vector2> ChatPanelRectMaskSoftness { get; private set; }

    public static ConfigEntry<Vector2> ChatPanelPosition { get; private set; }
    public static ConfigEntry<Vector2> ChatPanelSize { get; private set; }
    public static ConfigEntry<float> ChatContentWidthOffset { get; private set; }

    public static void BindConfig(ConfigFile config) {
      Config = config;
      IsModEnabled = config.Bind("_Global", "isModEnabled", true, "Globally enable or disable this mod.");

      // Behaviour
      _hideChatPanelDelay =
          config.Bind(
              "Behaviour",
              "hideChatPanelDelay",
              defaultValue: 10,
              new ConfigDescription(
                  "Delay (in seconds) before hiding the ChatPanel.", new AcceptableValueRange<int>(1, 180)));

      _hideChatPanelAlpha =
          config.Bind(
              "Behaviour",
              "hideChatPanelAlpha",
              defaultValue: 0.2f,
              new ConfigDescription(
                  "Color alpha (in %) for the ChatPanel when hidden.", new AcceptableValueRange<float>(0f, 1f)));

      // Content
      _showMessageHudCenterMessages =
          config.Bind(
              "Content",
              "showMessageHudCenterMessages",
              defaultValue: true,
              "Show messages from the MessageHud that display in the top-center (usually boss messages).");

      ShowChatPanelMessageDividers =
          config.Bind(
              "Content",
              "showChatPanelMessageDividers",
              defaultValue: true,
              "Show the horizontal dividers between groups of messages.");

      ChatPanelBackgroundColor =
          config.Bind(
              "Style",
              "chatPanelBackgroundColor",
              (Color) new Color32(0, 0, 0, 128),
              "The background color for the ChatPanel.");

      ChatPanelRectMaskSoftness =
          config.Bind(
              "Style", "chatPanelRectMaskSoftness", new Vector2(20f, 20f), "Softness of the ChatPanel's RectMask2D.");

      ChatPanelContentSpacing =
          config.Bind(
              "Style",
              "chatPanelContentSpacing",
              10f,
              new ConfigDescription(
                  "The spacing (in pixels) between rows in the ChatPanel content.",
                  new AcceptableValueRange<float>(-100, 100)));
    }

    static readonly Dictionary<string, Font> _fontCache = new();

    public static Font MessageFont {
      get {
        if (!_fontCache.TryGetValue(ChatMessageFont.Value, out Font font)) {
          font = Font.CreateDynamicFontFromOSFont(ChatMessageFont.Value, ChatMessageFontSize.Value);
          _fontCache[font.name] = font;
        }

        return font;
      }
    }

    public static void BindChatMessageFont(Font defaultFont) {
      foreach (Font font in Resources.FindObjectsOfTypeAll<Font>()) {
        _fontCache[font.name] = font;
      }

      string[] fontNames =
          _fontCache.Keys.OrderBy(f => f).Concat(Font.GetOSInstalledFontNames().OrderBy(f => f)).ToArray();

      ChatMessageFont =
          Config.Bind(
              "Style",
              "chatMessageFont",
              defaultFont.name,
              new ConfigDescription("The font to use for chat messages.", new AcceptableValueList<string>(fontNames)));

      ChatMessageFontSize =
          Config.Bind(
              "Style",
              "chatMessageFontSize",
              defaultFont.fontSize,
              new ConfigDescription("The font size to use for chat messages.", new AcceptableValueRange<int>(8, 64)));
    }

    public static void BindChatPanelSize(RectTransform chatWindowRectTransform) {
      ChatPanelPosition =
          Config.Bind(
              "Panel",
              "chatPanelPosition",
              chatWindowRectTransform.anchoredPosition,
              "The Vector2 position of the ChatPanel.");

      ChatPanelSize =
          Config.Bind(
              "Panel",
              "chatPanelSize",
              chatWindowRectTransform.sizeDelta - new Vector2(10f, 0f),
              "The size (width, height) of the ChatPanel.");

      ChatContentWidthOffset =
          Config.Bind(
              "Panel",
              "chatContentWidthOffset",
              -50f,
              new ConfigDescription(
                  "Offsets the width of a row in the ChatPanel content.",
                  new AcceptableValueRange<float>(-400, 400)));
    }
  }
}
