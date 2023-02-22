using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace ComfyLib {
  public static class UIResources {
    static readonly Dictionary<string, Sprite> _spriteCache = new();

    public static Sprite GetSprite(string spriteName) {
      if (!_spriteCache.TryGetValue(spriteName, out Sprite sprite)) {
        sprite = Resources.FindObjectsOfTypeAll<Sprite>().FirstOrDefault(sprite => sprite.name == spriteName);
        _spriteCache[spriteName] = sprite;
      }

      return sprite;
    }

    static readonly Dictionary<string, Material> _materialCache = new();

    public static Material GetMaterial(string materialName) {
      if (!_materialCache.TryGetValue(materialName, out Material material)) {
        material =
            Resources.FindObjectsOfTypeAll<Material>().FirstOrDefault(material => material.name == materialName);
        _materialCache[materialName] = material;
      }

      return material;
    }

    static readonly Dictionary<string, Font> _fontCache = new();

    public static Font GetFont(string fontName) {
      if (!_fontCache.TryGetValue(fontName, out Font font)) {
        font = Resources.FindObjectsOfTypeAll<Font>().FirstOrDefault(font => font.name == fontName);
        _fontCache[fontName] = font;
      }

      return font;
    }

    public static Font AveriaSerifLibre { get => GetFont("AveriaSerifLibre-Regular"); }
    public static Font Norsebold { get => GetFont("Norsebold"); }

    public static Color Orange = new(1f, 0.7176f, 0.3603f, 1f);
  }
}
