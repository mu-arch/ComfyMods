using UnityEngine;
using UnityEngine.UI;

using ComfyLib;

namespace Recipedia {
  public class RecipeListPanel {
    public GameObject Panel { get; private set; }
    public GameObject Viewport { get; private set; }
    public GameObject Content { get; private set; }
    public ScrollRect ScrollRect { get; private set; }

    public ValueCell RecipeNameFilter { get; private set; }

    public RecipeListPanel(Transform parentTransform) {
      Panel = CreateChildPanel(parentTransform);

      RecipeNameFilter = new(Panel.transform);
      RecipeNameFilter.Cell.LayoutElement().SetFlexible(width: 1f);

      Viewport = CreateChildViewport(Panel.transform);
      Content = CreateChildContent(Viewport.transform);
      ScrollRect = CreateChildScrollRect(Panel, Viewport, Content);
    }

    GameObject CreateChildPanel(Transform parentTransform) {
      GameObject panel = new("RecipeListPanel", typeof(RectTransform));
      panel.SetParent(parentTransform);

      panel.AddComponent<VerticalLayoutGroup>()
          .SetChildControl(width: true, height: true)
          .SetChildForceExpand(width: false, height: false)
          .SetPadding(left: 8, right: 8, top: 8, bottom: 8)
          .SetSpacing(8);

      panel.AddComponent<Image>()
          .SetType(Image.Type.Sliced)
          .SetSprite(UIBuilder.CreateRoundedCornerSprite(256, 256, 16))
          .SetColor(new(0f, 0f, 0f, 0.9f));

      return panel;
    }

    GameObject CreateChildViewport(Transform parentTransform) {
      GameObject viewport = new("Viewport", typeof(RectTransform));
      viewport.SetParent(parentTransform);

      viewport.AddComponent<RectMask2D>()
          .SetSoftness(Vector2Int.zero);

      viewport.AddComponent<LayoutElement>()
          .SetFlexible(width: 1f, height: 1f);

      viewport.AddComponent<Image>()
          .SetType(Image.Type.Sliced)
          .SetSprite(UIBuilder.CreateRoundedCornerSprite(128, 128, 8))
          .SetColor(new(0.5f, 0.5f, 0.5f, 0.1f));

      viewport.AddComponent<Outline>()
          .SetEffectDistance(new(2f, -2f));

      return viewport;
    }

    GameObject CreateChildContent(Transform parentTransform) {
      GameObject content = new("Content", typeof(RectTransform));
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
          .SetColor(Color.clear)
          .SetRaycastTarget(true);

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
