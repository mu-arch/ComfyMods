using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace ZoneScouter {
  public class UIResources {
    static readonly Dictionary<string, Font> _fontCache = new();

    public static Font GetFont(string name) {
      if (!_fontCache.TryGetValue(name, out Font font)) {
        font = Resources.FindObjectsOfTypeAll<Font>().First(f => f.name == name);
        _fontCache[name] = font;
      }

      return font;
    }

    public static Font AveriaSerifLibre { get => GetFont("AveriaSerifLibre-Regular"); }

    static readonly Dictionary<string, Sprite> _spriteCache = new();

    public static Sprite GetSprite(string spriteName) {
      if (!_spriteCache.TryGetValue(spriteName, out Sprite sprite)) {
        sprite = Resources.FindObjectsOfTypeAll<Sprite>().FirstOrDefault(sprite => sprite.name == spriteName);
        _spriteCache[spriteName] = sprite;
      }

      return sprite;
    }
  }
}
