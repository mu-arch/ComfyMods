using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace Chatter {
  public static class UIResources {
    static readonly Dictionary<string, Font> _fontCache = new();

    public static Font GetFont(string fontName) {
      if (_fontCache.TryGetValue(fontName, out Font cachedFont)) {
        return cachedFont;
      }

      if (Font.GetOSInstalledFontNames().Contains(fontName)) {
        _fontCache[fontName] = Font.CreateDynamicFontFromOSFont(fontName, size: 20);
      } else {
        foreach (Font font in Resources.FindObjectsOfTypeAll<Font>()) {
          _fontCache[font.name] = font;
        }
      }

      _fontCache.TryGetValue(fontName, out cachedFont);
      return cachedFont;
    }

    public static Font AveriaSerifLibre { get => GetFont("AveriaSerifLibre-Regular"); }
  }
}