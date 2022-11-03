using System.Collections.Generic;

using BepInEx.Configuration;

namespace ComfyLib {
  public static class ConfigFileExtensions {
    static readonly Dictionary<string, int> SectionToOrderCache = new();

    static int GetSettingOrder(string section) {
      if (!SectionToOrderCache.TryGetValue(section, out int order)) {
        order = 0;
      }

      SectionToOrderCache[section] = order - 1;
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
        AcceptableValueBase acceptableValues) {
      return config.Bind(
          section,
          key,
          defaultValue,
          new ConfigDescription(
              description, acceptableValues, new ConfigurationManagerAttributes { Order = GetSettingOrder(section) }));
    }

    public static StringListConfigEntry BindInOrder(
        this ConfigFile config, string section, string key, string description, string valuesSeparator) {
      return new StringListConfigEntry(config, section, key, description, valuesSeparator, GetSettingOrder(section));
    }

    internal sealed class ConfigurationManagerAttributes {
      public int? Order;
    }
  }
}
