using System.Globalization;

using BepInEx.Configuration;

using UnityEngine;

namespace ComfyLib {
  public class FloatConfigEntry {
    public ConfigEntry<float> ConfigEntry { get; }
    public float Value { get; private set; }

    string _fieldText;
    Color _fieldColor;

    public FloatConfigEntry(ConfigFile config, string section, string key, float defaultValue, string description) {
      ConfigEntry = config.BindInOrder(section, key, defaultValue, description, Drawer);
      SetValue(ConfigEntry.Value);
    }

    public void SetValue(float value) {
      ConfigEntry.Value = value;
      Value = value;

      _fieldText = value.ToString(NumberFormatInfo.InvariantInfo);
      _fieldColor = GUI.color;
    }

    public void Drawer(ConfigEntryBase configEntry) {
      float configValue = (float) configEntry.BoxedValue;

      if (GUIFocus.HasChanged() || GUIHelper.IsEnterPressed() || Value != configValue) {
        SetValue(configValue);
      }

      GUIHelper.BeginColor(_fieldColor);
      string textValue = GUILayout.TextField(_fieldText, GUILayout.ExpandWidth(true));
      GUIHelper.EndColor();

      if (textValue == _fieldText) {
        return;
      }

      _fieldText = textValue;

      if (ShouldParse(textValue)
          && float.TryParse(textValue, NumberStyles.Float, NumberFormatInfo.InvariantInfo, out float result)) {
        Value = result;
        ConfigEntry.Value = result;
        _fieldColor = GUI.color;
      } else {
        _fieldColor = Color.red;
      }
    }

    static bool ShouldParse(string text) {
      if (text == null || text.Length <= 0) {
        return false;
      }

      return text[text.Length - 1] switch {
        'e' or 'E' or '+' or '-' or '.' or ',' => false,
        _ => true,
      };
    }
  }
}
