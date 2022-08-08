using UnityEngine;
using UnityEngine.UI;

namespace Pinnacle {
  public class VectorCell {
    public GameObject Cell { get; private set; }

    public ValueCell XValue { get; private set; }
    public Text XLabel { get; private set; }

    public Text YLabel { get; private set; }
    public ValueCell YValue { get; private set; }

    public Text ZLabel { get; private set; }
    public ValueCell ZValue { get; private set; }

    public VectorCell(Transform parentTransform) {
      Cell = CreateChildCell(parentTransform);

      XValue = new(Cell.transform);
      XValue.Cell.LayoutElement().SetFlexible(width: 1f);

      XLabel = UIBuilder.CreateLabel(XValue.Cell.transform).Text();
      XLabel.SetText("X");

      XValue.InputField.textComponent.SetAlignment(TextAnchor.MiddleRight);
      XValue.InputField.GetComponent<LayoutElement>()
          .SetFlexible(width: 1f)
          .SetPreferred(width: UIBuilder.GetPreferredWidth(XLabel, "-99999"));

      YValue = new(Cell.transform);
      YValue.Cell.LayoutElement().SetFlexible(width: 1f);

      YLabel = UIBuilder.CreateLabel(YValue.Cell.transform).Text();
      YLabel.SetText("Y");

      YValue.InputField.textComponent.SetAlignment(TextAnchor.MiddleRight);
      YValue.InputField.GetComponent<LayoutElement>()
          .SetFlexible(width: 1f)
          .SetPreferred(width: UIBuilder.GetPreferredWidth(YLabel, "-99999"));

      ZValue = new(Cell.transform);
      ZValue.Cell.LayoutElement().SetFlexible(width: 1f);

      ZLabel = UIBuilder.CreateLabel(ZValue.Cell.transform).Text();
      ZLabel.SetText("Z");

      ZValue.InputField.textComponent.SetAlignment(TextAnchor.MiddleRight);
      ZValue.InputField.GetComponent<LayoutElement>()
          .SetFlexible(width: 1f)
          .SetPreferred(width: UIBuilder.GetPreferredWidth(ZLabel, "-99999"));
    }

    GameObject CreateChildCell(Transform parentTransform) {
      GameObject cell = new("Cell", typeof(RectTransform));
      cell.SetParent(parentTransform);

      cell.AddComponent<HorizontalLayoutGroup>()
          .SetChildControl(width: true, height: true)
          .SetChildForceExpand(width: false, height: false)
          .SetSpacing(8f)
          .SetChildAlignment(TextAnchor.MiddleCenter);

      return cell;
    }
  }
}
