using System;
using System.Collections;

using UnityEngine;
using UnityEngine.EventSystems;

namespace Pinnacle {
  public class PanelResizer :
      MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IDragHandler, IEndDragHandler {
    Vector2 _lastMousePosition;
    CanvasGroup _canvasGroup;

    public RectTransform TargetRectTransform;
    public event EventHandler<Vector2> OnPanelEndResize;

    Coroutine _lerpAlphaCoroutine = null;

    void Awake() {
      _canvasGroup = GetComponent<CanvasGroup>();
    }

    void SetCanvasGroupAlpha(float alpha) {
      if (_lerpAlphaCoroutine != null) {
        StopCoroutine(_lerpAlphaCoroutine);
        _lerpAlphaCoroutine = null;
      }

      if (_canvasGroup.alpha == alpha) {
        return;
      }

      _lerpAlphaCoroutine = StartCoroutine(LerpCanvasGroupAlpha(alpha, 0.25f));
    }

    IEnumerator LerpCanvasGroupAlpha(float targetAlpha, float lerpDuration) {
      float timeElapsed = 0f;
      float sourceAlpha = _canvasGroup.alpha;

      while (timeElapsed < lerpDuration) {
        float t = timeElapsed / lerpDuration;
        t = t * t * (3f - (2f * t));

        _canvasGroup.SetAlpha(Mathf.Lerp(sourceAlpha, targetAlpha, t));
        timeElapsed += Time.deltaTime;

        yield return null;
      }

      _canvasGroup.SetAlpha(targetAlpha);
    }

    public void OnPointerEnter(PointerEventData eventData) {
      SetCanvasGroupAlpha(1f);
    }

    public void OnPointerExit(PointerEventData eventData) {
      if (!eventData.dragging) {
        SetCanvasGroupAlpha(0f);
      }
    }

    public void OnBeginDrag(PointerEventData eventData) {
      SetCanvasGroupAlpha(1f);
      _lastMousePosition = eventData.position;
    }

    public void OnDrag(PointerEventData eventData) {
      Vector2 difference = _lastMousePosition - eventData.position;

      if (TargetRectTransform) {
        TargetRectTransform.anchoredPosition += new Vector2(0, -0.5f * difference.y);
        TargetRectTransform.sizeDelta += new Vector2(-1f * difference.x, difference.y);
      }

      SetCanvasGroupAlpha(1f);
      _lastMousePosition = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData) {
      OnPanelEndResize?.Invoke(this, TargetRectTransform.sizeDelta);
    }
  }
}
