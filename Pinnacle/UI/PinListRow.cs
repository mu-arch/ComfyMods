using System;

using UnityEngine;
using UnityEngine.UI;

namespace Pinnacle {
  public class PinListRow {
    public GameObject Row { get; private set; }

    public Image PinIcon { get; private set; }
    public Text PinName { get; private set; }

    public Text PositionX { get; private set; }
    public Text PositionY { get; private set; }
    public Text PositionZ { get; private set; }

    Vector3 _pinPositionCache = Vector3.zero;

    public PinListRow(Transform parentTransform) {
      Row = CreateChildRow(parentTransform);
      Row.Button().onClick.AddListener(() => Pinnacle.CenterMapOnOrTeleportTo(_pinPositionCache));

      PinIcon = CreateChildPinIcon(Row.transform).Image();
      PinName = CreateChildPinName(Row.transform).Text();

      UIBuilder.CreateRowSpacer(Row.transform);

      PositionX = CreateChildPinPositionValue(Row.transform).Text();
      PositionX.SetColor(new(1f, 0.878f, 0.51f));
      PositionY = CreateChildPinPositionValue(Row.transform).Text();
      PositionY.SetColor(new(0.565f, 0.792f, 0.976f));
      PositionZ = CreateChildPinPositionValue(Row.transform).Text();
      PositionZ.SetColor(new(0.647f, 0.839f, 0.655f));
    }

    public PinListRow SetRowContent(Minimap.PinData pin) {
      _pinPositionCache = pin.m_pos;

      PinIcon.SetSprite(pin.m_icon);
      PinName.SetText(pin.m_name.Length == 0 ? pin.m_type.ToString() : pin.m_name);

      PositionX.SetText($"{pin.m_pos.x:F0}");
      PositionY.SetText($"{pin.m_pos.y:F0}");
      PositionZ.SetText($"{pin.m_pos.z:F0}");

      return this;
    }

    GameObject CreateChildRow(Transform parentTransform) {
      GameObject row = new("PinList.Row", typeof(RectTransform));
      row.SetParent(parentTransform);

      row.AddComponent<HorizontalLayoutGroup>()
          .SetChildControl(width: true, height: true)
          .SetChildForceExpand(width: false, height: false)
          .SetChildAlignment(TextAnchor.MiddleCenter)
          .SetPadding(left: 5, right: 10, top: 5, bottom: 5)
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

      icon.AddComponent<LayoutElement>()
          .SetPreferred(width: 20f, height: 20f);

      icon.AddComponent<Image>()
          .SetType(Image.Type.Simple);

      return icon;
    }

    GameObject CreateChildPinName(Transform parentTransform) {
      GameObject name = UIBuilder.CreateLabel(parentTransform);
      name.SetName("Pin.Name");

      return name;
    }

    GameObject CreateChildPinPositionValue(Transform parentTransform) {
      GameObject value = UIBuilder.CreateLabel(parentTransform);
      value.SetName("Pin.Position.Value");

      value.Text()
          .SetAlignment(TextAnchor.MiddleRight)
          .SetText("-12345");

      value.AddComponent<LayoutElement>()
          .SetPreferred(width: value.Text().GetPreferredWidth());

      return value;
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
            fadeDuration = 0f,
          });
  }
}
