using System;
using System.Globalization;

using BepInEx.Configuration;

using UnityEngine;

namespace ComfyLib {
  public class ExtendedColorConfig {
    public ExtendedColorConfig() {
      _redInput = new("R");
      _greenInput = new("G");
      _blueInput = new("B");
      _alphaInput = new("A");
      _hexInput = new();
    }

    readonly FloatInputField _redInput;
    readonly FloatInputField _greenInput;
    readonly FloatInputField _blueInput;
    readonly FloatInputField _alphaInput;
    readonly HexColorField _hexInput;
    readonly Texture2D _colorTexture = new(20, 20, TextureFormat.ARGB32, mipChain: false);

    bool _showSliders = false;

    void SetValue(Color value) {
      _redInput.SetValue(value.r);
      _greenInput.SetValue(value.g);
      _blueInput.SetValue(value.b);
      _alphaInput.SetValue(value.a);
      _hexInput.SetValue(value);
    }
    
    public void DrawColor(ConfigEntryBase configEntry) {
      Color configColor = (Color) configEntry.BoxedValue;

      if (GUIFocus.HasChanged() || GUIHelper.IsEnterPressed()) {
        SetValue(configColor);
      }

      GUILayout.BeginVertical();
      GUILayout.BeginHorizontal();
      _hexInput.DrawField();

      GUILayout.Space(3f);
      GUIHelper.BeginColor(configColor);
      GUILayout.Label(string.Empty, GUILayout.ExpandWidth(true));
      GUI.DrawTexture(GUILayoutUtility.GetLastRect(), _colorTexture);
      GUIHelper.EndColor();
      GUILayout.Space(3f);

      if (GUILayout.Button(_showSliders ? "\u2228" : "\u2261", GUILayout.MinWidth(40f), GUILayout.ExpandWidth(false))) {
        _showSliders = !_showSliders;
      }

      GUILayout.EndHorizontal();

      if (_showSliders) {
        GUILayout.Space(4f);
        GUILayout.BeginHorizontal();

        _redInput.DrawField();
        GUILayout.Space(3f);
        _greenInput.DrawField();
        GUILayout.Space(3f);
        _blueInput.DrawField();
        GUILayout.Space(3f);
        _alphaInput.DrawField();

        GUILayout.EndHorizontal();
      }

      GUILayout.EndVertical();

      if (ConfigGUILayout.DefaultButton()) {
        Color defaultColor = (Color) configEntry.DefaultValue;
        configEntry.BoxedValue = defaultColor;
        SetValue(defaultColor);
        return;
      }

      Color sliderColor =
          new(_redInput.CurrentValue, _greenInput.CurrentValue, _blueInput.CurrentValue, _alphaInput.CurrentValue);

      if (sliderColor != configColor) {
        configEntry.BoxedValue = sliderColor;
        SetValue(sliderColor);
      } else if (_hexInput.CurrentValue != configColor) {
        configEntry.BoxedValue = _hexInput.CurrentValue;
        SetValue(_hexInput.CurrentValue);
      }
    }
  }

  public class FloatInputField {
    public string FieldLabel { get; set; }

    public float CurrentValue { get; set; }
    public string CurrentText { get; set; }

    public FloatInputField(string label) {
      FieldLabel = label;
    }

    public void SetValue(float value) {
      CurrentValue = value;
      CurrentText = value.ToString("F3", CultureInfo.InvariantCulture);
      _currentColor = GUI.color;
    }

    Color _currentColor = GUI.color;

    public void DrawField() {
      GUILayout.BeginVertical();

      GUILayout.BeginHorizontal();
      GUILayout.Label(FieldLabel, GUILayout.ExpandWidth(false));

      GUIHelper.BeginColor(_currentColor);
      string textValue = GUILayout.TextField(CurrentText, GUILayout.Width(50f), GUILayout.ExpandWidth(false));
      GUIHelper.EndColor();

      GUILayout.EndHorizontal();
      GUILayout.Space(2f);

      float sliderValue =
          GUILayout.HorizontalSlider(CurrentValue, 0f, 1f, GUILayout.ExpandWidth(true));

      GUILayout.EndVertical();

      if (sliderValue != CurrentValue) {
        SetValue(sliderValue);
        return;
      }

      if (textValue == CurrentText) {
        return;
      }

      // TODO(redseiko@): extract the range-check into a parameter for the constructor.
      if (float.TryParse(textValue, NumberStyles.Any, CultureInfo.InvariantCulture, out float result)
          && result >= 0f
          && result <= 1f) {
        CurrentValue = result;
        _currentColor = GUI.color;
      } else {
        _currentColor = Color.red;
      }

      CurrentText = textValue;
    }
  }

  public class HexColorField {
    public Color CurrentValue { get; private set; }
    public string CurrentText { get; private set; }

    Color _textColor = GUI.color;

    public void SetValue(Color value) {
      CurrentValue = value;
      CurrentText = $"#{ColorUtility.ToHtmlStringRGBA(value)}";

      _textColor = GUI.color;
    }

    public void DrawField() {
      GUIHelper.BeginColor(_textColor);
      string textValue = GUILayout.TextField(CurrentText, GUILayout.Width(100f), GUILayout.ExpandWidth(false));
      GUIHelper.EndColor();

      if (textValue == CurrentText) {
        return;
      }

      CurrentText = textValue;

      if (ColorUtility.TryParseHtmlString(textValue, out Color color)) {
        CurrentValue = color;
        _textColor = GUI.color;
      } else {
        _textColor = Color.red;
      }
    }
  }

  public static class ConfigGUILayout {
    public static bool DefaultButton() {
      GUILayout.Space(5);
      return GUILayout.Button("Reset", GUILayout.ExpandWidth(false));
    }
  }
}
