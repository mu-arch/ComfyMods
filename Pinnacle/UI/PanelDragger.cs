using UnityEngine;
using UnityEngine.EventSystems;

using System;

namespace Pinnacle {
  public class PanelDragger :
      MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IDragHandler, IEndDragHandler {
    Vector2 _lastMousePosition;
    CanvasGroup _canvasGroup;

    public RectTransform TargetRectTransform;
    public event EventHandler<Vector3> OnPanelEndDrag;

    public void OnPointerEnter(PointerEventData eventData) {
      if (!_canvasGroup) {
        _canvasGroup = GetComponent<CanvasGroup>();
      }

      _canvasGroup.Ref()?.SetAlpha(1f);
    }

    public void OnPointerExit(PointerEventData eventData) {
      _canvasGroup.Ref()?.SetAlpha(0.05f);
    }

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
        OnPanelEndDrag?.Invoke(this, TargetRectTransform.anchoredPosition);
      }
    }
  }
}
