using BepInEx.Configuration;

namespace PartyRock {
  public class PluginConfig {
    public static ConfigEntry<bool> IsModEnabled { get; private set; }

    public static ConfigEntry<int> CardPanelBorderRadius { get; private set; }
    public static ConfigEntry<int> CardBorderRadius { get; private set; }
    public static ConfigEntry<string> CardName { get; private set; }
    public static ConfigEntry<string> CardDescription { get; private set; }
    public static ConfigEntry<string> CardGraphicSpriteName { get; private set; }

    public static void BindConfig(ConfigFile config) {
      IsModEnabled = config.Bind("_Global", "isModEnabled", true, "Globally enable or disable this mod.");

      CardPanelBorderRadius = config.Bind("Card", "cardPanelBorderRadius", 30, "Card panel border radius.");
      CardBorderRadius = config.Bind("Card", "cardBorderRadius", 20, "Card border radius?");
      CardName = config.Bind("Card", "cardName", "Attack", "Card.Name text.");
      CardDescription = config.Bind("Card", "cardDescription", "Deal 8 damage.", "Card.Description text.");
      CardGraphicSpriteName = config.Bind("Card", "cardGraphicSpriteName", "axe_stone", "Card.Graphic sprite name.");
    }
  }
}
