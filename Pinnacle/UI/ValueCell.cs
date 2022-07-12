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
          .SetSprite(UIBuilder.CreateRoundedCornerSprite(200, 200, 5))
          .SetColor(new(0.565f, 0.792f, 0.976f, 0.1f));

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
          .SetNavigationMode(Navigation.Mode.None);

      row.AddComponent<DisableHighlightOnSelect>();

      return row;
    }
  }
}
