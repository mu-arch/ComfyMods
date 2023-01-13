using System.Linq;

using UnityEngine;
using UnityEngine.UI;

namespace Chatter {
  public class ChatPanel {
    public GameObject Panel { get; init; }
    public CanvasGroup CanvasGroup { get; init; }
    public GameObject Grabber { get; init; }
    public GameObject Viewport { get; init; }
    public GameObject Content { get; init; }
    public Image ContentImage { get; init; }
    public ScrollRect ScrollRect { get; init; }
    public GameObject TextPrefab { get; init; }
    public InputField InputField { get; init; }

    public ChatPanel(Transform parentTransform, Text parentText) {
      TextPrefab = CreateTextPrefab(parentText);
      _textPrefabText = TextPrefab.GetComponent<Text>();

      Panel = CreatePanel(parentTransform);
      CanvasGroup = Panel.GetComponent<CanvasGroup>();
      _panelRectTransform = Panel.GetComponent<RectTransform>();

      Viewport = CreateViewport(Panel.transform);
      _viewportRectTransform = Viewport.GetComponent<RectTransform>();
      _viewportRectMask = Viewport.GetComponent<RectMask2D>();
      _viewportImage = Viewport.GetComponent<Image>();

      Content = CreateContent(Viewport.transform);
      ContentImage = Content.GetComponent<Image>();
      _contentRectTransform = Content.GetComponent<RectTransform>();
      _contentLayoutGroup = Content.GetComponent<VerticalLayoutGroup>();

      ScrollRect = CreateScrollRect(Panel, Viewport, Content);

      InputField = CreateChatInputField(Panel.transform);
      _inputFieldImage = InputField.GetComponentInParent<Image>();

      Grabber = CreateGrabber(Panel.transform);
      _grabberCanvasGroup = Grabber.GetComponent<CanvasGroup>();

      _contentWidthOffset = _contentLayoutGroup.padding.horizontal * -1f;
      _panelRectTransform.SetAsFirstSibling();
    }

    public const string RowName = "Message.Row";
    public const string RowHeaderName = "Message.Row.Header";
    public const string HeaderLeftCellName = "Row.Header.LeftCell";
    public const string HeaderRightCellName = "Row.Header.RightCell";
    public const string ContentRowBodyName = "Message.Row.Text";

    readonly RectTransform _panelRectTransform;
    readonly RectTransform _viewportRectTransform;
    readonly RectMask2D _viewportRectMask;
    readonly Image _viewportImage;
    readonly RectTransform _contentRectTransform;
    readonly VerticalLayoutGroup _contentLayoutGroup;
    readonly Image _inputFieldImage;
    readonly Text _textPrefabText;
    readonly CanvasGroup _grabberCanvasGroup;

    public Toggle SayToggle { get; private set; } = default!;
    public Toggle ShoutToggle { get; private set; } = default!;
    public Toggle WhisperToggle { get; private set; } = default!;
    public Toggle PingToggle { get; private set; } = default!;
    public Toggle MessageHudToggle { get; private set; } = default!;
    public Toggle TextToggle { get; private set; } = default!;

    float _contentWidthOffset = 0f;

    public void SetFont(Font font) {
      _textPrefabText.font = font;

      foreach (Text text in Panel.GetComponentsInChildren<Text>(includeInactive: true)) {
        text.font = font;
      }
    }

    public void SetFontSize(int fontSize) {
      _textPrefabText.fontSize = fontSize;

      foreach (Text text in Panel.GetComponentsInChildren<Text>(includeInactive: true)) {
        text.fontSize = text.name == ContentRowBodyName ? fontSize : fontSize - 2;
      }
    }

    public void SetPanelBackgroundColor(Color color) {
      _viewportImage.color = color;
      _inputFieldImage.color = color;
    }

    public void SetPanelRectMaskSoftness(Vector2 softness) {
      _viewportRectMask.softness = Vector2Int.RoundToInt(softness);
    }

    public void SetPanelPosition(Vector2 position) {
      _panelRectTransform.anchoredPosition = position;
    }

    public void SetPanelSize(Vector2 sizeDelta) {
      _panelRectTransform.sizeDelta = sizeDelta;
      _viewportRectTransform.sizeDelta = sizeDelta;
      SetRowLayoutPreferredWidths();
    }

    public void SetContentWidthOffset(float widthOffset) {
      _contentWidthOffset = widthOffset;
      SetRowLayoutPreferredWidths();
    }

    void SetRowLayoutPreferredWidths() {
      float preferredWidth = _panelRectTransform.sizeDelta.x + _contentWidthOffset;

      foreach (
          LayoutElement layout in Content
              .GetComponentsInChildren<LayoutElement>(includeInactive: true)
              .Where(layout => layout.name == ContentRowBodyName)) {
        layout.preferredWidth = preferredWidth;
      }
    }

    public void SetContentSpacing(float spacing) {
      _contentLayoutGroup.spacing = spacing;
    }

    public void SetContentBodySpacing(float spacing) {
      foreach (
          VerticalLayoutGroup layoutGroup in Content
              .GetComponentsInChildren<VerticalLayoutGroup>(includeInactive: true)
              .Where(lg => lg.name == RowName)) {
        layoutGroup.SetSpacing(spacing);
      }
    }

    public void ToggleGrabber(bool toggle) {
      _grabberCanvasGroup.alpha = toggle ? 1f : 0f;
      _grabberCanvasGroup.blocksRaycasts = toggle;
    }

    public void SetVerticalScrollPosition(float position) {
      ScrollRect.verticalNormalizedPosition = position;
    }

    public void OffsetVerticalScrollPosition(float offset) {
      float percent = (offset / _contentRectTransform.sizeDelta.y);
      ScrollRect.verticalNormalizedPosition += percent;
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

      panel.AddComponent<Image>()
          .SetColor(Color.clear);

      panel.AddComponent<Outline>()
          .SetEffectColor(new Color32(255, 255, 255, 32))
          .SetEffectDistance(Vector2.zero)
          .SetUseGraphicAlpha(false)
          .SetEnabled(false);

      return panel;
    }

    GameObject CreateGrabber(Transform parentTransform) {
      GameObject grabber = new("ChatPanel.Grabber", typeof(RectTransform));
      grabber.SetParent(parentTransform);

      grabber.GetComponent<RectTransform>()
          .SetAnchorMin(Vector2.zero)
          .SetAnchorMax(Vector2.zero)
          .SetPivot(Vector2.zero)
          .SetPosition(Vector2.zero);

      grabber.AddComponent<HorizontalLayoutGroup>()
          .SetChildControl(width: true, height: true)
          .SetChildForceExpand(width: false, height: false);

      grabber.AddComponent<Image>()
          .SetColor(new Color32(255, 255, 255, 32));

      grabber.AddComponent<CanvasGroup>();

      CreateGrabberResizer(grabber.transform);
      CreateGrabberDragger(grabber.transform);
      CreateShowMessageTypeTogglesRow(grabber.transform);

      return grabber;
    }

    GameObject CreateGrabberResizer(Transform parentTransform) {
      GameObject resizer = new("Grabber.Resizer", typeof(RectTransform));
      resizer.SetParent(parentTransform.transform);

      resizer.AddComponent<LayoutElement>()
          .SetPreferred(width: 25f, height: 25f);

      resizer.AddComponent<PanelResizer>();

      resizer.AddComponent<Text>()
          .SetFont(_textPrefabText.font)
          .SetFontSize(10)
          .SetAlignment(TextAnchor.MiddleCenter)
          .SetText("\u2199\u2197")
          .SetColor(Color.white);

      resizer.AddComponent<Outline>()
          .SetEffectColor(Color.black);

      return resizer;
    }

    GameObject CreateGrabberDragger(Transform parentTransform) {
      GameObject dragger = new("Grabber.Dragger", typeof(RectTransform));
      dragger.SetParent(parentTransform.transform);

      dragger.AddComponent<LayoutElement>()
          .SetFlexible(width: 1f)
          .SetPreferred(height: 25f);

      dragger.AddComponent<Image>()
          .SetColor(new Color32(192, 192, 192, 48))
          .SetRaycastTarget(true);

      dragger.AddComponent<PanelDragger>();

      return dragger;
    }

    GameObject CreateShowMessageTypeTogglesRow(Transform parentTransform) {
      GameObject row = new("Show.Toggles.Row", typeof(RectTransform));
      row.SetParent(parentTransform.transform);

      row.AddComponent<HorizontalLayoutGroup>()
          .SetChildControl(width: true, height: true)
          .SetChildForceExpand(width: false, height: false)
          .SetPadding(left: 10, right: 10)
          .SetSpacing(10f);

      row.AddComponent<ContentSizeFitter>()
          .SetHorizontalFit(ContentSizeFitter.FitMode.PreferredSize)
          .SetVerticalFit(ContentSizeFitter.FitMode.PreferredSize);

      row.AddComponent<Image>()
          .SetColor(new Color32(192, 192, 192, 32));

      SayToggle = CreateMessageTypeToggle(row.transform, "Say".ToUpperInvariant());
      ShoutToggle = CreateMessageTypeToggle(row.transform, "Shout".ToUpperInvariant());
      WhisperToggle = CreateMessageTypeToggle(row.transform, "Whisper".ToUpperInvariant());
      PingToggle = CreateMessageTypeToggle(row.transform, "Ping".ToUpperInvariant());
      MessageHudToggle = CreateMessageTypeToggle(row.transform, "Hud".ToUpperInvariant());
      TextToggle = CreateMessageTypeToggle(row.transform, "Text".ToUpperInvariant());

      return row;
    }

    Toggle CreateMessageTypeToggle(Transform parentTransform, string label) {
      GameObject togglePrefab = new($"Toggle.{label}", typeof(RectTransform));
      togglePrefab.SetParent(parentTransform);

      togglePrefab.AddComponent<LayoutElement>()
          .SetPreferred(height: 25f);

      Text toggleText =
          togglePrefab.AddComponent<Text>()
              .SetFont(_textPrefabText.font)
              .SetFontSize(10)
              .SetColor(Color.white)
              .SetAlignment(TextAnchor.MiddleCenter)
              .SetText(label);

      togglePrefab.AddComponent<Outline>()
          .SetEffectColor(Color.black);

      Toggle toggle = togglePrefab.AddComponent<Toggle>();
      toggle.targetGraphic = togglePrefab.GetComponent<Text>();
      toggle.onValueChanged.AddListener(isOn => toggleText.color = isOn ? Color.white : Color.gray);
      toggle.isOn = false;

      return toggle;
    }

    static GameObject CreateViewport(Transform parentTransform) {
      GameObject viewport = new("ChatPanel.Viewport", typeof(RectTransform));
      viewport.SetParent(parentTransform);

      viewport.GetComponent<RectTransform>()
          .SetAnchorMin(Vector2.zero)
          .SetAnchorMax(Vector2.zero)
          .SetPivot(Vector2.zero)
          .SetPosition(Vector2.zero);

      viewport.AddComponent<Image>()
          .SetColor(PluginConfig.ChatPanelBackgroundColor.Value)
          .SetSprite(CreateGradientSprite())
          .SetRaycastTarget(false);

      RectMask2D viewportRectMask = viewport.AddComponent<RectMask2D>();
      viewportRectMask.softness = Vector2Int.RoundToInt(PluginConfig.ChatPanelRectMaskSoftness.Value);

      viewport.AddComponent<LayoutElement>()
          .SetFlexible(width: 1f, height: 1f);

      return viewport;
    }

    InputField CreateChatInputField(Transform parentTransform) {
      GameObject row = new("ChatPanel.InputField", typeof(RectTransform));
      row.SetParent(parentTransform, worldPositionStays: false);

      row.GetComponent<RectTransform>()
          .SetAnchorMin(Vector2.zero)
          .SetAnchorMax(Vector2.zero)
          .SetPivot(Vector2.zero)
          .SetPosition(Vector2.zero);

      row.AddComponent<HorizontalLayoutGroup>()
          .SetChildControl(width: true, height: true)
          .SetChildForceExpand(width: true, height: false)
          .SetChildAlignment(TextAnchor.MiddleLeft)
          .SetPadding(left: 20, right: 20, top: 8, bottom: 8);

      Image rowImage = row.AddComponent<Image>().SetColor(PluginConfig.ChatPanelBackgroundColor.Value);

      InputField inputField = CreateInputField(row.transform);
      inputField.targetGraphic = rowImage;
      inputField.transition = Selectable.Transition.ColorTint;

      return inputField;
    }

    InputField CreateInputField(Transform parentTransform) {
      GameObject inputFieldRow = new("ChatPanel.InputField.Row", typeof(RectTransform));
      inputFieldRow.transform.SetParent(parentTransform, worldPositionStays: false);

      inputFieldRow.GetComponent<RectTransform>()
          .SetAnchorMin(Vector2.zero)
          .SetAnchorMax(Vector2.zero)
          .SetPivot(Vector2.zero)
          .SetPosition(Vector2.zero);

      inputFieldRow.AddComponent<HorizontalLayoutGroup>()
          .SetChildControl(width: true, height: true)
          .SetChildForceExpand(width: false, height: false)
          .SetChildAlignment(TextAnchor.MiddleLeft);

      GameObject inputFieldText = Object.Instantiate(TextPrefab, inputFieldRow.transform, worldPositionStays: false);
      inputFieldText.name = "ChatPanel.InputField.Row.Text";

      inputFieldText.GetComponent<RectTransform>()
          .SetAnchorMin(Vector2.zero)
          .SetAnchorMax(Vector2.zero)
          .SetPivot(Vector2.zero)
          .SetPosition(Vector2.zero);

      inputFieldText.GetComponent<Text>()
          .SetSupportRichText(false);

      inputFieldText.AddComponent<LayoutElement>()
          .SetFlexible(width: 1f);

      GameObject inputFieldPlaceholder = Object.Instantiate(TextPrefab, inputFieldRow.transform, false);
      inputFieldPlaceholder.SetName("Placeholder");

      inputFieldPlaceholder.GetComponent<RectTransform>()
          .SetAnchorMin(Vector2.zero)
          .SetAnchorMax(Vector2.one)
          .SetPivot(new(0f, 0.5f))
          .SetPosition(Vector2.zero);

      inputFieldPlaceholder.GetComponent<Text>()
          .SetAlignment(TextAnchor.MiddleLeft)
          .SetColor(new(1f, 1f, 1f, 0.3f))
          .SetText("...");

      inputFieldPlaceholder.AddComponent<LayoutElement>()
          .SetIgnoreLayout(true);

      InputField inputField = inputFieldRow.AddComponent<InputField>();
      inputField.textComponent = inputFieldText.GetComponent<Text>();
      inputField.placeholder = inputFieldPlaceholder.GetComponent<Text>();

      inputFieldRow.AddComponent<LayoutElement>()
          .SetFlexible(width: 1f);

      return inputField;
    }

    static GameObject CreateContent(Transform parentTransform) {
      GameObject content = new("ChatPanel.Content", typeof(RectTransform));
      content.SetParent(parentTransform);

      content.GetComponent<RectTransform>()
          .SetAnchorMin(Vector2.zero)
          .SetAnchorMax(new(1f, 0f))
          .SetPivot(Vector2.zero)
          .SetPosition(Vector2.zero);

      content.AddComponent<Image>()
          .SetColor(Color.clear)
          .SetRaycastTarget(true);

      content.AddComponent<VerticalLayoutGroup>()
          .SetChildControl(width: true, height: true)
          .SetChildForceExpand(width: false, height: false)
          .SetSpacing(PluginConfig.ContentRowSpacing)
          .SetPadding(left: 20, right: 20, top: 20, bottom: 20);

      content.AddComponent<ContentSizeFitter>()
          .SetHorizontalFit(ContentSizeFitter.FitMode.PreferredSize)
          .SetVerticalFit(ContentSizeFitter.FitMode.PreferredSize);

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

      textPrefab.AddComponent<Text>()
          .SetFont(PluginConfig.MessageFont)
          .SetFontSize(PluginConfig.ChatMessageFontSize.Value);

      if (parentText.TryGetComponent(out Outline parentTextOutline)) {
        textPrefab.AddComponent<Outline>()
            .SetEffectColor(parentTextOutline.effectColor)
            .SetEffectDistance(parentTextOutline.effectDistance);
      }

      return textPrefab;
    }

    public GameObject CreateMessageDivider(Transform parentTransform) {
      GameObject divider = new("Message.Divider", typeof(RectTransform));
      divider.SetParent(parentTransform, worldPositionStays: false);

      divider.AddComponent<Image>()
          .SetColor(new Color32(255, 255, 255, 16))
          .SetRaycastTarget(true)
          .SetMaskable(true);

      divider.AddComponent<LayoutElement>()
          .SetFlexible(width: 1f)
          .SetPreferred(height: 1f);

      return divider;
    }

    public GameObject CreateChatMessageRow(Transform parentTransform) {
      GameObject row = new(RowName, typeof(RectTransform));
      row.SetParent(parentTransform, worldPositionStays: false);

      row.AddComponent<VerticalLayoutGroup>()
          .SetChildControl(width: true, height: true)
          .SetChildForceExpand(width: false, height: false)
          .SetPadding(left: 0, right: 0, top: 0, bottom: 0)
          .SetSpacing(PluginConfig.ContentRowBodySpacing);

      row.AddComponent<ContentSizeFitter>()
          .SetHorizontalFit(ContentSizeFitter.FitMode.PreferredSize)
          .SetVerticalFit(ContentSizeFitter.FitMode.PreferredSize);

      return row;
    }

    public (GameObject header, GameObject leftCell, GameObject rightCell) CreateChatMessageRowHeader(
        Transform parentTransform, string leftText, string rightText) {
      GameObject header = new(RowHeaderName, typeof(RectTransform));
      header.SetParent(parentTransform, worldPositionStays: false);

      header.AddComponent<HorizontalLayoutGroup>()
          .SetChildControl(width: true, height: true)
          .SetChildForceExpand(width: false, height: false)
          .SetPadding(left: 0, right: 0, top: 0, bottom: 0);

      GameObject leftCell = Object.Instantiate(TextPrefab, header.transform, worldPositionStays: false);
      leftCell.SetName(HeaderLeftCellName);

      leftCell.GetComponent<Text>()
          .SetText(leftText)
          .SetAlignment(TextAnchor.MiddleLeft)
          .SetFontSize(_textPrefabText.fontSize - 2);

      leftCell.AddComponent<LayoutElement>();

      GameObject spacer = new("Header.Spacer", typeof(RectTransform));
      spacer.SetParent(header.transform);
      spacer.AddComponent<LayoutElement>().SetFlexible(width: 1f);

      GameObject rightCell = Object.Instantiate(TextPrefab, header.transform, worldPositionStays: false);
      rightCell.SetName(HeaderRightCellName);

      rightCell.GetComponent<Text>()
          .SetText(rightText)
          .SetAlignment(TextAnchor.MiddleRight)
          .SetFontSize(_textPrefabText.fontSize - 2);

      rightCell.AddComponent<LayoutElement>();

      return (header, leftCell, rightCell);
    }

    public GameObject CreateChatMessageRowBody(Transform parentTransform, string text) {
      GameObject body = Object.Instantiate(TextPrefab, parentTransform, worldPositionStays: false);
      body.name = ContentRowBodyName;

      body.GetComponent<Text>()
          .SetText(text)
          .SetAlignment(TextAnchor.MiddleLeft);

      body.AddComponent<LayoutElement>()
          .SetPreferred(width: _panelRectTransform.sizeDelta.x + _contentWidthOffset);

      return body;
    }
  }
}
