using UnityEngine;
using UnityEngine.UI;

using static ZoneScouter.PluginConfig;

namespace ZoneScouter {
  public class SectorZdoCountGrid {
    public GameObject Grid { get; private set; }

    public int Size { get; private set; }
    public GameObject[] Rows { get; private set; }
    public SectorZdoCountCell[,] Cells { get; private set; }

    public SectorZdoCountGrid(Transform parentTransform, GridSize gridSize) {
      Size = GetSize(gridSize);
      Grid = CreateChildGrid(parentTransform);

      Rows = new GameObject[Size];
      Cells = new SectorZdoCountCell[Size, Size];

      for (int i = 0; i < Size; i++) {
        GameObject row = CreateSectorZdoCountGridRow(Grid.transform);

        for (int j = 0; j < Size; j++) {
          Cells[i, j] = new(row.transform);
        }
      }
    }

    static int GetSize(GridSize gridSize) {
      return gridSize switch {
        GridSize.ThreeByThree => 3,
        GridSize.FiveByFive => 5,
        _ => 1
      };
    }

    GameObject CreateChildGrid(Transform parentTransform) {
      GameObject grid = new("Grid", typeof(RectTransform));
      grid.SetParent(parentTransform);

      grid.AddComponent<VerticalLayoutGroup>()
          .SetChildControl(width: true, height: true)
          .SetChildForceExpand(width: false, height: false)
          .SetChildAlignment(TextAnchor.MiddleCenter)
          .SetSpacing(6f);

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
