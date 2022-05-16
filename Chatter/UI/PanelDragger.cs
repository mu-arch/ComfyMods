using UnityEngine.EventSystems;
using UnityEngine;
using System;

namespace Chatter {
  public class PanelDragger : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {
    Vector2 _lastMousePosition;

    [SerializeField]
    public RectTransform TargetTransform { get; set; }

    [SerializeField]
    public Action EndDragAction { get; set; }

    public void OnBeginDrag(PointerEventData eventData) {
      _lastMousePosition = eventData.position;
    }

    public void OnDrag(PointerEventData eventData) {
      Vector2 difference = eventData.position - _lastMousePosition;

      TargetTransform.position += new Vector3(difference.x, difference.y, transform.position.z);
      _lastMousePosition = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData) {
      EndDragAction();
    }
  }
}
