using UnityEngine;

namespace PartyRock {
  public class ParentSizeFitter : MonoBehaviour {
    RectTransform _parentRectTransform;
    RectTransform _rectTransform;

    void Awake() {
      _parentRectTransform = transform.parent.GetComponent<RectTransform>();
      _rectTransform = GetComponent<RectTransform>();
    }

    void OnRectTransformDimensionsChange() {
      _rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _parentRectTransform.sizeDelta.x);
    }
  }
}
