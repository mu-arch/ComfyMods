using System;

using UnityEngine;
using UnityEngine.UI;

namespace Chatter {
  public class ChatPanel {
    public GameObject Panel { get; private set; }
    public CanvasGroup CanvasGroup { get; private set; }

    public GameObject Grabber { get; private set; }
    public GameObject Viewport { get; private set; }
    public Image ViewportImage { get; private set; }
    public GameObject Content { get; private set; }
    public Image ContentImage { get; private set; }
    public ScrollRect ScrollRect { get; private set; }
    public GameObject TextPrefab { get; private set; }

    public Image InputFieldImage { get; private set; }
    public InputField InputField { get; private set; }

    public ChatPanel(Transform parentTransform, Text parentText) {
      Panel = CreatePanel(parentTransform);
      CanvasGroup = Panel.GetComponent<CanvasGroup>();

      Viewport = CreateViewport(Panel.transform);
      ViewportImage = Viewport.GetComponent<Image>();
      Content = CreateContent(Viewport.transform);
      ContentImage = Content.GetComponent<Image>();
      ScrollRect = CreateScrollRect(Panel, Viewport, Content);
      TextPrefab = CreateTextPrefab(parentText);
      InputField = CreateChatInputField(Panel.transform);
      InputFieldImage = InputField.transform.parent.GetComponent<Image>();
      Grabber = CreateGrabber(Panel.transform);
    }

    GameObject CreatePanel(Transform parentTransform) {
      GameObject panel = new("ChatPanel", typeof(RectTransform));
      panel.SetParent(parentTransform);

      panel.GetComponent<RectTransform>()
          .SetAnchorMin(new(1f, 0f))
          .SetAnchorMax(new(1f, 0f))
          .SetPivot(new(1f, 0f))
          .SetPosition(Vector2.zero);

      panel.AddComponent<VerticalLayoutGroup>()
          .SetChildControl(width: true, height: true)
          .SetChildForceExpand(width: false, height: false);

      panel.AddComponent<CanvasGroup>();

      return panel;
    }

    static GameObject CreateGrabber(Transform parentTransform) {
      GameObject grabber = new("ChatPanel.Grabber", typeof(RectTransform));
      grabber.SetParent(parentTransform);

      grabber.GetComponent<RectTransform>()
          .SetAnchorMin(Vector2.zero)
          .SetAnchorMax(Vector2.zero)
          .SetPivot(Vector2.zero)
          .SetPosition(Vector2.zero);

      grabber.AddComponent<LayoutElement>()
          .SetFlexible(width: 1f)
          .SetPreferred(height: 5f);

      grabber.AddComponent<Image>()
          .SetColor(new Color32(255, 255, 255, 32))
          .SetRaycastTarget(true);

      return grabber;
    }

    static GameObject CreateViewport(Transform parentTransform) {
      GameObject viewport = new("ChatPanel.Viewport", typeof(RectTransform));
      viewport.SetParent(parentTransform);

      viewport.GetComponent<RectTransform>()
          .SetAnchorMin(Vector2.zero)
          .SetAnchorMax(Vector2.zero)
          .SetPivot(Vector2.zero)
          .SetPosition(Vector2.zero);

      Image viewportImage = viewport.AddComponent<Image>();
      viewportImage.color = PluginConfig.ChatPanelBackgroundColor.Value;
      viewportImage.sprite = CreateGradientSprite();
      viewportImage.raycastTarget = false;

      RectMask2D viewportRectMask = viewport.AddComponent<RectMask2D>();
      viewportRectMask.softness = Vector2Int.RoundToInt(PluginConfig.ChatPanelRectMaskSoftness.Value);

      viewport.AddComponent<LayoutElement>()
          .SetFlexible(width: 1f, height: 1f);

      return viewport;
    }

    InputField CreateChatInputField(Transform parentTransform) {
      GameObject row = new("ChatPanel.InputField", typeof(RectTransform));
      row.transform.SetParent(parentTransform, worldPositionStays: false);

      RectTransform rowRectTransform = row.GetComponent<RectTransform>();
      rowRectTransform.anchorMin = Vector2.zero;
      rowRectTransform.anchorMax = Vector2.zero;
      rowRectTransform.pivot = Vector2.zero;
      rowRectTransform.anchoredPosition = Vector2.zero;

      HorizontalLayoutGroup rowLayoutGroup = row.AddComponent<HorizontalLayoutGroup>();
      rowLayoutGroup.childControlWidth = true;
      rowLayoutGroup.childControlHeight = true;
      rowLayoutGroup.childForceExpandWidth = true;
      rowLayoutGroup.childForceExpandHeight = false;
      rowLayoutGroup.childAlignment = TextAnchor.MiddleLeft;
      rowLayoutGroup.padding = new(left: 10, right: 10, top: 8, bottom: 8);

      Image rowImage = row.AddComponent<Image>();
      Color color = PluginConfig.ChatPanelBackgroundColor.Value;
      rowImage.color = color;

      InputField inputField = CreateInputField(row.transform);
      inputField.targetGraphic = rowImage;
      inputField.transition = Selectable.Transition.ColorTint;

      return inputField;
    }

    InputField CreateInputField(Transform parentTransform) {
      GameObject inputFieldRow = new("ChatPanel.InputField.Row", typeof(RectTransform));
      inputFieldRow.transform.SetParent(parentTransform, worldPositionStays: false);

      RectTransform rowRectTransform = inputFieldRow.GetComponent<RectTransform>();
      rowRectTransform.anchorMin = Vector2.zero;
      rowRectTransform.anchorMax = Vector2.zero;
      rowRectTransform.pivot = Vector2.zero;
      rowRectTransform.anchoredPosition = Vector2.zero;

      HorizontalLayoutGroup rowLayoutGroup = inputFieldRow.AddComponent<HorizontalLayoutGroup>();
      rowLayoutGroup.childControlWidth = true;
      rowLayoutGroup.childControlHeight = true;
      rowLayoutGroup.childForceExpandWidth = true;
      rowLayoutGroup.childForceExpandHeight = false;
      rowLayoutGroup.childAlignment = TextAnchor.MiddleLeft;

      GameObject inputFieldText =
          UnityEngine.Object.Instantiate(TextPrefab, inputFieldRow.transform, worldPositionStays: false);
      inputFieldText.name = "ChatPanel.InputField.Row.Text";

      RectTransform textRectTransform = inputFieldText.GetComponent<RectTransform>();
      textRectTransform.anchorMin = Vector2.zero;
      textRectTransform.anchorMax = Vector2.zero;
      textRectTransform.pivot = Vector2.zero;
      textRectTransform.anchoredPosition = Vector2.zero;

      InputField inputField = inputFieldRow.AddComponent<InputField>();
      inputField.textComponent = inputFieldText.GetComponent<Text>();

      LayoutElement textLayout = inputFieldRow.AddComponent<LayoutElement>();
      textLayout.flexibleWidth = 1f;

      return inputField;
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
      //contentImage.color = PluginConfig.ChatPanelBackgroundColor.Value;
      //contentImage.sprite = CreateGradientSprite();
      contentImage.color = Color.clear;
      contentImage.raycastTarget = true;

      VerticalLayoutGroup contentLayoutGroup = content.AddComponent<VerticalLayoutGroup>();
      contentLayoutGroup.childControlWidth = true;
      contentLayoutGroup.childControlHeight = true;
      contentLayoutGroup.childForceExpandWidth = false;
      contentLayoutGroup.childForceExpandHeight = false;
      contentLayoutGroup.spacing = PluginConfig.ChatMessageBlockSpacing.Value;
      contentLayoutGroup.padding = new(20, 20, 20, 20);

      ContentSizeFitter contentFitter = content.AddComponent<ContentSizeFitter>();
      contentFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
      contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

      return content;
    }

    static Sprite CreateGradientSprite() {
      Texture2D texture = new(width: 1, height: 2);
      texture.wrapMode = TextureWrapMode.Clamp;
      texture.SetPixel(0, 0, Color.white);
      texture.SetPixel(0, 1, Color.clear);
      texture.Apply();

      return Sprite.Create(texture, new(0, 0, 1, 2), Vector2.zero);
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
      text.font = PluginConfig.MessageFont; // parentText.font;
      text.fontSize = PluginConfig.MessageFontSize; // parentText.fontSize;

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

    public GameObject CreateMessageDivider(Transform parentTransform) {
      GameObject divider = new("Message.Divider", typeof(RectTransform));
      divider.transform.SetParent(parentTransform, worldPositionStays: false);

      Image image = divider.AddComponent<Image>();
      image.color = new Color32(255, 255, 255, 16);
      image.raycastTarget = true;
      image.maskable = true;

      LayoutElement layout = divider.AddComponent<LayoutElement>();
      layout.flexibleWidth = 1f;
      layout.preferredHeight = 1;

      return divider;
    }

    // TODO: get the script from here and attach it to each child element instead of the content-size fitter?
    // https://sushanta1991.blogspot.com/2019/09/force-expand-child-width-in-vertical.html
    public GameObject CreateChatMessageRow(Transform parentTransform) {
      GameObject row = new("Message.Row", typeof(RectTransform));
      row.transform.SetParent(parentTransform, worldPositionStays: false);

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

      return row;
    }

    public GameObject CreateChatMessageRowHeader(Transform parentTransform, ChatMessage message) {
      return CreateChatMessageRowHeader(parentTransform, message.User, message.Timestamp.ToString("T"));
    }

    public GameObject CreateChatMessageRowHeader(Transform parentTransform, string leftText, string rightText) {
      GameObject header = new("Message.Row.Header", typeof(RectTransform));
      header.transform.SetParent(parentTransform, worldPositionStays: false);

      header.AddComponent<HorizontalLayoutGroup>()
          .SetChildControl(width: true, height: true)
          .SetChildForceExpand(width: false, height: false)
          .SetPadding(left: 0, right: 0, top: 0, bottom: 0);

      GameObject leftCell = UnityEngine.Object.Instantiate(TextPrefab, header.transform, worldPositionStays: false);
      leftCell.name = "Header.LeftCell";

      Text leftCellText = leftCell.GetComponent<Text>();
      leftCellText.text = leftText;
      leftCellText.alignment = TextAnchor.MiddleLeft;
      leftCellText.fontSize -= 2;

      leftCell.AddComponent<LayoutElement>();

      GameObject spacer = new("Header.Spacer", typeof(RectTransform));
      spacer.SetParent(header.transform);
      spacer.AddComponent<LayoutElement>().SetFlexible(width: 1f);

      GameObject rightCell = UnityEngine.Object.Instantiate(TextPrefab, header.transform, worldPositionStays: false);
      rightCell.name = "Header.RightCell";

      Text rightCellText = rightCell.GetComponent<Text>();
      rightCellText.text = rightText;
      rightCellText.alignment = TextAnchor.MiddleRight;
      rightCellText.fontSize -= 2;

      rightCell.AddComponent<LayoutElement>();

      return header;
    }

    public GameObject CreateChatMessageRowBody(Transform parentTransform, string text) {
      GameObject body = UnityEngine.Object.Instantiate(TextPrefab, parentTransform, worldPositionStays: false);
      body.name = "Message.Row.Text";

      body.GetComponent<Text>()
          .SetText(text)
          .SetAlignment(TextAnchor.MiddleLeft);

      body.AddComponent<LayoutElement>()
          .SetPreferred(
              width: Panel.GetComponent<RectTransform>().sizeDelta.x + PluginConfig.ChatMessageWidthOffset.Value);

      return body;
    }
  }
}
