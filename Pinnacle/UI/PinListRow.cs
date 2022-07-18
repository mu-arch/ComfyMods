using System;

using UnityEngine;
using UnityEngine.UI;

namespace Pinnacle {
  public class PinListRow {
    public GameObject Row { get; private set; }

    public Image PinIcon { get; private set; }
    public Text PinName { get; private set; }

    public PinListRow(Transform parentTransform) {
      Row = CreateChildRow(parentTransform);
      PinIcon = CreateChildPinIcon(Row.transform).Image();
      PinName = CreateChildPinName(Row.transform).Text();

      UIBuilder.CreateRowSpacer(Row.transform);
    }

    public PinListRow SetRowContent(Minimap.PinData pin) {
      PinIcon.SetSprite(pin.m_icon);
      PinName.SetText(pin.m_name.Length == 0 ? pin.m_type.ToString() : pin.m_name);
      return this;
    }

    GameObject CreateChildRow(Transform parentTransform) {
      GameObject row = new("PinList.Row", typeof(RectTransform));
      row.SetParent(parentTransform);

      row.AddComponent<HorizontalLayoutGroup>()
          .SetChildControl(width: true, height: true)
          .SetChildForceExpand(width: false, height: false)
          .SetChildAlignment(TextAnchor.MiddleCenter)
          .SetPadding(left: 8, right: 8, top: 5, bottom: 5)
          .SetSpacing(5f);

      row.AddComponent<Image>()
          .SetType(Image.Type.Sliced)
          .SetSprite(UIBuilder.CreateRoundedCornerSprite(400, 400, 5));

      row.AddComponent<Button>()
          .SetNavigationMode(Navigation.Mode.None)
          .SetTargetGraphic(row.Image())
          .SetColors(ButtonColorBlock.Value);

      row.AddComponent<ContentSizeFitter>()
          .SetHorizontalFit(ContentSizeFitter.FitMode.Unconstrained)
          .SetVerticalFit(ContentSizeFitter.FitMode.PreferredSize);

      row.AddComponent<ParentSizeFitter>();

      return row;
    }

    GameObject CreateChildPinIcon(Transform parentTransform) {
      GameObject icon = new("Pin.Icon", typeof(RectTransform));
      icon.SetParent(parentTransform);

      icon.AddComponent<Image>()
          .SetType(Image.Type.Simple);

      icon.AddComponent<LayoutElement>()
          .SetPreferred(width: 20f, height: 20f);

      return icon;
    }

    GameObject CreateChildPinName(Transform parentTransform) {
      GameObject name = UIBuilder.CreateLabel(parentTransform);
      name.SetName("Pin.Name");

      return name;
    }

    static readonly Lazy<ColorBlock> ButtonColorBlock =
        new(() =>
          new() {
            normalColor = new Color(0f, 0f, 0f, 0.01f),
            highlightedColor = new Color32(50, 161, 217, 128),
            disabledColor = new Color(0f, 0f, 0f, 0.1f),
            pressedColor = new Color32(50, 161, 217, 192),
            selectedColor = new Color32(50, 161, 217, 248),
            colorMultiplier = 1f,
            fadeDuration = 0.15f,
          });
  }
}
