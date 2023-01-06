using System.Globalization;

using BepInEx.Configuration;

using UnityEngine;

namespace ComfyLib {
  public class FloatConfigEntry {
    readonly FloatTextField _valueInput;
    float _currentValue;

    public FloatConfigEntry() {
      _valueInput = new();
      _currentValue = float.NaN;
    }

    public void SetValue(float value) {
      _valueInput.SetValue(value);
      _currentValue = value;
    }

    public void Drawer(ConfigEntryBase configEntry) {
      float configValue = (float) configEntry.BoxedValue;

      if (GUIFocus.HasChanged() || GUIHelper.IsEnterPressed() || _currentValue != configValue) {
        SetValue(configValue);
      }

      _valueInput.DrawField();

      if (_valueInput.CurrentValue != configValue) {
        configEntry.BoxedValue = _valueInput.CurrentValue;
      }
    }
  }

  public class FloatTextField {
    public float CurrentValue { get; private set; }
    public string CurrentText { get; private set; }
    public Color CurrentColor { get; private set; }

    public void SetValue(float value) {
      CurrentValue = value;
      CurrentText = GetFormattedText(value);
      CurrentColor = GUI.color;
    }

    string GetFormattedText(float value) {
      string text = value.ToString(NumberFormatInfo.InvariantInfo);
      return text.Contains(".") ? text : text + ".0";
    }

    public void DrawField() {
      GUIHelper.BeginColor(CurrentColor);
      string textValue = GUILayout.TextField(CurrentText, GUILayout.ExpandWidth(true));
      GUIHelper.EndColor();

      if (textValue == CurrentText) {
        return;
      }

      if (float.TryParse(textValue, NumberStyles.Any, NumberFormatInfo.InvariantInfo, out float result)) {
        CurrentValue = result;
        CurrentColor = GUI.color;
      } else {
        CurrentColor = Color.red;
      }

      CurrentText = textValue;
    }
  }
}
