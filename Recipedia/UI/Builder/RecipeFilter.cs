using ComfyLib;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

namespace Recipedia {
  public class RecipeFilter : MonoBehaviour {
    public RectTransform RectTransform { get; private set; }
    public Image Background { get; private set; }
    public TMP_InputField InputField { get; private set; }

    void Awake() {
      RectTransform = GetComponent<RectTransform>();
      RectTransform
          .SetAnchorMin(Vector2.up)
          .SetAnchorMax(Vector2.one)
          .SetPivot(Vector2.up)
          .SetAnchoredPosition(new(0f, -5f))
          .SetSizeDelta(new(-10f, 35f));

      Background = gameObject.AddComponent<Image>();
      Background
          .SetType(Image.Type.Sliced)
          .SetSprite(UIResources.GetSprite("text_field"))
          .SetColor(Color.white);

      gameObject.AddComponent<RectMask2D>();

      InputField = CreateChildInputField(RectTransform);
    }

    TMP_InputField CreateChildInputField(Transform parentTransform) {
      GameObject row = new("InputField", typeof(RectTransform));
      row.transform.SetParent(parentTransform, worldPositionStays: false);

      row.GetComponent<RectTransform>()
          .SetAnchorMin(Vector2.zero)
          .SetAnchorMax(Vector2.one)
          .SetPivot(new(0.5f, 0.5f))
          .SetAnchoredPosition(Vector2.zero)
          .SetSizeDelta(new(-16f, 0f));

      row.AddComponent<RectMask2D>();

      TextMeshProUGUI label = UIBuilder.CreateTMPLabel(row.transform);
      label.name = "Text";

      label.rectTransform
          .SetAnchorMin(Vector2.zero)
          .SetAnchorMax(Vector2.one)
          .SetAnchoredPosition(Vector2.zero)
          .SetSizeDelta(Vector2.zero);

      label
          .SetAlignment(TextAlignmentOptions.Left)
          .SetTextWrappingMode(TextWrappingModes.NoWrap)
          .SetOverflowMode(TextOverflowModes.Overflow)
          .SetRichText(false)
          .SetText(string.Empty);

      TextMeshProUGUI placeholder = UIBuilder.CreateTMPLabel(row.transform);
      label.name = "Placeholder";

      placeholder.rectTransform
          .SetAnchorMin(Vector2.zero)
          .SetAnchorMax(Vector2.one)
          .SetAnchoredPosition(Vector2.zero)
          .SetSizeDelta(Vector2.zero);

      placeholder
          .SetAlignment(TextAlignmentOptions.Left)
          .SetColor(new(1f, 1f, 1f, 0.3f))
          .SetTextWrappingMode(TextWrappingModes.NoWrap)
          .SetOverflowMode(TextOverflowModes.Overflow)
          .SetRichText(false)
          .SetText("Filter...");

      TMP_InputField inputField = row.AddComponent<TMP_InputField>();
      inputField.textViewport = row.GetComponent<RectTransform>();
      inputField.textComponent = label;
      inputField.placeholder = placeholder;
      inputField.onFocusSelectAll = false;

      return inputField;
    }
  }
}
