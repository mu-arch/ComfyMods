using System;

using UnityEngine;
using UnityEngine.UI;

namespace Pinnacle {
  public class ValueCell {
    public GameObject Cell { get; private set; }
    public Image Background { get; private set; }
    public InputField InputField { get; private set; }

    public ValueCell(Transform parentTransform) {
      Cell = CreateChildCell(parentTransform);
      Background = Cell.Image();

      InputField = CreateChildInputField(Cell.transform).GetComponent<InputField>();
      InputField.SetTargetGraphic(Background);
    }

    GameObject CreateChildCell(Transform parentTransform) {
      GameObject cell = new("Cell", typeof(RectTransform));
      cell.SetParent(parentTransform);

      cell.AddComponent<HorizontalLayoutGroup>()
          .SetChildControl(width: true, height: true)
          .SetChildForceExpand(width: false, height: false)
          .SetPadding(left: 8, right: 8, top: 4, bottom: 4)
          .SetSpacing(8f)
          .SetChildAlignment(TextAnchor.MiddleCenter);

      cell.AddComponent<Image>()
          .SetType(Image.Type.Sliced)
          .SetSprite(UIBuilder.CreateRoundedCornerSprite(64, 64, 8))
          .SetColor(new(0.5f, 0.5f, 0.5f, 0.5f));

      cell.AddComponent<Shadow>()
          .SetEffectDistance(new(2, -2));

      cell.AddComponent<LayoutElement>();

      return cell;
    }

    GameObject CreateChildInputField(Transform parentTransform) {
      GameObject row = new("InputField", typeof(RectTransform));
      row.SetParent(parentTransform);

      row.AddComponent<HorizontalLayoutGroup>()
          .SetChildControl(width: true, height: true)
          .SetChildForceExpand(width: false, height: false)
          .SetChildAlignment(TextAnchor.MiddleLeft)
          .SetSpacing(8f);

      row.AddComponent<LayoutElement>();

      GameObject label = UIBuilder.CreateLabel(row.transform);
      label.SetName("InputField.Text");

      label.Text()
          .SetSupportRichText(false)
          .SetText("InputField.Text");

      label.AddComponent<LayoutElement>()
          .SetFlexible(width: 1f);

      InputField inputField = row.AddComponent<InputField>();

      inputField
          .SetTextComponent(label.Text())
          .SetTransition(Selectable.Transition.ColorTint)
          .SetNavigationMode(Navigation.Mode.None)
          .SetColors(InputFieldColorBlock.Value);

      row.AddComponent<DisableHighlightOnSelect>();

      return row;
    }

    static readonly Lazy<ColorBlock> InputFieldColorBlock =
        new(() =>
          new() {
            normalColor = new Color(1f, 1f, 1f, 0.9f),
            highlightedColor = new Color(0.565f, 0.792f, 0.976f),
            disabledColor = new Color(0.2f, 0.2f, 0.2f, 0.8f),
            pressedColor = new Color(0.647f, 0.839f, 0.655f),
            selectedColor = new Color(1f, 0.878f, 0.51f),
            colorMultiplier = 1f,
            fadeDuration = 0.25f,
          });
  }
}
