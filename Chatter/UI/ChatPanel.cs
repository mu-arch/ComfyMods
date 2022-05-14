using System.Collections.Concurrent;

using UnityEngine;
using UnityEngine.UI;

namespace Chatter {
  public class ChatPanel {
    public GameObject Panel { get; private set; }
    public GameObject Viewport { get; private set; }
    public GameObject Content { get; private set; }
    public GameObject TextPrefab { get; private set; }
    public ScrollRect ScrollRect { get; private set; }

    readonly ConcurrentQueue<GameObject> _contentRowsCache = new();

    public ChatPanel(Transform parentTransform, Text parentText) {
      Panel = CreatePanel(parentTransform);
      Viewport = CreateViewport(Panel.transform);
      Content = CreateContent(Viewport.transform);
      ScrollRect = CreateScrollRect(Panel, Viewport, Content);
      TextPrefab = CreateTextPrefab(parentText);
    }

    static GameObject CreatePanel(Transform parentTransform) {
      GameObject panel = new("ChatPanel", typeof(RectTransform));
      panel.transform.SetParent(parentTransform, worldPositionStays: false);

      RectTransform panelRectTransform = panel.GetComponent<RectTransform>();
      panelRectTransform.anchorMin = Vector2.zero;
      panelRectTransform.anchorMax = Vector2.zero;
      panelRectTransform.pivot = Vector2.zero;
      panelRectTransform.anchoredPosition = Vector2.zero;

      RectMask2D panelRectMask = panel.AddComponent<RectMask2D>();
      panelRectMask.softness = new(25, 25);

      return panel;
    }

    static GameObject CreateViewport(Transform parentTransform) {
      GameObject viewport = new("ChatPanel.Viewport", typeof(RectTransform));
      viewport.transform.SetParent(parentTransform, worldPositionStays: false);

      RectTransform viewportRectTransform = viewport.GetComponent<RectTransform>();
      viewportRectTransform.anchorMin = Vector2.zero;
      viewportRectTransform.anchorMax = new(1f, 0f);
      viewportRectTransform.pivot = Vector2.zero;
      viewportRectTransform.anchoredPosition = Vector2.zero;

      return viewport;
    }

    static GameObject CreateContent(Transform parentTransform) {
      GameObject content = new("ChatPanel.Content", typeof(RectTransform));
      content.transform.SetParent(parentTransform, worldPositionStays: false);

      RectTransform contentRectTransform = content.GetComponent<RectTransform>();
      contentRectTransform.anchorMin = Vector2.zero;
      contentRectTransform.anchorMax = new(1f, 0f);
      contentRectTransform.pivot = Vector2.zero;
      contentRectTransform.anchoredPosition = Vector2.zero;

      Image contentImage = content.AddComponent<Image>();
      contentImage.color = Color.clear;
      contentImage.raycastTarget = true;

      VerticalLayoutGroup contentLayoutGroup = content.AddComponent<VerticalLayoutGroup>();
      contentLayoutGroup.childControlWidth = true;
      contentLayoutGroup.childControlHeight = true;
      contentLayoutGroup.childForceExpandWidth = false;
      contentLayoutGroup.childForceExpandHeight = false;
      contentLayoutGroup.spacing = 10f;
      contentLayoutGroup.padding = new(10, 10, 10, 10);

      ContentSizeFitter contentFitter = content.AddComponent<ContentSizeFitter>();
      contentFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
      contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

      return content;
    }

    static ScrollRect CreateScrollRect(GameObject panel, GameObject viewport, GameObject content) {
      ScrollRect panelScroll = panel.AddComponent<ScrollRect>();
      panelScroll.viewport = viewport.GetComponent<RectTransform>();
      panelScroll.content = content.GetComponent<RectTransform>();
      panelScroll.horizontal = false;
      panelScroll.vertical = true;
      panelScroll.scrollSensitivity = 30f;

      return panelScroll;
    }

    static GameObject CreateTextPrefab(Text parentText) {
      GameObject textPrefab = new("Text", typeof(RectTransform));

      Text text = textPrefab.AddComponent<Text>();
      text.font = parentText.font;
      text.fontSize = parentText.fontSize;

      if (parentText.TryGetComponent(out Outline parentTextOutline)) {
        Outline textOutline = textPrefab.AddComponent<Outline>();
        textOutline.effectColor = parentTextOutline.effectColor;
        textOutline.effectDistance = parentTextOutline.effectDistance;
      }

      return textPrefab;
    }

    public GameObject AddChatMessage(ChatMessage message) {
      GameObject row = CreateChatMessageRow(Content.transform);
      CreateChatMessageRowHeader(row.transform, message);
      CreateChatMessageRowBody(row.transform, GetMessageText(message));

      return row;
    }

    public static string GetMessageText(ChatMessage message) {
      return message.Type switch {
        Talker.Type.Normal => $"{message.Text}",
        Talker.Type.Shout => $"<color=yellow>{message.Text}</color>",
        Talker.Type.Whisper => $"<color=purple>{message.Text}</color>",
        Talker.Type.Ping => $"Ping! <color=cyan>{message.Position}</color>",
        _ => string.Empty,
      };
    }

    public GameObject CreateChatMessageRow(Transform parentTransform) {
      GameObject row = new("Message.Row", typeof(RectTransform));
      row.transform.SetParent(parentTransform, worldPositionStays: false);

      VerticalLayoutGroup rowLayoutGroup = row.AddComponent<VerticalLayoutGroup>();
      rowLayoutGroup.childControlWidth = true;
      rowLayoutGroup.childControlHeight = true;
      rowLayoutGroup.childForceExpandWidth = false;
      rowLayoutGroup.childForceExpandHeight = false;
      rowLayoutGroup.padding = new(0, 0, 0, 0);
      rowLayoutGroup.spacing = 3f;

      ContentSizeFitter rowFitter = row.AddComponent<ContentSizeFitter>();
      rowFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
      rowFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

      return row;
    }

    public GameObject CreateChatMessageRowHeader(Transform parentTransform, ChatMessage message) {
      GameObject header = new("Message.Row.Header", typeof(RectTransform));
      header.transform.SetParent(parentTransform, worldPositionStays: false);

      HorizontalLayoutGroup headerLayoutGroup = header.AddComponent<HorizontalLayoutGroup>();
      headerLayoutGroup.childControlWidth = true;
      headerLayoutGroup.childControlHeight = true;
      headerLayoutGroup.childForceExpandWidth = false;
      headerLayoutGroup.childForceExpandHeight = false;
      headerLayoutGroup.padding = new(left: 0, right: 0, top: 0, bottom: -5); // Balance out the row spacing.

      GameObject username = Object.Instantiate(TextPrefab, header.transform, worldPositionStays: false);
      username.name = "Header.Username";

      Text usernameText = username.GetComponent<Text>();
      usernameText.text = message.User;
      usernameText.alignment = TextAnchor.MiddleLeft;
      usernameText.fontSize -= 2;

      username.AddComponent<LayoutElement>();

      GameObject spacer = new("Header.Spacer", typeof(RectTransform));
      spacer.transform.SetParent(header.transform, worldPositionStays: false);

      LayoutElement spacerLayout = spacer.AddComponent<LayoutElement>();
      spacerLayout.flexibleWidth = 1f;

      GameObject timestamp = Object.Instantiate(TextPrefab, header.transform, worldPositionStays: false);
      timestamp.name = "Header.Timestamp";

      Text timestampText = timestamp.GetComponent<Text>();
      timestampText.text = message.Timestamp.ToShortTimeString();
      timestampText.alignment = TextAnchor.MiddleRight;
      timestampText.fontSize -= 2;

      timestamp.AddComponent<LayoutElement>();

      return header;
    }

    public GameObject CreateChatMessageRowBody(Transform parentTransform, string text) {
      GameObject body = Object.Instantiate(TextPrefab, parentTransform, worldPositionStays: false);
      body.name = "Message.Row.Text";

      Text bodyText = body.GetComponent<Text>();
      bodyText.text = text;
      bodyText.alignment = TextAnchor.MiddleLeft;

      LayoutElement bodyLayout = body.AddComponent<LayoutElement>();
      bodyLayout.preferredWidth = Panel.GetComponent<RectTransform>().sizeDelta.x - 20f;

      return body;
    }
  }
}
