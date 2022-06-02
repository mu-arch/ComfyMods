using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace PartyRock {
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
  }
}
