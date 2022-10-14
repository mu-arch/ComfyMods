using System.Collections.Generic;
using System.Linq;

using BepInEx.Configuration;

using ComfyLib;

using UnityEngine;

namespace LicensePlate {
  public static class PluginConfig {
    public static ConfigEntry<bool> IsModEnabled { get; private set; }

    public static void BindConfig(ConfigFile config) {
      IsModEnabled = config.BindInOrder("_Global", "isModEnabled", true, "Globally enable or disable this mod.");

      BindShipNameConfig(config);
      BindCartNameConfig(config);
    }

    public static ConfigEntry<bool> ShowShipNames { get; private set; }
    public static ConfigEntry<float> ShipNameCutoffDistance { get; private set; }
    public static ConfigEntry<Vector3> ShipNameDisplayOffset { get; private set; }
    public static ConfigEntry<int> ShipNameFontSize { get; private set; }
    public static ConfigEntry<bool> ShipNameStripHtmlTags { get; private set; }

    private static void BindShipNameConfig(ConfigFile config) {
      ShowShipNames = config.BindInOrder("ShipName", "showShipNames", true, "Show custom names over ships.");

      ShipNameCutoffDistance =
          config.BindInOrder(
              "ShipName",
              "shipNameCutoffDistance",
              25f,
              "Cutoff distance for custom ship names to appear/disappear.",
              new AcceptableValueRange<float>(1f, 40f));

      ShipNameDisplayOffset =
          config.BindInOrder("ShipName", "shipNameDisplayOffset", Vector3.up, "Display offset for custom ship names.");

      ShipNameFontSize =
          config.BindInOrder(
              "ShipName",
              "shipNameFontSize",
              20,
              "Font size for custom ship names.",
              new AcceptableValueRange<int>(2, 64));

      ShipNameStripHtmlTags =
          config.BindInOrder(
              "ShipName",
              "shipNameStripHtmlTags",
              false,
              "If true, html tags will be stripped from custom ship names when they are displayed.");
    }

    public static ConfigEntry<bool> ShowCartNames { get; private set; }
    public static ConfigEntry<float> CartNameCutoffDistance { get; private set; }
    public static ConfigEntry<Vector3> CartNameDisplayOffset { get; private set; }
    public static ConfigEntry<int> CartNameFontSize { get; private set; }
    public static ConfigEntry<bool> CartNameStripHtmlTags { get; private set; }

    private static void BindCartNameConfig(ConfigFile config) {
      ShowCartNames = config.BindInOrder("CartName", "showCartNames", true, "Show custom names over carts.");

      CartNameCutoffDistance =
          config.BindInOrder(
              "CartName",
              "cartNameCutoffDistance",
              10f,
              "Cutoff distance for custom cart names to appear/disappear.",
              new AcceptableValueRange<float>(1f, 25f));

      CartNameDisplayOffset =
          config.BindInOrder("CartName", "cartNameDisplayOffset", Vector3.up, "Display offset for custom cart names.");

      CartNameFontSize =
          config.BindInOrder(
              "CartName",
              "cartNameFontSize",
              20,
              "Font size for custom cart names.",
              new AcceptableValueRange<int>(2, 64));

      CartNameStripHtmlTags =
          config.BindInOrder(
              "CartName",
              "cartNameStripHtmlTags",
              false,
              "If true, html tags will be stripped from custom cart names when they are displayed.");
    }
  }
}
