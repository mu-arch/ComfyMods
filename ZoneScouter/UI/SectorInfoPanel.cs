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
    public ValueWithLabel SectorX { get; private set; }
    public ValueWithLabel SectorY { get; private set; }

    public GameObject ZdoCountUpperRow { get; private set; }
    public Text ZdoCountUpperLeft { get; private set; }
    public Text ZdoCountUpperCenter { get; private set; }
    public Text ZdoCountUpperRight { get; private set; }

    public GameObject ZdoCountCenterRow { get; private set; }
    public Text ZdoCountCenterLeft { get; private set; }
    public Text ZdoCountCenter { get; private set; }
    public Text ZdoCountCenterRight { get; private set; }

    public GameObject ZdoCountLowerRow { get; private set; }
    public Text ZdoCountLowerLeft { get; private set; }
    public Text ZdoCountLowerCenter { get; private set; }
    public Text ZdoCountLowerRight { get; private set; }

    static int FontSize { get => SectorInfoPanelFontSize.Value; }

    static IEnumerable<T> CreateEnumerable<T>(params T[] items) {
      return items ?? Enumerable.Empty<T>();
    }

    public SectorInfoPanel(Transform parentTransform) {
      Panel = CreatePanel(parentTransform);

      PositionContent = new(Panel.transform);
      PositionContent.Label.SetText("Position");

      PositionX = new(PositionContent.Row.transform);
      PositionX.SetValueTextWidth("-00000");
      PositionX.Row.Image().SetColor(PositionValueXTextColor.Value.SetAlpha(0.1f));
      PositionX.Label.SetText("X");
      PositionX.Value.SetColor(PositionValueXTextColor.Value);

      PositionY = new(PositionContent.Row.transform);
      PositionY.SetValueTextWidth("-00000");
      PositionY.Row.Image().SetColor(PositionValueYTextColor.Value.SetAlpha(0.1f));
      PositionY.Label.SetText("Y");
      PositionY.Value.SetColor(PositionValueYTextColor.Value);

      PositionZ = new(PositionContent.Row.transform);
      PositionZ.SetValueTextWidth("-00000");
      PositionZ.Row.Image().SetColor(PositionValueZTextColor.Value.SetAlpha(0.1f));
      PositionZ.Label.SetText("Z");
      PositionZ.Value.SetColor(PositionValueZTextColor.Value);

      SectorContent = new(Panel.transform);
      SectorContent.Label.SetText("Sector");

      SectorX = new(SectorContent.Row.transform);
      SectorX.Value.gameObject.LayoutElement().SetFlexible(width: 1f);
      SectorX.Row.Image().SetColor(PositionValueXTextColor.Value.SetAlpha(0.1f));
      SectorX.Label.SetText("X");
      SectorX.Value.SetColor(PositionValueXTextColor.Value);

      SectorY = new(SectorContent.Row.transform);
      SectorY.Value.gameObject.LayoutElement().SetFlexible(width: 1f);
      SectorY.Row.Image().SetColor(PositionValueYTextColor.Value.SetAlpha(0.1f));
      SectorY.Label.SetText("Y");
      SectorY.Value.SetColor(PositionValueYTextColor.Value);



      float longestWidth =
          Mathf.Max(GetTextPreferredWidth(PositionContent.Label), GetTextPreferredWidth(SectorContent.Label));

      PositionContent.Label.gameObject.LayoutElement().SetPreferred(width: longestWidth);
      SectorContent.Label.gameObject.LayoutElement().SetPreferred(width: longestWidth);

      ZdoCountUpperRow = CreateZdoCountRow(Panel.transform);
      ZdoCountUpperLeft = CreateZdoCountValue(CreateZdoCountCell(ZdoCountUpperRow.transform).transform).Text();
      ZdoCountUpperCenter = CreateZdoCountValue(CreateZdoCountCell(ZdoCountUpperRow.transform).transform).Text();
      ZdoCountUpperRight = CreateZdoCountValue(CreateZdoCountCell(ZdoCountUpperRow.transform).transform).Text();

      ZdoCountCenterRow = CreateZdoCountRow(Panel.transform);
      ZdoCountCenterLeft = CreateZdoCountValue(CreateZdoCountCell(ZdoCountCenterRow.transform).transform).Text();
      ZdoCountCenter = CreateZdoCountValue(CreateZdoCountCell(ZdoCountCenterRow.transform).transform).Text();
      ZdoCountCenter.SetColor(Color.yellow);
      ZdoCountCenterRight = CreateZdoCountValue(CreateZdoCountCell(ZdoCountCenterRow.transform).transform).Text();

      ZdoCountLowerRow = CreateZdoCountRow(Panel.transform);
      ZdoCountLowerLeft = CreateZdoCountValue(CreateZdoCountCell(ZdoCountLowerRow.transform).transform).Text();
      ZdoCountLowerCenter = CreateZdoCountValue(CreateZdoCountCell(ZdoCountLowerRow.transform).transform).Text();
      ZdoCountLowerRight = CreateZdoCountValue(CreateZdoCountCell(ZdoCountLowerRow.transform).transform).Text();
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
      public Text Label { get; private set; }

      public ContentRow(Transform parentTransform) {
        Row = CreateChildRow(parentTransform);
        Label = CreateChildLabel(Row.transform).Text();
      }

      GameObject CreateChildRow(Transform parentTransform) {
        GameObject row = new("Row", typeof(RectTransform));
        row.SetParent(parentTransform);

        row.AddComponent<HorizontalLayoutGroup>()
            .SetChildControl(width: true, height: true)
            .SetChildForceExpand(width: false, height: false)
            .SetSpacing(6f)
            .SetChildAlignment(TextAnchor.MiddleCenter);

        return row;
      }

      GameObject CreateChildLabel(Transform parentTransform) {
        GameObject label = CreateLabel(parentTransform);
        label.SetName("Label");

        label.Text()
            .SetFontSize(FontSize)
            .SetAlignment(TextAnchor.MiddleRight)
            .SetText("Property");

        label.AddComponent<LayoutElement>();

        return label;
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

      public ValueWithLabel SetValueTextWidth(string longestText) {
        Value.gameObject.LayoutElement().SetPreferred(width: GetTextPreferredWidth(Value, longestText));
        return this;
      }

      GameObject CreateChildRow(Transform parentTransform) {
        GameObject row = new("Row", typeof(RectTransform));
        row.SetParent(parentTransform);

        row.AddComponent<HorizontalLayoutGroup>()
            .SetChildControl(width: true, height: true)
            .SetChildForceExpand(width: false, height: false)
            .SetPadding(left: 2, right: 8, top: 2, bottom: 2)
            .SetSpacing(8f);

        row.AddComponent<ContentSizeFitter>()
            .SetHorizontalFit(ContentSizeFitter.FitMode.PreferredSize)
            .SetVerticalFit(ContentSizeFitter.FitMode.Unconstrained);

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
          .SetSpacing(8f);

      return row;
    }

    GameObject CreateZdoCountCell(Transform parentTransform) {
      GameObject cell = new("ZdoCount.Cell", typeof(RectTransform));
      cell.SetParent(parentTransform);

      cell.RectTransform()
          .SetAnchorMin(Vector2.zero)
          .SetAnchorMax(Vector2.zero)
          .SetPivot(Vector2.zero);

      cell.AddComponent<LayoutElement>()
          .SetPreferred(width: 80f, height: 25f)
          .SetFlexible(width: 1f);

      cell.AddComponent<Image>()
          .SetType(Image.Type.Sliced)
          .SetSprite(CreateRoundedCornerSprite(200, 200, 10))
          .SetColor(new(0f, 0f, 0f, 0.9f));

      return cell;
    }

    GameObject CreateZdoCountValue(Transform parentTransform) {
      GameObject value = CreateLabel(parentTransform);
      value.SetName("ZdoCount.Value");

      value.Text()
          .SetFontSize(SectorInfoPanelFontSize.Value)
          .SetAlignment(TextAnchor.MiddleCenter)
          .SetText("0");

      return value;
    }
  }
}
