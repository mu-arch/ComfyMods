using BepInEx.Configuration;

using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace DyeHard {
  public static class PluginExtensions {
    public static T Ref<T>(this T gameObject) where T : UnityEngine.Object {
      return gameObject ? gameObject : null;
    }

    public static IEnumerable<string> AlphanumericSort(this IEnumerable<string> text) {
      return text.OrderBy(x => Regex.Replace(x, @"\d+", m => m.Value.PadLeft(50, '0')));
    }
  }

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
              description,
              acceptableValues: null,
              new ConfigurationManagerAttributes { Order = GetSettingOrder(section) }));
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
              description,
              acceptableValues,
              new ConfigurationManagerAttributes { Order = GetSettingOrder(section) }));
    }

    internal sealed class ConfigurationManagerAttributes {
      public int? Order;
    }
  }
}
