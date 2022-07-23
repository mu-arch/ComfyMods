using System.Linq;

using UnityEngine;
using UnityEngine.UI;

namespace Pinnacle {
  public class PinFilterPanel {
    public GameObject Panel { get; private set; }
    public PinIconSelector PinIconSelector { get; private set; }

    public PinFilterPanel(Transform parentTransform) {
      Panel = CreateChildPanel(parentTransform);

      PinIconSelector = new(Panel.transform);

      PinIconSelector.Grid.GetComponent<GridLayoutGroup>()
          .SetConstraint(GridLayoutGroup.Constraint.FixedColumnCount)
          .SetConstraintCount(2)
          .SetStartAxis(GridLayoutGroup.Axis.Vertical)
          .SetCellSize(new(40f, 40f));

      PinIconSelector.OnPinIconClicked += (_, pinType) => Minimap.m_instance.ToggleIconFilter(pinType);
    }

    public void UpdatePinIconFilters() {
      foreach (Minimap.PinType pinType in PinIconSelector.IconsByType.Keys) {
        PinIconSelector.IconsByType[pinType]
            .Image()
            .Ref()?
            .SetColor(Minimap.m_instance.m_visibleIconTypes[(int) pinType] ? Color.white : Color.gray);
      }
    }

    GameObject CreateChildPanel(Transform parentTransform) {
      GameObject panel = new("PinFilter.Panel", typeof(RectTransform));
      panel.SetParent(parentTransform);

      panel.AddComponent<VerticalLayoutGroup>()
          .SetChildControl(width: true, height: true)
          .SetChildForceExpand(width: false, height: false)
          .SetPadding(left: 8, right: 8, top: 8, bottom: 8)
          .SetSpacing(4f);

      panel.AddComponent<ContentSizeFitter>()
          .SetHorizontalFit(ContentSizeFitter.FitMode.PreferredSize)
          .SetVerticalFit(ContentSizeFitter.FitMode.PreferredSize);

      panel.AddComponent<Image>()
          .SetType(Image.Type.Sliced)
          .SetSprite(UIBuilder.CreateRoundedCornerSprite(128, 128, 16))
          .SetColor(new(0f, 0f, 0f, 0.9f));

      panel.AddComponent<CanvasGroup>()
          .SetBlocksRaycasts(true);

      return panel;
    }
  }
}
