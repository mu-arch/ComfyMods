using UnityEngine;
using UnityEngine.EventSystems;

namespace PartyRock {
  public class CardHandCardHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
    RectTransform _rectTransform;
    int _siblingIndex;
    Vector3 _lastPosition;
    Quaternion _lastRotation;

    void Awake() {
      _rectTransform = GetComponent<RectTransform>();
    }

    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData) {
      _lastPosition = _rectTransform.anchoredPosition;
      _rectTransform.SetPosition(new(_lastPosition.x, 0f));

      _lastRotation = _rectTransform.rotation;
      _rectTransform.rotation = Quaternion.identity;

      _siblingIndex = _rectTransform.GetSiblingIndex();
      _rectTransform.SetAsLastSibling();
    }

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData) {
      _rectTransform.anchoredPosition = _lastPosition;
      _rectTransform.rotation = _lastRotation;
      _rectTransform.SetSiblingIndex(_siblingIndex);
    }
  }
}
