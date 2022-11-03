using UnityEngine;
using UnityEngine.EventSystems;

namespace Pinnacle {
  public class PointerState : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
    public bool IsPointerHovered { get; private set; } = false;

    public void OnPointerEnter(PointerEventData eventData) {
      IsPointerHovered = true;
    }

    public void OnPointerExit(PointerEventData eventData) {
      IsPointerHovered = false;
    }
  }
}
