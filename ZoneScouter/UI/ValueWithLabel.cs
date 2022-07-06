using UnityEngine;
using UnityEngine.UI;

using static ZoneScouter.PluginConfig;
using static ZoneScouter.UIBuilder;

namespace ZoneScouter {
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
      Value.GetOrAddComponent<LayoutElement>()
          .SetFlexible(width: 1f)
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
          .SetFontSize(SectorInfoPanelFontSize.Value)
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
          .SetFontSize(SectorInfoPanelFontSize.Value)
          .SetAlignment(TextAnchor.UpperLeft)
          .SetText("X");

      return label;
    }
  }
}
