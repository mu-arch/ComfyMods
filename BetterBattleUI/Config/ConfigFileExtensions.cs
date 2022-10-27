using System.Collections.Generic;

using BepInEx.Configuration;

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
        System.Action<ConfigEntryBase> customDrawer) {
      return config.Bind(
          section,
          key,
          defaultValue,
          new ConfigDescription(
              description,
              null,
              new ConfigurationManagerAttributes { CustomDrawer = customDrawer, Order = GetSettingOrder(section) }));
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

    internal sealed class ConfigurationManagerAttributes {
      public System.Action<ConfigEntryBase> CustomDrawer;
      public int? Order;
    }
  }
}
