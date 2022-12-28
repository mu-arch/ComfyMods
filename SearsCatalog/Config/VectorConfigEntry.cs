using System.Globalization;

using BepInEx.Configuration;

using UnityEngine;

namespace ComfyLib {
  public class Vector2ConfigEntry {
    readonly VectorFloatTextField _xValueInput;
    readonly VectorFloatTextField _yValueInput;
    Vector2 _currentValue;

    public Vector2ConfigEntry() {
      _xValueInput = new();
      _yValueInput = new();
      _currentValue = default;
    }

    public void SetValue(Vector2 value) {
      _xValueInput.SetValue(value.x);
      _yValueInput.SetValue(value.y);
      _currentValue = value;
    }

    public void Drawer(ConfigEntryBase configEntry) {
      Vector2 configValue = (Vector2) configEntry.BoxedValue;

      if (GUIFocus.HasChanged() || GUIHelper.IsEnterPressed() || _currentValue != configValue) {
        SetValue(configValue);
      }

      GUILayout.Label("X", GUILayout.ExpandWidth(false));
      _xValueInput.DrawField();

      GUILayout.Label("Y", GUILayout.ExpandWidth(false));
      _yValueInput.DrawField();

      Vector2 value = new(_xValueInput.CurrentValue, _yValueInput.CurrentValue);

      if (value != configValue) {
        configEntry.BoxedValue = value;
      }
    }
  }

  public class VectorFloatTextField {
    public float CurrentValue { get; private set; }
    public string CurrentText { get; private set; }
    public Color CurrentColor { get; private set; }

    public void SetValue(float value) {
      CurrentValue = value;
      CurrentText = value.ToString("F", NumberFormatInfo.InvariantInfo);
      CurrentColor = GUI.color;
    }

    public void DrawField() {
      GUIHelper.BeginColor(CurrentColor);

      string textValue =
          GUILayout.TextField(
              CurrentText, GUILayout.MinWidth(50f), GUILayout.MaxWidth(100f), GUILayout.ExpandWidth(true));

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
