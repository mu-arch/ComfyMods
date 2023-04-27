using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

namespace ComfyLib {
  public static class UIBuilder {
    static readonly Dictionary<string, Sprite> RoundedCornerSpriteCache = new();

    static readonly Color32 ColorWhite = Color.white;
    static readonly Color32 ColorClear = Color.clear;

    public static Sprite CreateRoundedCornerSprite(
        int width, int height, int radius, FilterMode filterMode = FilterMode.Bilinear) {
      string name = $"RoundedCorner-{width}w-{height}h-{radius}r";

      if (RoundedCornerSpriteCache.TryGetValue(name, out Sprite sprite)) {
        return sprite;
      }

      Texture2D texture =
          new Texture2D(width, height)
              .SetName(name)
              .SetWrapMode(TextureWrapMode.Clamp)
              .SetFilterMode(filterMode);

      Color32[] pixels = new Color32[width * height];

      for (int y = 0; y < height; y++) {
        for (int x = 0; x < width; x++) {
          pixels[(y * width) + x] = IsCornerPixel(x, y, width, height, radius) ? ColorClear : ColorWhite;
        }
      }

      texture.SetPixels32(pixels);
      texture.Apply();

      int borderWidth;
      for (borderWidth = 0; borderWidth < width; borderWidth++) {
        if (pixels[borderWidth] == Color.white) {
          break;
        }
      }

      int borderHeight;
      for (borderHeight = 0; borderHeight < height; borderHeight++) {
        if (pixels[borderHeight * width] == Color.white) {
          break;
        }
      }

      sprite =
          Sprite.Create(
              texture,
              new(0, 0, width, height),
              new(0.5f, 0.5f),
              pixelsPerUnit: 100f,
              extrude: 0,
              SpriteMeshType.FullRect,
              new(borderWidth, borderHeight, borderWidth, borderHeight))
          .SetName(name);

      RoundedCornerSpriteCache[name] = sprite;
      return sprite;
    }

    static bool IsCornerPixel(int x, int y, int w, int h, int rad) {
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

    public static GameObject CreateLabel(Transform parentTransform) {
      GameObject label = new($"{parentTransform.name}.Label", typeof(RectTransform));
      label.SetParent(parentTransform);

      label.AddComponent<Text>()
          .SetSupportRichText(true)
          .SetFont(UIResources.AveriaSerifLibre)
          .SetFontSize(16)
          .SetAlignment(TextAnchor.MiddleLeft)
          .SetColor(Color.white)
          .SetResizeTextForBestFit(false);

      label.AddComponent<Outline>()
          .SetEffectColor(Color.black);

      return label;
    }

    public static GameObject CreateResizer(Transform parentTransform) {
      GameObject resizer = new("Resizer", typeof(RectTransform));
      resizer.SetParent(parentTransform);

      resizer.AddComponent<LayoutElement>()
          .SetIgnoreLayout(true);

      resizer.RectTransform()
          .SetAnchorMin(Vector2.right)
          .SetAnchorMax(Vector2.right)
          .SetPivot(Vector2.right)
          .SetSizeDelta(new(40f, 40f))
          .SetPosition(new(10f, -10f));

      resizer.AddComponent<Image>()
          .SetType(Image.Type.Sliced)
          .SetSprite(CreateRoundedCornerSprite(128, 128, 12))
          .SetColor(new(0.565f, 0.792f, 0.976f, 0.849f));

      resizer.AddComponent<Shadow>()
          .SetEffectDistance(new(2f, -2f));

      resizer.AddComponent<CanvasGroup>()
          .SetAlpha(0f);

      GameObject icon = CreateLabel(resizer.transform);
      icon.SetName("Resizer.Icon");

      icon.AddComponent<LayoutElement>()
          .SetIgnoreLayout(true);

      icon.RectTransform()
          .SetAnchorMin(Vector2.zero)
          .SetAnchorMax(Vector2.one)
          .SetPivot(new(0.5f, 0.5f))
          .SetSizeDelta(Vector2.zero);

      icon.Text()
          .SetAlignment(TextAnchor.MiddleCenter)
          .SetFontSize(28)
          .SetHorizontalOverflow(HorizontalWrapMode.Overflow)
          .SetVerticalOverflow(VerticalWrapMode.Overflow)
          .SetText("\u21D6\u21D8");

      return resizer;
    }
  }
}
