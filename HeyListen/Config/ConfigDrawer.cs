using System;
using System.Globalization;

using BepInEx.Configuration;

using UnityEngine;

namespace ComfyLib {
  public static class ConfigDrawer {
    public static Action<ConfigEntryBase> InputField() {
      return new InputField().Drawer;
    }
  }

  public class InputField {
    string _textFieldText = string.Empty;
    Color _textFieldColor;

    public void Drawer(ConfigEntryBase configEntry) {
      if (GUIFocus.HasChanged()) {
        _textFieldText = Convert.ToString(configEntry.BoxedValue, CultureInfo.InvariantCulture);
        _textFieldColor = GUI.color;
      }

      GUIHelper.BeginColor(_textFieldColor);
      string value = GUILayout.TextField(_textFieldText, GUILayout.ExpandWidth(true));
      GUIHelper.EndColor();

      if (value == _textFieldText) {
        return;
      }

      _textFieldText = value;

      try {
        configEntry.BoxedValue = Convert.ChangeType(value, configEntry.SettingType, CultureInfo.InvariantCulture);
        _textFieldColor = GUI.color;
      } catch {
        _textFieldColor = Color.red;
      }
    }
  }
}
