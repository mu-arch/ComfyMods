using System;

using UnityEngine;
using UnityEngine.EventSystems;

namespace Chatter {
  public class PanelResizer : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {
    Vector2 _lastMousePosition;

    public RectTransform TargetRectTransform { get; set; } = null;
    public Action<Vector2> OnEndDragAction { get; set; } = null;

    public void OnBeginDrag(PointerEventData eventData) {
      _lastMousePosition = eventData.position;
    }

    public void OnDrag(PointerEventData eventData) {
      Vector2 difference = _lastMousePosition - eventData.position;

      if (TargetRectTransform) {
        TargetRectTransform.anchoredPosition += new Vector2(0, -difference.y);
        TargetRectTransform.sizeDelta += new Vector2(difference.x, difference.y);
      }

      _lastMousePosition = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData) {
      OnEndDragAction(TargetRectTransform.sizeDelta);
    }
  }
}
