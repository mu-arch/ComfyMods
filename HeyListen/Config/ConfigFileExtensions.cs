﻿using System.Collections.Generic;

using BepInEx.Configuration;

using UnityEngine;

namespace ComfyLib {
  public static class ConfigFileExtensions {
    static readonly Dictionary<string, int> _sectionToSettingOrder = new();

    static int GetSettingOrder(string section) {
      if (!_sectionToSettingOrder.TryGetValue(section, out int order)) {
        order = 0;
      }

      _sectionToSettingOrder[section] = order - 1;
      return order;
    }

    public static ConfigEntry<T> BindInOrder<T>(
        this ConfigFile config,
        string section,
        string key,
        T defaultValue,
        string description) {
      return config.Bind(
          section,
          key,
          defaultValue,
          new ConfigDescription(
              description, null, new ConfigurationManagerAttributes { Order = GetSettingOrder(section) }));
    }

    public static ConfigEntry<T> BindInOrder<T>(
        this ConfigFile config,
        string section,
        string key,
        T defaultValue,
        string description,
        System.Action<ConfigEntryBase> customDrawer,
        bool hideDefaultButton = false) {
      return config.Bind(
          section,
          key,
          defaultValue,
          new ConfigDescription(
              description,
              null,
              new ConfigurationManagerAttributes {
                CustomDrawer = customDrawer,
                HideDefaultButton = hideDefaultButton,
                Order = GetSettingOrder(section)
              }));
    }

    public static ConfigEntry<T> BindInOrder<T>(
        this ConfigFile config,
        string section,
        string key,
        T defaultValue,
        string description,
        AcceptableValueBase acceptableValues) {
      return config.Bind(
          section,
          key,
          defaultValue,
          new ConfigDescription(
              description, acceptableValues, new ConfigurationManagerAttributes { Order = GetSettingOrder(section) }));
    }

    public static ConfigEntry<Color> BindColorInOrder(
        this ConfigFile config,
        string section,
        string key,
        Color defaultValue,
        string description) {
      return config.BindInOrder(
          section,
          key,
          defaultValue,
          description,
          customDrawer: new ExtendedColorConfig().DrawColor,
          hideDefaultButton: true);
    }

    internal sealed class ConfigurationManagerAttributes {
      public System.Action<ConfigEntryBase> CustomDrawer;
      public bool? HideDefaultButton;
      public int? Order;
    }
  }
}
