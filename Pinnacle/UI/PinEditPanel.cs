using UnityEngine;
using UnityEngine.UI;

namespace Pinnacle {
  public class PinEditPanel {
    public GameObject Panel { get; private set; }
    public LabelValueRow PinName { get; private set; }

    public LabelRow PinIconSelectorLabelRow { get; private set; }
    public PinIconSelector PinIconSelector { get; private set; }

    public LabelValueRow PinType { get; private set; }

    public GameObject PinPosition { get; private set; }
    public ValueCell PositionX { get; private set; }
    public ValueCell PositionY { get; private set; }
    public ValueCell PositionZ { get; private set; }

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

      PinPosition = CreateChildRow(Panel.transform);
      GameObject pinPositionLabel = CreateChildLabel(PinPosition.transform);
      pinPositionLabel.Text().SetText("Position");

      GameObject positionValues = CreateChildRow(PinPosition.transform);
      positionValues.GetComponent<HorizontalLayoutGroup>()
          .SetPadding(left: 0, right: 0, top: 0, bottom: 0)
          .SetSpacing(8f);

      PositionX = new(positionValues.transform);
      CreateChildLabel(PositionX.Cell.transform).SetName("X Label").Text().SetText("X");
      PositionX.Cell.LayoutElement()
          .SetFlexible(width: 1f)
          .SetPreferred(width: -1f);
      PositionX.InputField.GetOrAddComponent<LayoutElement>()
          .SetFlexible(width: 1f)
          .SetPreferred(width: UIBuilder.GetPreferredWidth(pinPositionLabel.Text(), "-99999"));
      PositionX.InputField.textComponent.SetAlignment(TextAnchor.MiddleRight);

      PositionY = new(positionValues.transform);
      CreateChildLabel(PositionY.Cell.transform).SetName("Y Label").Text().SetText("Y");
      PositionY.Cell.LayoutElement()
          .SetFlexible(width: 1f)
          .SetPreferred(width: -1f);
      PositionY.InputField.GetOrAddComponent<LayoutElement>()
          .SetFlexible(width: 1f)
          .SetPreferred(width: UIBuilder.GetPreferredWidth(pinPositionLabel.Text(), "-99999"));
      PositionY.InputField.textComponent.SetAlignment(TextAnchor.MiddleRight);

      PositionZ = new(positionValues.transform);
      CreateChildLabel(PositionZ.Cell.transform).SetName("Z Label").Text().SetText("Z");
      PositionZ.Cell.LayoutElement()
          .SetFlexible(width: 1f)
          .SetPreferred(width: -1f);
      PositionZ.InputField.GetOrAddComponent<LayoutElement>()
          .SetFlexible(width: 1f)
          .SetPreferred(width: UIBuilder.GetPreferredWidth(pinPositionLabel.Text(), "-99999"));
      PositionZ.InputField.textComponent.SetAlignment(TextAnchor.MiddleRight);

      float labelWidth =
          GetPreferredWidth(PinName.Label, PinIconSelectorLabelRow.Label, PinType.Label, pinPositionLabel.Text());
      float valueWidth = 200f;

      SetPreferredWidths(labelWidth, valueWidth, PinName, PinType);
      pinPositionLabel.AddComponent<LayoutElement>().SetPreferred(width: labelWidth);
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
