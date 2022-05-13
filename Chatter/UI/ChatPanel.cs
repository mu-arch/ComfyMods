using UnityEngine;
using UnityEngine.UI;

namespace Chatter {
  public class ChatPanel {
    GameObject _chatPanel;
    GameObject _chatViewport;
    GameObject _chatMessages;

    public GameObject Create() {
      _chatPanel = new("ChatPanel", typeof(RectTransform));

      RectTransform panelRectTransform = _chatPanel.GetComponent<RectTransform>();
      panelRectTransform.anchorMin = Vector2.zero;
      panelRectTransform.anchorMax = Vector2.zero;
      panelRectTransform.pivot = Vector2.zero;
      panelRectTransform.anchoredPosition = Vector2.zero;

      RectMask2D panelRectMask = _chatPanel.AddComponent<RectMask2D>();
      panelRectMask.softness = new(50, 50);

      _chatViewport = new("ChatPanel.Viewport", typeof(RectTransform));
      _chatViewport.transform.SetParent(_chatPanel.transform, worldPositionStays: false);

      RectTransform viewportRectTransform = _chatViewport.GetComponent<RectTransform>();
      viewportRectTransform.anchorMin = Vector2.zero;
      viewportRectTransform.anchorMax = new(1f, 0f);
      viewportRectTransform.pivot = Vector2.zero;
      viewportRectTransform.anchoredPosition = Vector2.zero;

      _chatMessages = new("ChatPanel.Messages", typeof(RectTransform));
      _chatMessages.transform.SetParent(_chatViewport.transform, worldPositionStays: false);

      RectTransform messagesRectTransform = _chatMessages.GetComponent<RectTransform>();
      messagesRectTransform.anchorMin = Vector2.zero;
      messagesRectTransform.anchorMax = new(1f, 0f);
      messagesRectTransform.pivot = Vector2.zero;
      messagesRectTransform.anchoredPosition = Vector2.zero;

      Image messagesImage = _chatMessages.AddComponent<Image>();
      messagesImage.color = Color.clear;
      messagesImage.raycastTarget = true;

      VerticalLayoutGroup messagesLayoutGroup = _chatMessages.AddComponent<VerticalLayoutGroup>();
      messagesLayoutGroup.childControlWidth = true;
      messagesLayoutGroup.childControlHeight = true;
      messagesLayoutGroup.childForceExpandWidth = false;
      messagesLayoutGroup.childForceExpandHeight = false;
      messagesLayoutGroup.spacing = 10f;
      messagesLayoutGroup.padding = new(10, 10, 10, 10);

      ContentSizeFitter messagesFitter = _chatMessages.AddComponent<ContentSizeFitter>();
      messagesFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
      messagesFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

      ScrollRect panelScroll = _chatPanel.AddComponent<ScrollRect>();
      panelScroll.content = messagesRectTransform;
      panelScroll.viewport = viewportRectTransform;
      panelScroll.horizontal = false;
      panelScroll.vertical = true;
      panelScroll.scrollSensitivity = 30f;

      return _chatPanel;
    }
  }
}
