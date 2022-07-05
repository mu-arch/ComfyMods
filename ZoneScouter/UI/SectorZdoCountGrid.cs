using UnityEngine;
using UnityEngine.UI;

using static ZoneScouter.PluginConfig;
using static ZoneScouter.UIBuilder;

namespace ZoneScouter {
  public class SectorZdoCountGrid {
    public GameObject Grid { get; private set; }

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

    public SectorZdoCountGrid(Transform parentTransform) {
      Grid = CreateChildGrid(parentTransform);

      ZdoCountUpperRow = CreateSectorZdoCountGridRow(Grid.transform);
      ZdoCountUpperLeft = new(ZdoCountUpperRow.transform);
      ZdoCountUpperCenter = new(ZdoCountUpperRow.transform);
      ZdoCountUpperRight = new(ZdoCountUpperRow.transform);

      ZdoCountCenterRow = CreateSectorZdoCountGridRow(Grid.transform);
      ZdoCountCenterLeft = new(ZdoCountCenterRow.transform);
      ZdoCountCenter = new(ZdoCountCenterRow.transform);
      ZdoCountCenter.ZdoCount.SetColor(PositionValueZTextColor.Value);
      ZdoCountCenter.SectorBackground.SetColor(PositionValueZTextColor.Value.SetAlpha(0.2f));
      ZdoCountCenter.Sector.SetColor(Color.white);
      ZdoCountCenterRight = new(ZdoCountCenterRow.transform);

      ZdoCountLowerRow = CreateSectorZdoCountGridRow(Grid.transform);
      ZdoCountLowerLeft = new(ZdoCountLowerRow.transform);
      ZdoCountLowerCenter = new(ZdoCountLowerRow.transform);
      ZdoCountLowerRight = new(ZdoCountLowerRow.transform);
    }

    GameObject CreateChildGrid(Transform parentTransform) {
      GameObject grid = new("Grid", typeof(RectTransform));
      grid.SetParent(parentTransform);

      grid.AddComponent<VerticalLayoutGroup>()
          .SetChildControl(width: true, height: true)
          .SetChildForceExpand(width: false, height: false)
          .SetChildAlignment(TextAnchor.MiddleCenter)
          .SetSpacing(6f);

      CreateLabel(grid.transform).Text()
          .SetAlignment(TextAnchor.MiddleCenter)
          .SetFontSize(SectorInfoPanelFontSize.Value)
          .SetText("ZDOs per Sector");

      return grid;
    }

    GameObject CreateSectorZdoCountGridRow(Transform parentTransform) {
      GameObject row = new("SectorZdoCountGrid.Row", typeof(RectTransform));
      row.SetParent(parentTransform);

      row.AddComponent<HorizontalLayoutGroup>()
          .SetChildControl(width: true, height: true)
          .SetChildForceExpand(width: false, height: false)
          .SetChildAlignment(TextAnchor.MiddleCenter)
          .SetSpacing(6f);

      return row;
    }
  }
}
