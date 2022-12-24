using System;
using System.Globalization;

using BepInEx.Configuration;

using UnityEngine;

namespace ComfyLib {


  public class ExtendedColorSetting {
    public ExtendedColorSetting() {
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

      GUIHelper.BeginColor(configColor);
      GUILayout.Label(string.Empty, GUILayout.ExpandWidth(true));
      GUI.DrawTexture(GUILayoutUtility.GetLastRect(), _colorTexture);
      GUIHelper.EndColor();

      if (GUILayout.Button(
            _showSliders ? "\u25BC" : "\u25C4", GUILayout.MinWidth(50f), GUILayout.ExpandWidth(false))) {
        _showSliders = !_showSliders;
      }

      GUILayout.EndHorizontal();

      if (_showSliders) {
        GUILayout.Space(5f);
        GUILayout.BeginHorizontal();

        _redInput.Draw();
        _greenInput.Draw();
        _blueInput.Draw();
        _alphaInput.Draw();

        GUILayout.EndHorizontal();
      }

      GUILayout.EndVertical();

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

    static readonly Lazy<GUIStyle> _centeredLabelStyle =
        new(() => new(GUI.skin.label) { alignment = TextAnchor.MiddleCenter });

    public void Draw() {
      GUILayout.BeginVertical();

      GUILayout.BeginHorizontal();
      GUILayout.Label(FieldLabel, _centeredLabelStyle.Value, GUILayout.ExpandWidth(false));

      GUIHelper.BeginColor(_currentColor);
      string textValue = GUILayout.TextField(CurrentText, GUILayout.MinWidth(50f), GUILayout.ExpandWidth(true));
      GUIHelper.EndColor();

      GUILayout.EndHorizontal();

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
      string textValue = GUILayout.TextField(CurrentText, GUILayout.MinWidth(100f), GUILayout.ExpandWidth(false));
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
}
