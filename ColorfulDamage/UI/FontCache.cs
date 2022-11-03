using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace ComfyLib {
  public static class FontCache {
    static readonly Dictionary<string, Font> _fontCache = new();

    public static Font GetFont(string name) {
      if (!_fontCache.TryGetValue(name, out Font font)) {
        font = Resources.FindObjectsOfTypeAll<Font>().First(f => f.name == name);
        _fontCache[name] = font;
      }

      return font;
    }
  }
}
