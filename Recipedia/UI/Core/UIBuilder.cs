using TMPro;

using UnityEngine;

namespace ComfyLib {
  public static class UIBuilder {
    public static TextMeshProUGUI CreateTMPLabel(Transform parentTransform) {
      TextMeshProUGUI label =
          UnityEngine.Object.Instantiate(UnifiedPopup.instance.bodyText, parentTransform, worldPositionStays: false);

      label.name = "Label";
      label.fontSize = 16f;
      label.enableAutoSizing = false;
      label.richText = true;
      label.color = Color.white;
      label.text = string.Empty;

      return label;
    }
  }
}
