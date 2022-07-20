using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Pinnacle {
  public class PinListPanel {
    public GameObject Panel { get; private set; }
    public GameObject Viewport { get; private set; }
    public GameObject Content { get; private set; }
    public ScrollRect ScrollRect { get; private set; }

    public ValueCell PinNameFilter { get; private set; }
    public PinIconSelector PinTypeSelector { get; private set; }
    public LabelCell PinStats { get; private set; }

    readonly PointerState _pointerState;

    public PinListPanel(Transform parentTransform) {
      Panel = CreateChildPanel(parentTransform);

      PinNameFilter = new(Panel.transform);
      PinNameFilter.Cell.LayoutElement().SetFlexible(width: 1f);
      PinNameFilter.Cell.GetComponent<HorizontalLayoutGroup>().SetPadding(left: 8, right: 8, top: 5, bottom: 5);
      PinNameFilter.InputField.onValueChanged.AddListener(OnPinNameFilterChanged);

      PinTypeSelector = new(Panel.transform);
      PinTypeSelector.SetIconSize(new(18f, 18f));
      PinTypeSelector.Grid.GetComponent<GridLayoutGroup>().SetConstraintCount(1);

      Viewport = CreateChildViewport(Panel.transform);
      Content = CreateChildContent(Viewport.transform);
      ScrollRect = CreateChildScrollRect(Panel, Viewport, Content);

      PinStats = new(Panel.transform);
      PinStats.Cell.GetComponent<HorizontalLayoutGroup>().SetPadding(left: 8, right: 8, top: 5, bottom: 5);
      PinStats.Cell.Image().SetColor(new(0.5f, 0.5f, 0.5f, 0.1f));

      _pointerState = Panel.AddComponent<PointerState>();
    }

    public bool HasFocus() {
      return _pointerState.IsPointerHovered;
    }

    public readonly List<Minimap.PinData> TargetPins = new();
    Coroutine _refreshPinListRowsCoroutine;

    public void SetTargetPins(IEnumerable<Minimap.PinData> pins) {
      if (_refreshPinListRowsCoroutine != null) {
        Minimap.m_instance.StopCoroutine(_refreshPinListRowsCoroutine);
      }

      TargetPins.Clear();
      TargetPins.AddRange(pins.OrderBy(p => p.m_type).ThenBy(p => p.m_name));

      _refreshPinListRowsCoroutine = Minimap.m_instance.StartCoroutine(RefreshPinListRows());

      PinStats.Label.SetText($"{TargetPins.Count} pins.");
    }

    void OnPinNameFilterChanged(string value) {
      if (value == string.Empty) {
        SetTargetPins(Minimap.m_instance.m_pins);
        return;
      }

      SetTargetPins(
          Minimap.m_instance.m_pins
              .Where(
                  pin =>
                      pin.m_name.Length > 0
                      && pin.m_name.IndexOf(value, 0, StringComparison.InvariantCultureIgnoreCase) >= 0));
    }

    readonly List<PinListRow> _rowCache = new();
    int _visibleRows = 0;
    float _rowPreferredHeight = 0f;
    LayoutElement _bufferBlock;

    IEnumerator RefreshPinListRows() {
      yield return null;

      ScrollRect.SetVerticalScrollPosition(1f);
      _previousRowIndex = -1;

      yield return null;

      ScrollRect.onValueChanged.RemoveAllListeners();
      _rowCache.Clear();

      foreach (GameObject child in Content.Children()) {
        UnityEngine.Object.Destroy(child);
      }

      GameObject block = new("Block", typeof(RectTransform));
      block.SetParent(Content.transform);

      _bufferBlock = block.AddComponent<LayoutElement>();
      _bufferBlock.SetPreferred(height: 0f);

      PinListRow row = new(Content.transform);

      yield return null;

      _rowPreferredHeight = LayoutUtility.GetPreferredHeight(row.Row.RectTransform());
      _visibleRows = Mathf.CeilToInt(Viewport.RectTransform().sizeDelta.y / _rowPreferredHeight);

      UnityEngine.Object.Destroy(row.Row);

      yield return null;

      Content.RectTransform().SetSizeDelta(
          new(Viewport.RectTransform().sizeDelta.x, _rowPreferredHeight * TargetPins.Count));

      for (int i = 0; i < Mathf.Min(TargetPins.Count, _visibleRows); i++) {
        row = new(Content.transform);
        row.SetRowContent(TargetPins[i]);
        _rowCache.Add(row);
      }

      _previousRowIndex = -1;
      ScrollRect.SetVerticalScrollPosition(1f);

      if (TargetPins.Count > _visibleRows) {
        ScrollRect.onValueChanged.AddListener(OnVerticalScroll);
      }

      //yield return null;

      //foreach (Image image in _rowCache.Select(r => r.Row.Image())) {
      //  image.SetColor(image.color.SetAlpha(1f));
      //}
    }

    int _previousRowIndex = -1;

    void OnVerticalScroll(Vector2 scroll) {
      float scrolledY = Content.RectTransform().anchoredPosition.y;

      int rowIndex =
          Mathf.Clamp(Mathf.CeilToInt(scrolledY / _rowPreferredHeight), 0, TargetPins.Count - _rowCache.Count);

      if (rowIndex == _previousRowIndex) {
        return;
      }

      //ZLog.Log($"scrolledY {scrolledY}, rowIndex: {rowIndex}, previousRowIndex: {_previousRowIndex}, "
      //    + $"RowCacheCount: {_rowCache.Count}, TargetPins.Count: {TargetPins.Count}");

      if (rowIndex > _previousRowIndex) {
        PinListRow row = _rowCache[0];
        _rowCache.RemoveAt(0);
        row.Row.RectTransform().SetAsLastSibling();

        int index = Mathf.Clamp(rowIndex + _rowCache.Count, 0, TargetPins.Count - 1);
        row.SetRowContent(TargetPins[index]);
        _rowCache.Add(row);

        //ZLog.Log($"Removed row at 0, set to last with pin at index: {index}");
        //if (index == TargetPins.Count - 1) {
        //  ZLog.Log($"index: {index} Should be last.");
        //}
      } else {
        PinListRow row = _rowCache[_rowCache.Count - 1];
        _rowCache.RemoveAt(_rowCache.Count - 1);
        row.Row.RectTransform().SetSiblingIndex(1);
        row.SetRowContent(TargetPins[rowIndex]);
        _rowCache.Insert(0, row);

        //ZLog.Log($"Removed row at last, set to 0 with pin at index: {rowIndex}");
        //if (rowIndex == 0) {
        //  ZLog.Log($"Should be first.");
        //}
      }

      _bufferBlock.SetPreferred(height: rowIndex * _rowPreferredHeight);
      _previousRowIndex = rowIndex;
    }

    GameObject CreateChildPanel(Transform parentTransform) {
      GameObject panel = new("PinList.Panel", typeof(RectTransform));
      panel.SetParent(parentTransform);

      panel.AddComponent<VerticalLayoutGroup>()
          .SetChildControl(width: true, height: true)
          .SetChildForceExpand(width: false, height: false)
          .SetPadding(left: 10, right: 10, top: 10, bottom: 10)
          .SetSpacing(10);

      panel.AddComponent<Image>()
          .SetType(Image.Type.Sliced)
          .SetSprite(UIBuilder.CreateRoundedCornerSprite(400, 400, 15))
          .SetColor(new(0f, 0f, 0f, 0.8f));

      panel.AddComponent<CanvasGroup>()
          .SetBlocksRaycasts(true);

      return panel;
    }

    GameObject CreateChildViewport(Transform parentTransform) {
      GameObject viewport = new("PinList.Viewport", typeof(RectTransform));
      viewport.SetParent(parentTransform);

      viewport.AddComponent<RectMask2D>();

      viewport.AddComponent<LayoutElement>()
          .SetFlexible(width: 1f, height: 1f);

      viewport.AddComponent<Image>()
          .SetColor(Color.clear);

      return viewport;
    }

    GameObject CreateChildContent(Transform parentTransform) {
      GameObject content = new("PinList.Content", typeof(RectTransform));
      content.SetParent(parentTransform);

      content.RectTransform()
          .SetAnchorMin(Vector2.up)
          .SetAnchorMax(Vector2.up)
          .SetPivot(Vector2.up);

      content.AddComponent<VerticalLayoutGroup>()
          .SetChildControl(width: true, height: true)
          .SetChildForceExpand(width: false, height: false)
          .SetSpacing(0f);

      content.AddComponent<Image>()
          .SetColor(Color.clear);

      return content;
    }

    ScrollRect CreateChildScrollRect(GameObject panel, GameObject viewport, GameObject content) {
      return panel.AddComponent<ScrollRect>()
          .SetViewport(viewport.RectTransform())
          .SetContent(content.RectTransform())
          .SetHorizontal(false)
          .SetVertical(true)
          .SetScrollSensitivity(30f);
    }
  }
}
