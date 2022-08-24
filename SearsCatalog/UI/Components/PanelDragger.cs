using UnityEngine;
using UnityEngine.EventSystems;

using System;

namespace ComfyLib {
  public class PanelDragger : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {
    public RectTransform TargetRectTransform;
    public event EventHandler<Vector3> PanelOnEndDrag;

    Vector2 _lastMousePosition;

    public void OnBeginDrag(PointerEventData eventData) {
      _lastMousePosition = eventData.position;
    }

    public void OnDrag(PointerEventData eventData) {
      Vector2 difference = eventData.position - _lastMousePosition;

      if (TargetRectTransform) {
        TargetRectTransform.position += new Vector3(difference.x, difference.y, TargetRectTransform.position.z);
      }

      _lastMousePosition = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData) {
      if (TargetRectTransform) {
        PanelOnEndDrag?.Invoke(this, TargetRectTransform.anchoredPosition);
      }
    }
  }
}
