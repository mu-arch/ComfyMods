using UnityEngine;
using UnityEngine.UI;

namespace Chatter {
  public class ChatPanel {
    public GameObject Panel { get; private set; }
    public GameObject Viewport { get; private set; }
    public GameObject Content { get; private set; }
    public ScrollRect ScrollRect { get; private set; }
    public Text TextPrefab { get; private set; }

    public ChatPanel(Transform parentTransform, Text parentText) {
      Panel = CreatePanel(parentTransform);
      Viewport = CreateViewport(Panel.transform);
      Content = CreateContent(Viewport.transform);
      ScrollRect = CreateScrollRect(Panel, Viewport, Content);
      TextPrefab = CreateTextPrefab(parentText);
    }

    GameObject CreatePanel(Transform parentTransform) {
      GameObject panel = new("ChatPanel", typeof(RectTransform));
      panel.transform.SetParent(parentTransform, worldPositionStays: false);

      RectTransform panelRectTransform = panel.GetComponent<RectTransform>();
      panelRectTransform.anchorMin = Vector2.zero;
      panelRectTransform.anchorMax = Vector2.zero;
      panelRectTransform.pivot = Vector2.zero;
      panelRectTransform.anchoredPosition = Vector2.zero;

      RectMask2D panelRectMask = panel.AddComponent<RectMask2D>();
      panelRectMask.softness = new(50, 50);

      return panel;
    }

    GameObject CreateViewport(Transform parentTransform) {
      GameObject viewport = new("ChatPanel.Viewport", typeof(RectTransform));
      viewport.transform.SetParent(parentTransform, worldPositionStays: false);

      RectTransform viewportRectTransform = viewport.GetComponent<RectTransform>();
      viewportRectTransform.anchorMin = Vector2.zero;
      viewportRectTransform.anchorMax = new(1f, 0f);
      viewportRectTransform.pivot = Vector2.zero;
      viewportRectTransform.anchoredPosition = Vector2.zero;

      return viewport;
    }

    GameObject CreateContent(Transform parentTransform) {
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

    ScrollRect CreateScrollRect(GameObject panel, GameObject viewport, GameObject content) {
      ScrollRect panelScroll = panel.AddComponent<ScrollRect>();
      panelScroll.viewport = viewport.GetComponent<RectTransform>();
      panelScroll.content = content.GetComponent<RectTransform>();
      panelScroll.horizontal = false;
      panelScroll.vertical = true;
      panelScroll.scrollSensitivity = 30f;

      return panelScroll;
    }

    Text CreateTextPrefab(Text parentText) {
      GameObject textPrefab = new("Text", typeof(RectTransform));

      Text text = textPrefab.AddComponent<Text>();
      text.font = parentText.font;
      text.fontSize = parentText.fontSize;

      if (parentText.TryGetComponent(out Outline parentTextOutline)) {
        Outline textOutline = textPrefab.AddComponent<Outline>();
        textOutline.effectColor = parentTextOutline.effectColor;
        textOutline.effectDistance = parentTextOutline.effectDistance;
      }

      return text;
    }
  }
}
