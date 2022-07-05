using UnityEngine;
using UnityEngine.UI;

using static ZoneScouter.PluginConfig;
using static ZoneScouter.UIBuilder;

namespace ZoneScouter {
  public class SectorZdoCountGrid {
    public GameObject Grid { get; private set; }

    public GameObject[] Rows { get; private set; }
    public SectorZdoCountCell[,] Cells { get; private set; }

    // TODO: not this.
    static int GridSize => SectorZdoCountGridSize.Value == PluginConfig.GridSize.ThreeByThree ? 3 : 5;

    public SectorZdoCountGrid(Transform parentTransform) {
      Grid = CreateChildGrid(parentTransform);

      Rows = new GameObject[GridSize];
      Cells = new SectorZdoCountCell[GridSize, GridSize];

      for (int i = 0; i < GridSize; i++) {
        GameObject row = CreateSectorZdoCountGridRow(Grid.transform);

        for (int j = 0; j < GridSize; j++) {
          Cells[i, j] = new(row.transform);
        }
      }
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
