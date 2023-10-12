using ComfyLib;

using UnityEngine;

namespace Recipedia {
  public class RecipeFilterController {
    public static RecipeFilter RecipeFilter { get; private set; }

    public static void AddRecipeFilter(InventoryGui inventoryGui) {
      if (!RecipeFilter) {
        Transform recipeListTransform = inventoryGui.m_crafting.Find("RecipeList");

        GameObject filter = new("RecipeFilter", typeof(RectTransform));
        filter.transform.SetParent(recipeListTransform);
        RecipeFilter = filter.AddComponent<RecipeFilter>();

        inventoryGui.m_recipeEnsureVisible.GetComponent<RectTransform>()
            .SetSizeDelta(new(187f, -45f));

        inventoryGui.m_recipeListScroll.GetComponent<RectTransform>()
            .SetAnchorMin(Vector2.right)
            .SetAnchorMax(Vector2.one)
            .SetPivot(Vector2.zero)
            .SetAnchoredPosition(new(-15f, -2.5f))
            .SetSizeDelta(new(12.5f, -40f));
      }
    }
  }
}
