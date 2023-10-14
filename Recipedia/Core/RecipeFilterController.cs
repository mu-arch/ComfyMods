using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

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

    static readonly Dictionary<string, RectTransform> _recipeElementByName = new();

    static void CacheRecipeElements(InventoryGui inventoryGui) {
      _recipeElementByName.Clear();

      foreach (GameObject element in inventoryGui.m_recipeList) {
        string recipeName = element.transform.Find("name").GetComponent<TMP_Text>().text;
        _recipeElementByName[recipeName] = element.GetComponent<RectTransform>();
      }
    }

    static void OnRecipeFilterChanged(string value) {
      InventoryGui inventoryGui = InventoryGui.instance;
      int count = 0;
      float spacing = inventoryGui.m_recipeListSpace;

      if (value.Length <= 0) {
        foreach (RectTransform element in _recipeElementByName.Values) {
          element.gameObject.SetActive(true);
          element.anchoredPosition = new(0f, count * -spacing);
          count++;
        }
      } else {
        foreach (KeyValuePair<string, RectTransform> pair in _recipeElementByName) {
          bool isMatching = pair.Key.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0;
          pair.Value.gameObject.SetActive(isMatching);

          if (isMatching) {
            pair.Value.anchoredPosition = new(0f, count * -spacing);
            count++;
          }
        }
      }

      float height = Mathf.Max(inventoryGui.m_recipeListBaseSize, count * spacing);
      inventoryGui.m_recipeListRoot.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
      inventoryGui.m_recipeEnsureVisible.mScrollRect.normalizedPosition = Vector2.up;
    }

    public static bool IsFocused() {
      return RecipeFilter && RecipeFilter.InputField && RecipeFilter.InputField.isFocused;
    }
  }
}
