using System;

using BepInEx.Configuration;

using UnityEngine;

namespace ComfyLib {
  public static class ConfigDrawer {
    public static Action<ConfigEntryBase> InputField() {
      return new InputField().Drawer;
    }
  }

  public class InputField {
    string _textFieldText;
    Color _textFieldColor;

    public void Drawer(ConfigEntryBase configEntry) {
      if (GUIFocus.HasChanged() || GUIHelper.IsEnterPressed()) {
        _textFieldText = configEntry.GetSerializedValue();
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
        configEntry.BoxedValue = TomlTypeConverter.ConvertToValue(value, configEntry.SettingType);
        _textFieldColor = configEntry.GetSerializedValue() == value ? GUI.color : Color.yellow;
      } catch {
        _textFieldColor = Color.red;
      }
    }
  }

  public class Vector3Field {
    public void Drawer(ConfigEntryBase configEntry) {

    }
  }
}
