using System.Collections;

using UnityEngine;
using UnityEngine.EventSystems;

namespace PartyRock {
  public class CardHandCardHover :
      MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IDragHandler, IEndDragHandler {
    RectTransform _rectTransform;
    int _siblingIndex;
    Vector3 _lastPosition;
    Quaternion _lastRotation;
    Vector2 _lastMousePosition;

    Coroutine _lastCoroutine = null;

    void Awake() {
      _rectTransform = GetComponent<RectTransform>();
    }

    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData) {
      if (_lastCoroutine != null) {
        _rectTransform.anchoredPosition = _lastPosition;
        _rectTransform.rotation = _lastRotation;

        StopCoroutine(_lastCoroutine);
      }

      _lastPosition = _rectTransform.anchoredPosition;
      _rectTransform.SetPosition(new(_lastPosition.x, 0f));

      _lastRotation = _rectTransform.rotation;
      _rectTransform.rotation = Quaternion.identity;

      _siblingIndex = _rectTransform.GetSiblingIndex();
      _rectTransform.SetAsLastSibling();
    }

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData) {
      _rectTransform.SetSiblingIndex(_siblingIndex);
      _lastCoroutine = StartCoroutine(LerpToLastPositionRotation(0.5f));
    }

    IEnumerator LerpToLastPositionRotation(float lerpDuration) {
      float timeElapsed = 0f;
      Vector3 startPosition = _rectTransform.anchoredPosition;
      Quaternion startRotation = _rectTransform.rotation;

      while (timeElapsed < lerpDuration) {
        float t = timeElapsed / lerpDuration;
        t = t * t * (3f - 2f * t);

        _rectTransform.anchoredPosition = Vector3.Lerp(startPosition, _lastPosition, t);
        _rectTransform.rotation = Quaternion.Lerp(startRotation, _lastRotation, t);
        timeElapsed += Time.deltaTime;
        yield return null;
      }

      _rectTransform.anchoredPosition = _lastPosition;
      _rectTransform.rotation = _lastRotation;
    }

    void IBeginDragHandler.OnBeginDrag(PointerEventData eventData) {
      _lastMousePosition = eventData.position;
    }

    void IDragHandler.OnDrag(PointerEventData eventData) {
      Vector2 difference = eventData.position - _lastMousePosition;

      _rectTransform.position += new Vector3(difference.x, difference.y, transform.position.z);
      _lastMousePosition = eventData.position;
    }

    void IEndDragHandler.OnEndDrag(PointerEventData eventData) {
      _lastCoroutine = StartCoroutine(LerpToLastPositionRotation(0.5f));
    }
  }
}
