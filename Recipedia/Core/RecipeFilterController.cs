using System.Collections;
using System.Collections.Generic;

using ComfyLib;

using TMPro;

using UnityEngine;

namespace Recipedia {
  public class RecipeFilterController {
    public static RecipeFilter RecipeFilter { get; private set; }

    public static void SetupRecipeFilter(InventoryGui inventoryGui) {
      if (!RecipeFilter) {
        AddRecipeFilter(inventoryGui);
        inventoryGui.StartCoroutine(FinishSetup());
      }

      CacheRecipeElements(inventoryGui);
    }

    static IEnumerator FinishSetup() {
      yield return null;
      if (RecipeFilter && RecipeFilter.InputField) {
        RecipeFilter.InputField.onValueChanged.AddListener(value => OnRecipeFilterChanged(value));
      } else {
        ZLog.LogError($"No filter?");
      }

    }

    static void AddRecipeFilter(InventoryGui inventoryGui) {
      inventoryGui.m_recipeEnsureVisible.GetComponent<RectTransform>()
          .SetSizeDelta(new(187f, -45f));

      inventoryGui.m_recipeListScroll.GetComponent<RectTransform>()
          .SetAnchorMin(Vector2.right)
          .SetAnchorMax(Vector2.one)
          .SetPivot(Vector2.zero)
          .SetAnchoredPosition(new(-15f, -2.5f))
          .SetSizeDelta(new(12.5f, -40f));

      GameObject filterObj = new("RecipeFilter", typeof(RectTransform));
      filterObj.transform.SetParent(inventoryGui.m_crafting.Find("RecipeList"));

      RecipeFilter = filterObj.AddComponent<RecipeFilter>();
    }

    static readonly Dictionary<string, GameObject> _recipeElementByName = new();

    static void CacheRecipeElements(InventoryGui inventoryGui) {
      _recipeElementByName.Clear();

      foreach (GameObject element in inventoryGui.m_recipeList) {
        string recipeName = element.transform.Find("name").GetComponent<TMP_Text>().text;
        _recipeElementByName[recipeName] = element;
      }
    }

    static void OnRecipeFilterChanged(string value) {
      if (string.IsNullOrEmpty(value)) {
        foreach (GameObject element in _recipeElementByName.Values) {
          element.SetActive(true);
        }
      } else {
        foreach (KeyValuePair<string, GameObject> pair in _recipeElementByName) {
          pair.Value.SetActive(pair.Key.IndexOf(value, System.StringComparison.OrdinalIgnoreCase) >= 0);
        }
      }
    }

    public static bool IsFocused() {
      return RecipeFilter && RecipeFilter.InputField && RecipeFilter.InputField.isFocused;
    }
  }
}
