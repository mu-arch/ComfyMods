using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace ComfyLib {
  public class UIResources {
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
  }
}
