using System;
using System.Linq;

using BepInEx.Configuration;

using UnityEngine;

namespace ComfyLib {
  public static class KeyboardShortcutExtensions {
    static readonly Func<KeyCode, bool> _modifiersGetKeyDelegate = m => Input.GetKey(m);

    public static bool IsKeyDown(this KeyboardShortcut shortcut) {
      return Input.GetKeyDown(shortcut.MainKey) && shortcut.Modifiers.All(_modifiersGetKeyDelegate);
    }
  }
}
