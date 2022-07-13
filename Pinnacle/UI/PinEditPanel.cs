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
      PinName.Value.InputField.onEndEdit.AddListener(OnPinNameValueChange);

      PinIconSelectorLabelRow = new(Panel.transform);
      PinIconSelectorLabelRow.Label
          .SetAlignment(TextAnchor.UpperLeft)
          .SetText("Icon");

      PinIconSelector = new(PinIconSelectorLabelRow.Row.transform);
      PinIconSelector.OnPinIconClicked += (_, pinType) => OnPinTypeValueChange(pinType);

      PinType = new(Panel.transform);
      PinType.Label.SetText("Type");

      PinPositionLabelRow = new(Panel.transform);
      PinPositionLabelRow.Label.SetText("Position");

      PinPosition = new(PinPositionLabelRow.Row.transform);
      PinPosition.XValue.InputField.textComponent.SetColor(new(1f, 0.878f, 0.51f));
      PinPosition.XValue.InputField.onEndEdit.AddListener(_ => OnPinPositionValueChange());
      PinPosition.YValue.InputField.textComponent.SetColor(new(0.565f, 0.792f, 0.976f));
      PinPosition.YValue.InputField.onEndEdit.AddListener(_ => OnPinPositionValueChange());
      PinPosition.ZValue.InputField.textComponent.SetColor(new(0.647f, 0.839f, 0.655f));
      PinPosition.ZValue.InputField.onEndEdit.AddListener(_ => OnPinPositionValueChange());

      float labelWidth =
          GetPreferredWidth(PinName.Label, PinIconSelectorLabelRow.Label, PinType.Label, PinPositionLabelRow.Label);
      float valueWidth = 200f;

      SetPreferredWidths(labelWidth, valueWidth, PinName, PinType);
      PinPositionLabelRow.Label.GetComponent<LayoutElement>().SetPreferred(width: labelWidth);
      PinIconSelectorLabelRow.Label.GetComponent<LayoutElement>().SetPreferred(width: labelWidth);
    }

    public Minimap.PinData TargetPin { get; private set; }

    public void SetTargetPin(Minimap.PinData pin) {
      TargetPin = pin;

      if (pin == null) {
        return;
      }

      PinName.Value.InputField.text = pin.m_name;

      PinIconSelector.UpdateIcons(pin.m_type);
      PinType.Value.InputField.text = pin.m_type.ToString();

      PinPosition.XValue.InputField.text = $"{pin.m_pos.x:F0}";
      PinPosition.YValue.InputField.text = $"{pin.m_pos.y:F0}";
      PinPosition.ZValue.InputField.text = $"{pin.m_pos.z:F0}";
    }

    void OnPinNameValueChange(string name) {
      if (TargetPin == null) {
        return;
      }

      TargetPin.m_name = name;
      TargetPin.m_nameElement.SetText(name);
    }

    void OnPinTypeValueChange(Minimap.PinType pinType) {
      if (TargetPin == null) {
        return;
      }

      TargetPin.m_type = pinType;
      TargetPin.m_icon = Minimap.m_instance.GetSprite(pinType);
      TargetPin.m_iconElement.SetSprite(TargetPin.m_icon);

      PinIconSelector.UpdateIcons(pinType);
      PinType.Value.InputField.text = pinType.ToString();
    }

    void OnPinPositionValueChange() {
      if (TargetPin == null) {
        return;
      }

      if (!float.TryParse(PinPosition.XValue.InputField.text, out float x)
          || !float.TryParse(PinPosition.YValue.InputField.text, out float y)
          || !float.TryParse(PinPosition.ZValue.InputField.text, out float z)) {
        return;
      }

      TargetPin.m_pos = new(x, y, z);
      TargetPin.m_uiElement.SetPosition(GetMapImagePosition(TargetPin.m_pos));

      Pinnacle.CenterMapOnPinPosition(TargetPin.m_pos);
    }

    static Vector2 GetMapImagePosition(Vector3 mapPosition) {
      Minimap.m_instance.WorldToMapPoint(mapPosition, out float mx, out float my);
      return Minimap.m_instance.MapPointToLocalGuiPos(mx, my, Minimap.m_instance.m_mapImageLarge);
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
