using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace Pinnacle {
  public class UIResources {
    static readonly Dictionary<string, Font> FontCache = new();

    public static Font FindFont(string name) {
      if (!FontCache.TryGetValue(name, out Font font)) {
        font = Resources.FindObjectsOfTypeAll<Font>().First(f => f.name == name);
        FontCache[name] = font;
      }

      return font;
    }

    public static Font AveriaSerifLibre { get => FindFont("AveriaSerifLibre-Regular"); }

    static readonly Dictionary<string, Sprite> SpriteCache = new();

    public static Sprite GetSprite(string spriteName) {
      if (!SpriteCache.TryGetValue(spriteName, out Sprite sprite)) {
        sprite = Resources.FindObjectsOfTypeAll<Sprite>().FirstOrDefault(sprite => sprite.name == spriteName);
        SpriteCache[spriteName] = sprite;
      }

      return sprite;
    }
  }
}
