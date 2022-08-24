using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

namespace Pinnacle {
  public static class UIBuilder {
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

    public static GameObject CreateRowSpacer(Transform parentTransform) {
      GameObject spacer = new($"{parentTransform.name}.Spacer", typeof(RectTransform));
      spacer.SetParent(parentTransform);

      spacer.AddComponent<LayoutElement>()
          .SetFlexible(width: 1f);

      return spacer;
    }

    static readonly Lazy<TextGenerator> CachedTextGenerator = new();

    public static float GetPreferredWidth(this Text text) {
      return GetPreferredWidth(text, text.text);
    }

    public static float GetPreferredWidth(Text text, string content) {
      return CachedTextGenerator.Value.GetPreferredWidth(
          content, text.GetGenerationSettings(text.rectTransform.rect.size));
    }

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
  }
}
