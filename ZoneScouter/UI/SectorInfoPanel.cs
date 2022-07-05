using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.UI;

using static ZoneScouter.PluginConfig;
using static ZoneScouter.UIBuilder;

namespace ZoneScouter {
  public class SectorInfoPanel {
    public GameObject Panel { get; private set; }

    public ContentRow PositionContent { get; private set; }
    public ValueWithLabel PositionX { get; private set; }
    public ValueWithLabel PositionY { get; private set; }
    public ValueWithLabel PositionZ { get; private set; }

    public ContentRow SectorContent { get; private set; }
    public ValueWithLabel SectorXY { get; private set; }
    public ValueWithLabel SectorZdoCount { get; private set; }

    public GameObject ZdoCountUpperRow { get; private set; }
    public SectorZdoCountCell ZdoCountUpperLeft { get; private set; }
    public SectorZdoCountCell ZdoCountUpperCenter { get; private set; }
    public SectorZdoCountCell ZdoCountUpperRight { get; private set; }

    public GameObject ZdoCountCenterRow { get; private set; }
    public SectorZdoCountCell ZdoCountCenterLeft { get; private set; }
    public SectorZdoCountCell ZdoCountCenter { get; private set; }
    public SectorZdoCountCell ZdoCountCenterRight { get; private set; }

    public GameObject ZdoCountLowerRow { get; private set; }
    public SectorZdoCountCell ZdoCountLowerLeft { get; private set; }
    public SectorZdoCountCell ZdoCountLowerCenter { get; private set; }
    public SectorZdoCountCell ZdoCountLowerRight { get; private set; }

    static int FontSize { get => SectorInfoPanelFontSize.Value; }

    static IEnumerable<T> CreateEnumerable<T>(params T[] items) {
      return items ?? Enumerable.Empty<T>();
    }

    public SectorInfoPanel(Transform parentTransform) {
      Panel = CreatePanel(parentTransform);

      PositionContent = new(Panel.transform);

      PositionX = new(PositionContent.Row.transform);
      PositionX.FitValueToText("-00000");
      PositionX.Row.Image().SetColor(PositionValueXTextColor.Value.SetAlpha(0.1f));
      PositionX.Label.SetText("X");
      PositionX.Value.SetColor(PositionValueXTextColor.Value);

      PositionY = new(PositionContent.Row.transform);
      PositionY.FitValueToText("-00000");
      PositionY.Row.Image().SetColor(PositionValueYTextColor.Value.SetAlpha(0.1f));
      PositionY.Label.SetText("Y");
      PositionY.Value.SetColor(PositionValueYTextColor.Value);

      PositionZ = new(PositionContent.Row.transform);
      PositionZ.FitValueToText("-00000");
      PositionZ.Row.Image().SetColor(PositionValueZTextColor.Value.SetAlpha(0.1f));
      PositionZ.Label.SetText("Z");
      PositionZ.Value.SetColor(PositionValueZTextColor.Value);

      SectorContent = new(Panel.transform);

      SectorXY = new(SectorContent.Row.transform);
      SectorXY.Value.GetComponent<LayoutElement>().SetFlexible(width: 1f);
      SectorXY.Value.SetColor(PositionValueXTextColor.Value);
      SectorXY.Row.Image().SetColor(PositionValueXTextColor.Value.SetAlpha(0.1f));
      SectorXY.Label.SetText("Sector");
      SectorXY.Label.GetComponent<RectTransform>().SetAsFirstSibling();

      SectorZdoCount = new(SectorContent.Row.transform);
      SectorZdoCount.Value.GetComponent<LayoutElement>().SetFlexible(width: 1f);
      SectorZdoCount.Row.Image().SetColor(PositionValueYTextColor.Value.SetAlpha(0.1f));
      SectorZdoCount.Label.SetText("ZDOs");
      SectorZdoCount.Value.SetColor(PositionValueYTextColor.Value);

      ContentRow zdoCountContent = new(Panel.transform);
      GameObject zdoCountContentLabel = CreateLabel(zdoCountContent.Row.transform);
      zdoCountContentLabel.Text()
          .SetAlignment(TextAnchor.MiddleCenter)
          .SetFontSize(FontSize)
          .SetText("ZDOs per Sector");

      ZdoCountUpperRow = CreateZdoCountRow(Panel.transform);
      ZdoCountUpperLeft = new(ZdoCountUpperRow.transform);
      ZdoCountUpperCenter = new(ZdoCountUpperRow.transform);
      ZdoCountUpperRight = new(ZdoCountUpperRow.transform);

      ZdoCountCenterRow = CreateZdoCountRow(Panel.transform);
      ZdoCountCenterLeft = new(ZdoCountCenterRow.transform);
      ZdoCountCenter = new(ZdoCountCenterRow.transform);
      ZdoCountCenter.ZdoCount.SetColor(PositionValueZTextColor.Value);
      ZdoCountCenter.SectorBackground.SetColor(PositionValueZTextColor.Value.SetAlpha(0.2f));
      ZdoCountCenter.Sector.SetColor(Color.white);
      ZdoCountCenterRight = new(ZdoCountCenterRow.transform);

      ZdoCountLowerRow = CreateZdoCountRow(Panel.transform);
      ZdoCountLowerLeft = new(ZdoCountLowerRow.transform);
      ZdoCountLowerCenter = new(ZdoCountLowerRow.transform);
      ZdoCountLowerRight = new(ZdoCountLowerRow.transform);
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
          .SetHorizontalFit(ContentSizeFitter.FitMode.PreferredSize)
          .SetVerticalFit(ContentSizeFitter.FitMode.PreferredSize);

      panel.AddComponent<Image>()
          .SetType(Image.Type.Sliced)
          .SetSprite(CreateRoundedCornerSprite(400, 400, 15))
          .SetColor(SectorInfoPanelBackgroundColor.Value);

      panel.AddComponent<CanvasGroup>()
          .SetBlocksRaycasts(false);

      return panel;
    }

    public class ContentRow {
      public GameObject Row { get; private set; }

      public ContentRow(Transform parentTransform) {
        Row = CreateChildRow(parentTransform);
      }

      GameObject CreateChildRow(Transform parentTransform) {
        GameObject row = new("Row", typeof(RectTransform));
        row.SetParent(parentTransform);

        row.AddComponent<HorizontalLayoutGroup>()
            .SetChildControl(width: true, height: true)
            .SetChildForceExpand(width: true, height: false)
            .SetSpacing(6f)
            .SetChildAlignment(TextAnchor.MiddleCenter);

        return row;
      }
    }

    public class ValueWithLabel {
      public GameObject Row { get; private set; }
      public Text Value { get; private set; }
      public Text Label { get; private set; }

      public ValueWithLabel(Transform parentTransform) {
        Row = CreateChildRow(parentTransform);
        Value = CreateChildValue(Row.transform).Text();
        Label = CreateChildLabel(Row.transform).Text();
      }

      public ValueWithLabel FitValueToText(string longestText) {
        if (!Row.TryGetComponent(out ContentSizeFitter _)) {
            Row.AddComponent<ContentSizeFitter>()
              .SetHorizontalFit(ContentSizeFitter.FitMode.PreferredSize)
              .SetVerticalFit(ContentSizeFitter.FitMode.Unconstrained);
        }

        Value.GetComponent<LayoutElement>()
            .SetPreferred(width: GetTextPreferredWidth(Value, longestText));

        return this;
      }

      GameObject CreateChildRow(Transform parentTransform) {
        GameObject row = new("Row", typeof(RectTransform));
        row.SetParent(parentTransform);

        row.AddComponent<HorizontalLayoutGroup>()
            .SetChildControl(width: true, height: true)
            .SetChildForceExpand(width: false, height: false)
            .SetPadding(left: 8, right: 8, top: 4, bottom: 4)
            .SetSpacing(8f);

        row.AddComponent<Image>()
            .SetType(Image.Type.Sliced)
            .SetSprite(CreateRoundedCornerSprite(200, 200, 5))
            .SetColor(new(0f, 0f, 0f, 0.1f));

        return row;
      }

      GameObject CreateChildValue(Transform parentTransform) {
        GameObject value = CreateLabel(parentTransform);
        value.SetName("Value");

        value.Text()
            .SetFontSize(FontSize)
            .SetAlignment(TextAnchor.UpperRight)
            .SetText("0");

        value.AddComponent<LayoutElement>()
            .SetPreferred(width: 50f);

        return value;
      }

      GameObject CreateChildLabel(Transform parentTransform) {
        GameObject label = CreateLabel(parentTransform);
        label.SetName("Label");

        label.Text()
            .SetFontSize(FontSize)
            .SetAlignment(TextAnchor.UpperLeft)
            .SetText("X");

        return label;
      }
    }

    GameObject CreateZdoCountRow(Transform parentTransform) {
      GameObject row = new("ZdoCount.Row", typeof(RectTransform));
      row.SetParent(parentTransform);

      row.AddComponent<HorizontalLayoutGroup>()
          .SetChildControl(width: true, height: true)
          .SetChildForceExpand(width: false, height: false)
          .SetChildAlignment(TextAnchor.MiddleCenter)
          .SetSpacing(6f);

      return row;
    }

    public class SectorZdoCountCell {
      public GameObject Cell { get; private set; }

      public Image ZdoCountBackground { get; private set; }
      public Text ZdoCount { get; private set; }

      public Image SectorBackground { get; private set; }
      public Text Sector { get; private set; }

      public SectorZdoCountCell(Transform parentTransform) {
        Cell = CreateChildCell(parentTransform);

        ZdoCountBackground = CreateChildBackground(Cell.transform).Image();
        ZdoCountBackground.SetColor(Color.clear);
        ZdoCount = CreateChildLabel(ZdoCountBackground.transform).Text();

        SectorBackground = CreateChildBackground(Cell.transform).Image();
        SectorBackground.SetColor(new(0.5f, 0.5f, 0.5f, 0.5f));
        Sector = CreateChildLabel(SectorBackground.transform).Text();
        Sector.SetColor(new(0.9f, 0.9f, 0.9f, 1f));
      }

      GameObject CreateChildCell(Transform parentTransform) {
        GameObject cell = new("Cell", typeof(RectTransform));
        cell.SetParent(parentTransform);

        cell.AddComponent<VerticalLayoutGroup>()
            .SetChildControl(width: true, height: true)
            .SetChildForceExpand(width: false, height: false)
            .SetSpacing(0f)
            .SetChildAlignment(TextAnchor.MiddleCenter);

        cell.AddComponent<ContentSizeFitter>()
            .SetHorizontalFit(ContentSizeFitter.FitMode.Unconstrained)
            .SetVerticalFit(ContentSizeFitter.FitMode.PreferredSize);

        cell.AddComponent<Image>()
            .SetType(Image.Type.Sliced)
            .SetSprite(CreateRoundedCornerSprite(200, 200, 10))
            .SetColor(new(0f, 0f, 0f, 0.3f));

        return cell;
      }

      GameObject CreateChildBackground(Transform parentTransform) {
        GameObject background = new("Background", typeof(RectTransform));
        background.SetParent(parentTransform);

        background.AddComponent<HorizontalLayoutGroup>()
            .SetChildControl(width: true, height: true)
            .SetChildForceExpand(width: false, height: false)
            .SetPadding(left: 4, right: 4, top: 2, bottom: 2)
            .SetChildAlignment(TextAnchor.MiddleCenter);

        background.AddComponent<Image>()
            .SetType(Image.Type.Sliced)
            .SetSprite(CreateRoundedCornerSprite(200, 200, 10))
            .SetColor(Color.clear);

        return background;
      }

      GameObject CreateChildLabel(Transform parentTransform) {
        GameObject label = CreateLabel(parentTransform);
        label.SetName("Label");

        label.Text()
            .SetFontSize(FontSize)
            .SetAlignment(TextAnchor.MiddleCenter)
            .SetText("123");

        label.AddComponent<LayoutElement>()
            .SetFlexible(width: 1f);

        return label;
      }
    }
  }
}
