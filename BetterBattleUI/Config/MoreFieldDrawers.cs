using BepInEx.Configuration;

using System;
using System.Collections.Generic;

using UnityEngine;

namespace ComfyLib {
  public static class MoreFieldDrawers {
    private sealed class ColorTexture {
      public Color Color { get; private set; }
      public string HtmlColor { get; private set; }
      public Texture2D Texture { get; private set; }

      public ColorTexture(Color color) {
        Texture = new(40, 10, TextureFormat.ARGB32, false);
        SetColor(color);
      }

      public void SetColor(Color color) {
        Color = color;
        HtmlColor = ColorUtility.ToHtmlStringRGBA(color);

        for (int x = 0; x < Texture.width; x++) {
          for (int y = 0; y < Texture.height; y++) {
            Texture.SetPixel(x, y, Color);
          }
        }

        Texture.Apply(false);
      }
    }

    private static readonly Dictionary<ConfigEntryBase, ColorTexture> _colorTextureCache = new();

    public static void ExtendedColorDrawer(ConfigEntryBase entry) {
      if (!typeof(Color).IsAssignableFrom(entry.SettingType)) {
        throw new ArgumentException($"{entry.Definition} is not of type UnityEngine.Color!");
      }

      Color color = (Color) entry.BoxedValue;

      if (!_colorTextureCache.TryGetValue(entry, out ColorTexture colorTexture)) {
        colorTexture = new(color);
        _colorTextureCache[entry] = colorTexture;
      }

      RectOffset margin = GUI.skin.horizontalSlider.margin;
      GUI.skin.horizontalSlider.margin = new(0, 0, 9, 9);

      GUILayout.BeginVertical();

      GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
      GUILayout.BeginVertical();

      GUILayout.BeginHorizontal();
      GUILayout.Label("R", GUILayout.ExpandWidth(false));
      color.r = GUILayout.HorizontalSlider(color.r, 0f, 1f, GUILayout.ExpandWidth(true));
      GUILayout.Label(color.r.ToString("0.00"), GUILayout.Width(30f));
      GUILayout.EndHorizontal();

      GUILayout.BeginHorizontal();
      GUILayout.Label("G",  GUILayout.ExpandWidth(false));
      color.g = GUILayout.HorizontalSlider(color.g, 0f, 1f, GUILayout.ExpandWidth(true));
      GUILayout.Label(color.g.ToString("0.00"), GUILayout.Width(30f));
      GUILayout.EndHorizontal();

      GUILayout.BeginHorizontal();
      GUILayout.Label("B", GUILayout.ExpandWidth(false));
      color.b = GUILayout.HorizontalSlider(color.b, 0f, 1f, GUILayout.ExpandWidth(true));
      GUILayout.Label(color.b.ToString("0.00"), GUILayout.Width(30f));
      GUILayout.EndHorizontal();

      GUILayout.BeginHorizontal();
      GUILayout.Label("A", GUILayout.ExpandWidth(false));
      color.a = GUILayout.HorizontalSlider(color.a, 0f, 1f, GUILayout.ExpandWidth(true));
      GUILayout.Label(color.a.ToString("0.00"), GUILayout.Width(30f));
      GUILayout.EndHorizontal();

      GUILayout.EndVertical();

      GUILayout.Space(10f);

      GUILayout.BeginVertical();

      GUILayout.BeginHorizontal();
      GUILayout.Label(colorTexture.Texture, GUILayout.ExpandWidth(true));
      GUILayout.EndHorizontal();

      GUILayout.BeginHorizontal();
      GUILayout.Label(colorTexture.HtmlColor, GUILayout.Width(80f));
      GUILayout.EndHorizontal();

      GUILayout.EndVertical();
      GUILayout.EndHorizontal();

      GUILayout.EndVertical();

      if (color != colorTexture.Color) {
        entry.BoxedValue = color;
        colorTexture.SetColor(color);
      }

      GUI.skin.horizontalSlider.margin = margin;
    }
  }
}
