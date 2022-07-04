using UnityEngine;
using UnityEngine.UI;

using static ZoneScouter.PluginConfig;
using static ZoneScouter.UIBuilder;

namespace ZoneScouter {
  public class SectorInfoPanel {
    public GameObject Panel { get; private set; }

    public GameObject PositionRow { get; private set; }
    public Text PositionX { get; private set; }
    public Text PositionY { get; private set; }
    public Text PositionZ { get; private set; }

    public SectorInfoPanel(Transform parentTransform) {
      Panel = CreatePanel(parentTransform);

      PositionRow = CreatePositionRow(Panel.transform);

      PositionX = CreatePositionValue(PositionRow.transform).Text();
      PositionX.SetColor(PositionValueXTextColor.Value);
          
      CreatePositionValueLabel(PositionRow.transform).Text().SetText("X");

      PositionY = CreatePositionValue(PositionRow.transform).Text();
      PositionY.SetColor(PositionValueYTextColor.Value);

      CreatePositionValueLabel(PositionRow.transform).Text().SetText("Y");

      PositionZ = CreatePositionValue(PositionRow.transform).Text();
      PositionZ.SetColor(PositionValueZTextColor.Value);

      CreatePositionValueLabel(PositionRow.transform).Text().SetText("Z");
    }

    GameObject CreatePanel(Transform parentTransform) {
      GameObject panel = new("SectorInfo.Panel", typeof(RectTransform));
      panel.SetParent(parentTransform);

      panel.AddComponent<VerticalLayoutGroup>()
          .SetChildControl(width: true, height: true)
          .SetChildForceExpand(width: true, height: false)
          .SetPadding(left: 10, right: 10, top: 10, bottom: 10)
          .SetSpacing(8f);

      panel.AddComponent<ContentSizeFitter>()
          .SetHorizontalFit(ContentSizeFitter.FitMode.Unconstrained)
          .SetVerticalFit(ContentSizeFitter.FitMode.PreferredSize);

      panel.AddComponent<Image>()
          .SetType(Image.Type.Sliced)
          .SetSprite(CreateRoundedCornerSprite(400, 400, 15))
          .SetColor(SectorInfoPanelBackgroundColor.Value);

      panel.AddComponent<CanvasGroup>()
          .SetBlocksRaycasts(false);

      return panel;
    }

    GameObject CreatePositionRow(Transform parentTransform) {
      GameObject row = new("Position.Row", typeof(RectTransform));
      row.SetParent(parentTransform);

      row.AddComponent<HorizontalLayoutGroup>()
          .SetChildControl(width: true, height: true)
          .SetChildForceExpand(width: false, height: false)
          .SetSpacing(8f)
          .SetChildAlignment(TextAnchor.UpperCenter);

      GameObject position = CreateLabel(row.transform);
      position.SetName("Position.Row.Label");

      position.Text()
          .SetAlignment(TextAnchor.UpperLeft)
          .SetFontSize(SectorInfoPanelFontSize.Value)
          .SetText("Position");

      CreateRowSpacer(row.transform);

      return row;
    }

    GameObject CreatePositionValueLabel(Transform parentTransform) {
      GameObject label = CreateLabel(parentTransform);
      label.SetName("Position.Value.Label");

      label.Text()
          .SetFontSize(SectorInfoPanelFontSize.Value)
          .SetAlignment(TextAnchor.UpperLeft);

      return label;
    }

    GameObject CreatePositionValue(Transform parentTransform) {
      GameObject value = CreateLabel(parentTransform);
      value.SetName("Position.Value");

      value.AddComponent<LayoutElement>()
          .SetPreferred(width: 50f);

      value.Text()
          .SetFontSize(SectorInfoPanelFontSize.Value)
          .SetAlignment(TextAnchor.UpperRight);

      return value;
    }
  }
}
