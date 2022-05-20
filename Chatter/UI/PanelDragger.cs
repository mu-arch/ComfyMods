using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using System;

namespace Chatter {
  public class PanelDragger : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {
    Vector2 _lastMousePosition;

    public RectTransform TargetRectTransform { get; set; } = null;
    public Outline TargetOutline { get; set; } = null;
    public Action<Vector3> OnEndDragAction { get; set; } = null;

    public void OnBeginDrag(PointerEventData eventData) {
      TargetOutline.enabled = true;
      _lastMousePosition = eventData.position;
    }

    public void OnDrag(PointerEventData eventData) {
      Vector2 difference = eventData.position - _lastMousePosition;

      TargetRectTransform.position += new Vector3(difference.x, difference.y, transform.position.z);
      _lastMousePosition = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData) {
      TargetOutline.enabled = false;
      OnEndDragAction(TargetRectTransform.anchoredPosition);
    }
  }
}
