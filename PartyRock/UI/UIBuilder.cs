using System;

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
          .SetChildForceExpand(width: true, height: false)
          .SetPadding(10, 10, 10, 10);

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
          .SetSpacing(15f)
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

    public static GameObject CreateColumn(Transform parentTransform) {
      GameObject column = new("Column", typeof(RectTransform));
      column.SetParent(parentTransform);

      column.GetComponent<RectTransform>()
          .SetAnchorMin(Vector2.zero)
          .SetAnchorMax(Vector2.zero)
          .SetPivot(Vector2.zero)
          .SetPosition(Vector2.zero);

      column.AddComponent<VerticalLayoutGroup>()
          .SetChildControl(width: true, height: true)
          .SetChildForceExpand(width: false, height: false)
          .SetChildAlignment(TextAnchor.MiddleLeft)
          .SetPadding(left: 0, right: 0, top: 0, bottom: 0)
          .SetSpacing(5);

      return column;
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
          .SetSupportRichText(true)
          .SetFont(UIResources.AveriaSerifLibre)
          .SetFontSize(16)
          .SetAlignment(TextAnchor.MiddleLeft)
          .SetColor(Color.white);

      label.AddComponent<Outline>()
          .SetEffectColor(Color.black);

      return label;
    }

    public static Sprite CreateGradientSprite() {
      Texture2D texture = new(width: 1, height: 2);
      texture.wrapMode = TextureWrapMode.Clamp;
      texture.SetPixel(0, 0, new(0.8f, 0.8f, 0.8f, 1f));
      texture.SetPixel(0, 1, Color.white);
      texture.Apply();

      return Sprite.Create(texture, new(0, 0, 1, 2), Vector2.zero);
    }

    public static Sprite CreateRoundedCornerSprite(int width, int height, int radius) {
      Texture2D texture = new(width, height);
      texture.name = $"RoundedCorner-{width}w-{height}h-{radius}r";
      texture.wrapMode = TextureWrapMode.Clamp;

      for (int x = 0; x < width; x++) {
        for (int y = 0; y < height; y++) {
          texture.SetPixel(x, y, IsCornerPixel(x, y, width, height, radius) ? Color.clear : Color.white);
        }
      }

      texture.Apply();

      Sprite sprite =
          Sprite.Create(
              texture,
              new(0, 0, width, height),
              new(0.5f, 0.5f),
              pixelsPerUnit: 100f,
              0,
              SpriteMeshType.FullRect,
              new(width * 0.15f, height * 0.15f, width * 0.15f, height * 0.15f));

      sprite.name = $"RoundedCorner-{width}w-{height}h-{radius}r";
      return sprite;
    }

    public static bool IsCornerPixel(int x, int y, int w, int h, int rad) {
      if (rad == 0) {
        return false;
      }

      int dx = Math.Min(x, w - x);
      int dy = Math.Min(y, h - y);

      if (dx == 0 && dy == 0) {
        return true;
      }

      if (dx > rad || dy > rad) {
        return false;
      }

      dx = rad - dx;
      dy = rad - dy;

      return Math.Round(Math.Sqrt(dx * dx + dy * dy)) > rad;
    }
  }
}
