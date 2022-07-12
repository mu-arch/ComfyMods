using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.UI;

namespace Pinnacle {
  public class PinIconSelector {
    public GameObject Grid { get; private set; }
    public List<GameObject> Icons { get; } = new();

    Minimap.PinData _targetPin;

    public PinIconSelector(Transform parentTransform) {
      Grid = CreateChildGrid(parentTransform);

      foreach (Minimap.PinType pinType in Enum.GetValues(typeof(Minimap.PinType))) {
        Sprite sprite = Minimap.m_instance.GetSprite(pinType);

        if (sprite == null) {
          continue;
        }

        GameObject icon = CreateChildIcon(Grid.transform);
        Icons.Add(icon);

        icon.Image()
            .SetSprite(sprite);

        icon.AddComponent<Button>()
            .SetNavigationMode(Navigation.Mode.None)
            .SetTargetGraphic(icon.Image())
            .SetTransition(Selectable.Transition.ColorTint)
            .SetColors(ButtonColorBlock.Value);

        icon.Button().onClick.AddListener(() => OnIconClicked(pinType));
      }
    }

    public void SetTargetPin(Minimap.PinData pin) {
      _targetPin = pin;
      UpdateIcons(_targetPin?.m_icon.name);
    }

    void OnIconClicked(Minimap.PinType pinType) {
      if (_targetPin != null) {
        ZLog.Log($"Setting Pin.m_type from: {_targetPin.m_type}, to: {pinType}");

        _targetPin.m_type = pinType;
        _targetPin.m_icon = Minimap.m_instance.GetSprite(pinType);
        _targetPin.m_iconElement.SetSprite(_targetPin.m_icon);

        UpdateIcons(_targetPin.m_icon.name);
      }
    }

    void UpdateIcons(string targetSpriteName) {
      foreach (Image icon in Icons.Select(i => i.Image())) {
        icon.SetColor(
            icon.sprite.name == targetSpriteName
                ? ButtonColorBlock.Value.selectedColor
                : ButtonColorBlock.Value.normalColor);
      }
    }

    GameObject CreateChildGrid(Transform parentTransform) {
      GameObject grid = new("PinIconSelector.Grid", typeof(RectTransform));
      grid.SetParent(parentTransform);

      grid.AddComponent<GridLayoutGroup>()
          .SetCellSize(new(25f, 25f))
          .SetSpacing(new(8f, 8f))
          .SetConstraint(GridLayoutGroup.Constraint.FixedRowCount)
          .SetConstraintCount(2)
          .SetStartAxis(GridLayoutGroup.Axis.Horizontal)
          .SetStartCorner(GridLayoutGroup.Corner.UpperLeft);

      grid.AddComponent<LayoutElement>()
          .SetFlexible(width: 1f);

      return grid;
    }

    GameObject CreateChildIcon(Transform parentTransform) {
      GameObject icon = new("Icon", typeof(RectTransform));
      icon.SetParent(parentTransform);

      icon.AddComponent<Image>()
          .SetType(Image.Type.Simple)
          .SetPreserveAspect(true);

      icon.AddComponent<LayoutElement>()
          .SetPreferred(width: 25f, height: 25f);

      return icon;
    }

    static readonly Lazy<ColorBlock> ButtonColorBlock =
        new(() =>
          new() {
            normalColor = new Color(1f, 1f, 1f, 0.8f),
            highlightedColor = new Color(0.565f, 0.792f, 0.976f),
            disabledColor = new Color(0f, 0f, 0f, 0.5f),
            pressedColor = new Color(0.647f, 0.839f, 0.655f),
            selectedColor = new Color(1f, 0.878f, 0.51f),
            colorMultiplier = 1f,
            fadeDuration = 0.25f,
          });
  }
}
