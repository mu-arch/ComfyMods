using UnityEngine;
using UnityEngine.UI;

namespace PartyRock {
  public class UIBuilder {
    public static GameObject CreatePanel(Transform parentTransform) {
      GameObject panel = new("Panel", typeof(RectTransform));
      panel.SetParent(parentTransform);

      panel.GetComponent<RectTransform>()
          .SetAnchorMin(new(0f, 0.5f))
          .SetAnchorMax(new(0f, 0.5f))
          .SetPivot(new(0f, 0.5f))
          .SetPosition(Vector2.zero);

      panel.AddComponent<VerticalLayoutGroup>()
          .SetChildControl(width: true, height: true)
          .SetChildForceExpand(width: true, height: false);

      panel.AddComponent<Image>()
          .SetColor(Color.clear);

      panel.AddComponent<CanvasGroup>()
          .SetBlocksRaycasts(true);

      return panel;
    }

    public static GameObject CreateViewport(Transform parentTransform) {
      GameObject viewport = new("Viewport", typeof(RectTransform));
      viewport.SetParent(parentTransform);

      viewport.GetComponent<RectTransform>()
          .SetAnchorMin(Vector2.zero)
          .SetAnchorMax(Vector2.one)
          .SetPivot(Vector2.up)
          .SetPosition(Vector2.zero);

      viewport.AddComponent<RectMask2D>();

      viewport.AddComponent<LayoutElement>()
          .SetFlexible(width: 1f, height: 1f);

      viewport.AddComponent<Image>()
          .SetColor(Color.clear);

      return viewport;
    }

    public static GameObject CreateContent(Transform parentTransform) {
      GameObject content = new("Content", typeof(RectTransform));
      content.SetParent(parentTransform);

      content.GetComponent<RectTransform>()
          .SetAnchorMin(Vector2.zero)
          .SetAnchorMax(Vector2.one)
          .SetPivot(Vector2.up)
          .SetPosition(Vector2.zero);

      content.AddComponent<VerticalLayoutGroup>()
          .SetChildControl(width: true, height: true)
          .SetChildForceExpand(width: false, height: false)
          .SetSpacing(5f)
          .SetPadding(left: 0, right: 0, top: 0, bottom: 0);

      content.AddComponent<ContentSizeFitter>()
          .SetHorizontalFit(ContentSizeFitter.FitMode.Unconstrained)
          .SetVerticalFit(ContentSizeFitter.FitMode.PreferredSize);

      content.AddComponent<ParentSizeFitter>();

      content.AddComponent<Image>()
          .SetColor(Color.clear);

      return content;
    }

    public static ScrollRect CreateScrollRect(GameObject panel, GameObject viewport, GameObject content) {
      return panel.AddComponent<ScrollRect>()
          .SetViewport(viewport.GetComponent<RectTransform>())
          .SetContent(content.GetComponent<RectTransform>())
          .SetHorizontal(false)
          .SetVertical(true)
          .SetScrollSensitivity(30f);
    }

    public static GameObject CreateRow(Transform parentTransform) {
      GameObject row = new("Row", typeof(RectTransform));
      row.SetParent(parentTransform);

      row.GetComponent<RectTransform>()
          .SetAnchorMin(Vector2.zero)
          .SetAnchorMax(Vector2.zero)
          .SetPivot(Vector2.zero)
          .SetPosition(Vector2.zero);

      row.AddComponent<HorizontalLayoutGroup>()
          .SetChildControl(width: true, height: true)
          .SetChildForceExpand(width: false, height: false)
          .SetChildAlignment(TextAnchor.MiddleLeft)
          .SetPadding(left: 0, right: 0, top: 0, bottom: 0)
          .SetSpacing(5);

      return row;
    }

    public static GameObject CreateSpacer(Transform parentTransform) {
      GameObject spacer = new("Spacer", typeof(RectTransform));
      spacer.SetParent(parentTransform);

      spacer.AddComponent<LayoutElement>().
          SetFlexible(width: 1f);

      return spacer;
    }

    public static GameObject CreateLabel(Transform parentTransform) {
      GameObject label = new("Label", typeof(RectTransform));
      label.SetParent(parentTransform);

      label.AddComponent<Text>()
          .SetFont(UIResources.AveriaSerifLibre)
          .SetFontSize(16)
          .SetAlignment(TextAnchor.MiddleLeft)
          .SetColor(Color.white);

      label.AddComponent<Outline>()
          .SetEffectColor(Color.black);

      return label;
    }
  }
}
