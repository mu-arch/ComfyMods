using ComfyLib;

using HarmonyLib;

using UnityEngine;
using UnityEngine.UI;

using static Inventorious.PluginConfig;

namespace Inventorious {
  [HarmonyPatch(typeof(InventoryGui))]
  static class InventoryGuiPatch {
    static CanvasGroup _animatorCanvasGroup;

    [HarmonyPostfix]
    [HarmonyPatch(nameof(InventoryGui.Awake))]
    static void AwakePostfix(ref InventoryGui __instance) {
      __instance.m_animator.enabled = false;
      
      if (!__instance.m_crafting.TryGetComponent(out PanelDragger craftingPanelDragger)) {
        craftingPanelDragger = __instance.m_crafting.gameObject.AddComponent<PanelDragger>();
      }

      craftingPanelDragger.TargetRectTransform = __instance.m_crafting;

      if (!__instance.m_animator.TryGetComponent(out _animatorCanvasGroup)) {
        _animatorCanvasGroup = __instance.m_animator.gameObject.AddComponent<CanvasGroup>();
      }

      _animatorCanvasGroup.alpha = 0f;
    }

    static Coroutine _showOrHideCoroutine;

    [HarmonyPostfix]
    [HarmonyPatch(nameof(InventoryGui.Show))]
    static void ShowPostfix(ref InventoryGui __instance) {
      if (_showOrHideCoroutine != null) {
        __instance.StopCoroutine(_showOrHideCoroutine);
      }

      _showOrHideCoroutine =
          __instance.StartCoroutine(
              CorouTweens.Lerp(
                  _animatorCanvasGroup.alpha, 1f, ShowTransitionDuration.Value, x => _animatorCanvasGroup.alpha = x));
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(InventoryGui.Hide))]
    static void HidePrefix(ref InventoryGui __instance, ref bool __state) {
      __state = __instance.m_animator.GetBool("visible");
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(InventoryGui.Hide))]
    static void HidePostfix(ref InventoryGui __instance, ref bool __state) {
      if (!__state || __instance.m_animator.GetBool("visible")) {
        return;
      }

      if (_showOrHideCoroutine != null) {
        __instance.StopCoroutine(_showOrHideCoroutine);
      }

      _showOrHideCoroutine =
          __instance.StartCoroutine(
              CorouTweens.Lerp(
                  _animatorCanvasGroup.alpha, 0f, HideTransitionDuration.Value, x => _animatorCanvasGroup.alpha = x));
    }

    static InventoryGrid.Element _selectedGridElement;
    static RectTransform _highlightBorder;

    [HarmonyPostfix]
    [HarmonyPatch(nameof(InventoryGui.SetRecipe))]
    static void SetRecipePostfix(ref InventoryGui __instance, int index) {
      if (_highlightBorder) {
        _highlightBorder.gameObject.SetActive(false);
      }

      ItemDrop.ItemData selectedItemData = __instance.m_selectedRecipe.Value;

      if (selectedItemData == null) {
        ZLog.Log($"No selectedItemData at index: {index}");
        return;
      }

      int gridIndex = (selectedItemData.m_gridPos.y * __instance.m_playerGrid.m_width) + selectedItemData.m_gridPos.x;

      if (gridIndex < 0 || gridIndex >= __instance.m_playerGrid.m_elements.Count) {
        ZLog.Log($"gridIndex out of bounds for itemData.gridPos: {selectedItemData.m_gridPos} (index: {index})");
        return;
      }

      if (!_highlightBorder) {
        ZLog.Log($"Creating new HighlightBorder gameObject.");
        _highlightBorder = CreateHighlightBorder();
      }

      _selectedGridElement = __instance.m_playerGrid.m_elements[gridIndex];

      _highlightBorder.SetParent(_selectedGridElement.m_icon.transform, worldPositionStays: false);
      _highlightBorder.gameObject.SetActive(true);

      ZLog.Log($"Selected: {_selectedGridElement.m_pos}");
    }

    static RectTransform CreateHighlightBorder() {
      GameObject highlightBorder = new("Highlight.Border");

      RectTransform rectTransform = highlightBorder.AddComponent<RectTransform>();
      rectTransform.anchorMin = Vector2.zero;
      rectTransform.anchorMax = Vector2.one;
      rectTransform.pivot = new(0.5f, 0f);
      rectTransform.sizeDelta = Vector2.zero;

      Image image = highlightBorder.AddComponent<Image>();
      image.color = new(1f, 1f, 0f, 0.5f);

      return rectTransform;
    }
  }
}
