using System;

using UnityEngine;
using UnityEngine.EventSystems;

namespace Pinnacle {
  public class PanelResizer :
      MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IDragHandler, IEndDragHandler {
    Vector2 _lastMousePosition;
    CanvasGroup _canvasGroup;

    public float PosYMultiplier = -0.5f;
    public float SizeXMultiplier = -1f;
    public float SizeYMultiplier = 1f;

    public RectTransform TargetRectTransform { get; set; }
    public event EventHandler<Vector2> OnPanelEndResize;

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
      Vector2 difference = _lastMousePosition - eventData.position;

      if (TargetRectTransform) {
        TargetRectTransform.anchoredPosition += new Vector2(0, PosYMultiplier * difference.y);
        TargetRectTransform.sizeDelta += new Vector2(SizeXMultiplier * difference.x, SizeYMultiplier * difference.y);
      }

      _lastMousePosition = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData) {
      OnPanelEndResize?.Invoke(this, TargetRectTransform.sizeDelta);
    }
  }
}
