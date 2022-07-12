using UnityEngine;
using UnityEngine.UI;

namespace Pinnacle {
  public class PinEditPanel {
    public GameObject Panel { get; private set; }
    public LabelValueRow PinName { get; private set; }

    public LabelRow PinIconSelectorLabelRow { get; private set; }
    public PinIconSelector PinIconSelector { get; private set; }

    public LabelValueRow PinType { get; private set; }

    public LabelRow PinPositionLabelRow { get; private set; }
    public VectorCell PinPosition { get; private set; }

    public PinEditPanel(Transform parentTransform) {
      Panel = CreatePanel(parentTransform);

      PinName = new(Panel.transform);
      PinName.Label.SetText("Name");

      PinIconSelectorLabelRow = new(Panel.transform);
      PinIconSelectorLabelRow.Label
          .SetAlignment(TextAnchor.UpperLeft)
          .SetText("Icon");

      PinIconSelector = new(PinIconSelectorLabelRow.Row.transform);

      PinType = new(Panel.transform);
      PinType.Label.SetText("Type");

      PinPositionLabelRow = new(Panel.transform);
      PinPositionLabelRow.Label.SetText("Position");

      PinPosition = new(PinPositionLabelRow.Row.transform);

      float labelWidth =
          GetPreferredWidth(PinName.Label, PinIconSelectorLabelRow.Label, PinType.Label, PinPositionLabelRow.Label);
      float valueWidth = 200f;

      SetPreferredWidths(labelWidth, valueWidth, PinName, PinType);
      PinPositionLabelRow.Label.GetComponent<LayoutElement>().SetPreferred(width: labelWidth);
      PinIconSelectorLabelRow.Label.GetComponent<LayoutElement>().SetPreferred(width: labelWidth);
    }

    static float GetPreferredWidth(params Text[] texts) {
      float width = 0f;

      foreach (Text text in texts) {
        width = Mathf.Max(width, text.GetPreferredWidth());
      }

      return width;
    }

    static void SetPreferredWidths(float labelWidth, float valueWidth, params LabelValueRow[] rows) {
      foreach (LabelValueRow row in rows) {
        row.Label.GetComponent<LayoutElement>().SetPreferred(width: labelWidth);
        row.Value.Cell.GetComponent<LayoutElement>().SetPreferred(width: valueWidth).SetFlexible(width: 1f);
      }
    }

    GameObject CreatePanel(Transform parentTransform) {
      GameObject panel = new("Panel", typeof(RectTransform));
      panel.SetParent(parentTransform);

      panel.AddComponent<VerticalLayoutGroup>()
          .SetChildControl(width: true, height: true)
          .SetChildForceExpand(width: false, height: false)
          .SetPadding(left: 2, right: 2, top: 4, bottom: 4)
          .SetSpacing(4f);

      panel.AddComponent<ContentSizeFitter>()
          .SetHorizontalFit(ContentSizeFitter.FitMode.PreferredSize)
          .SetVerticalFit(ContentSizeFitter.FitMode.PreferredSize);

      panel.AddComponent<Image>()
          .SetType(Image.Type.Sliced)
          .SetSprite(UIBuilder.CreateRoundedCornerSprite(400, 400, 15))
          .SetColor(new(0f, 0f, 0f, 0.9f));

      panel.AddComponent<CanvasGroup>()
          .SetBlocksRaycasts(true);

      return panel;
    }

    GameObject CreateChildRow(Transform parentTransform) {
      GameObject row = new("Row", typeof(RectTransform));
      row.SetParent(parentTransform);

      row.AddComponent<HorizontalLayoutGroup>()
          .SetChildControl(width: true, height: true)
          .SetChildForceExpand(width: false, height: false)
          .SetPadding(left: 8, right: 8, top: 2, bottom: 2)
          .SetSpacing(12f)
          .SetChildAlignment(TextAnchor.MiddleCenter);

      return row;
    }

    GameObject CreateChildLabel(Transform parentTransform) {
      GameObject label = UIBuilder.CreateLabel(parentTransform);
      label.SetName("Label");

      label.Text()
          .SetAlignment(TextAnchor.MiddleLeft)
          .SetText("Label");

      return label;
    }
  }
}
