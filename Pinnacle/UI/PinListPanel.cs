using System.Collections;
using System.Collections.Generic;
using System.Reflection;

using UnityEngine;
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

    public PinListPanel(Transform parentTransform) {
      Panel = CreateChildPanel(parentTransform);

      PinNameFilter = new(Panel.transform);
      PinNameFilter.Cell.LayoutElement().SetFlexible(width: 1f);
      PinNameFilter.Cell.GetComponent<HorizontalLayoutGroup>().SetPadding(left: 8, right: 8, top: 5, bottom: 5);

      PinTypeSelector = new(Panel.transform);
      PinTypeSelector.SetIconSize(new(18f, 18f));
      PinTypeSelector.Grid.GetComponent<GridLayoutGroup>().SetConstraintCount(1);

      Viewport = CreateChildViewport(Panel.transform);
      Content = CreateChildContent(Viewport.transform);
      ScrollRect = CreateChildScrollRect(Panel, Viewport, Content);

      PinStats = new(Panel.transform);
      PinStats.Cell.GetComponent<HorizontalLayoutGroup>().SetPadding(left: 8, right: 8, top: 5, bottom: 5);
      PinStats.Cell.Image().SetColor(new(0.5f, 0.5f, 0.5f, 0.1f));
    }

    public readonly List<Minimap.PinData> TargetPins = new();

    public void SetTargetPins(List<Minimap.PinData> pins) {
      TargetPins.Clear();
      TargetPins.AddRange(pins);

      Minimap.m_instance.StartCoroutine(RefreshPinListRows());

      PinStats.Label.SetText($"{TargetPins.Count} pins.");
    }

    readonly List<PinListRow> _rowCache = new();
    float _rowPreferredHeight = 0f;
    LayoutElement _bufferBlock;

    IEnumerator RefreshPinListRows() {
      yield return null;

      ScrollRect.onValueChanged.RemoveAllListeners();

      _rowCache.Clear();

      foreach (GameObject child in Content.Children()) {
        Object.Destroy(child);
      }

      if (TargetPins.Count == 0) {
        yield break;
      }

      GameObject block = new("Block", typeof(RectTransform));
      block.SetParent(Content.transform);

      _bufferBlock = block.AddComponent<LayoutElement>();
      _bufferBlock.SetPreferred(height: 0f);

      PinListRow row = new(Content.transform);
      row.SetRowContent(TargetPins[0]);
      _rowCache.Add(row);

      yield return null;

      _rowPreferredHeight = LayoutUtility.GetPreferredHeight(row.Row.RectTransform());
      int visibleRows = Mathf.CeilToInt(Viewport.RectTransform().sizeDelta.y / _rowPreferredHeight);

      Content.RectTransform().SetSizeDelta(
          new(Viewport.RectTransform().sizeDelta.x, _rowPreferredHeight * TargetPins.Count));

      for (int i = 1; i < Mathf.Min(TargetPins.Count, visibleRows); i++) {
        row = new(Content.transform);
        row.SetRowContent(TargetPins[i]);
        _rowCache.Add(row);
      }

      ScrollRect.SetVerticalScrollPosition(1f);

      if (TargetPins.Count > visibleRows) {
        ScrollRect.onValueChanged.AddListener(OnVerticalScroll);
      }
    }

    int _previousRowIndex = 0;

    void OnVerticalScroll(Vector2 scroll) {
      float scrolledY = Content.RectTransform().anchoredPosition.y;

      int rowIndex =
          Mathf.Clamp(Mathf.CeilToInt(scrolledY / _rowPreferredHeight), 0, TargetPins.Count - _rowCache.Count - 1);

      if (rowIndex == _previousRowIndex) {
        return;
      }

      ZLog.Log($"scrolledY {scrolledY}, rowIndex: {rowIndex}, previousRowIndex: {_previousRowIndex}");

      if (rowIndex > _previousRowIndex) {
        PinListRow row = _rowCache[0];
        _rowCache.RemoveAt(0);
        row.Row.RectTransform().SetAsLastSibling();

        int index = Mathf.Clamp(rowIndex + _rowCache.Count, 0, TargetPins.Count - 1);
        row.SetRowContent(TargetPins[index]);
        _rowCache.Add(row);

        ZLog.Log($"Removed row at 0, set to last with pin at index: {index}");
      } else {
        PinListRow row = _rowCache[_rowCache.Count - 1];
        _rowCache.RemoveAt(_rowCache.Count - 1);
        row.Row.RectTransform().SetSiblingIndex(1);
        row.SetRowContent(TargetPins[rowIndex]);
        _rowCache.Insert(0, row);

        ZLog.Log($"Removed row at last, set to 0 with pin at index: {rowIndex}");
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
