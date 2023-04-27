using BepInEx.Configuration;

using UnityEngine;

namespace PartyRock {
  public class PluginConfig {
    public static ConfigEntry<bool> IsModEnabled { get; private set; }

    public static ConfigEntry<Vector2> CardPanelSizeDelta { get; private set; }
    public static ConfigEntry<int> CardPanelBorderRadius { get; private set; }
    public static ConfigEntry<int> CardBorderRadius { get; private set; }
    public static ConfigEntry<string> CardName { get; private set; }
    public static ConfigEntry<string> CardDescription { get; private set; }
    public static ConfigEntry<string> CardGraphicSpriteName { get; private set; }

    public static ConfigEntry<Color> CardBorderColor { get; private set; }
    public static ConfigEntry<Color> CardMaskColor { get; private set; }

    public static ConfigEntry<int> CardCostBorderRadius { get; private set; }
    public static ConfigEntry<int> CardCostMaskRadius { get; private set; }
    public static ConfigEntry<string> CardCostLabelText { get; private set; }

    public static ConfigEntry<int> CardTypeBorderRadius { get; private set; }
    public static ConfigEntry<int> CardTypeMaskRadius { get; private set; }
    public static ConfigEntry<string> CardTypeLabelText { get; private set; }

    public static ConfigEntry<Vector2> CardHandPosition { get; private set; }
    public static ConfigEntry<int> CardHandCount { get; private set; }
    public static ConfigEntry<float> CardHandCardSpacing { get; private set; }
    public static ConfigEntry<float> CardHandCardTwist { get; private set; }
    public static ConfigEntry<float> CardHandCardNudge { get; private set; }

    public static void BindConfig(ConfigFile config) {
      IsModEnabled = config.Bind("_Global", "isModEnabled", true, "Globally enable or disable this mod.");

      CardPanelSizeDelta = config.Bind("Card", "cardPanelSizeDelta", new Vector2(250f, 360f), "Card panel size.");
      CardPanelBorderRadius = config.Bind("Card", "cardPanelBorderRadius", 30, "Card panel border radius.");
      CardBorderRadius = config.Bind("Card", "cardBorderRadius", 20, "Card border radius?");
      CardName = config.Bind("Card", "cardName", "Attack", "Card.Name text.");
      CardDescription = config.Bind("Card", "cardDescription", "Deal 8 damage.", "Card.Description text.");
      CardGraphicSpriteName = config.Bind("Card", "cardGraphicSpriteName", "axe_stone", "Card.Graphic sprite name.");

      CardCostBorderRadius = config.Bind("Card", "cardCostBorderRadius", 16, "Card.Cost.Border radius.");
      CardCostMaskRadius = config.Bind("Card", "cardCostMaskRadius", 13, "Card.Cost.Mask radius.");
      CardCostLabelText = config.Bind("Card", "cardCostLabelText", "3", "Card.Cost.Label text.");

      CardTypeBorderRadius = config.Bind("Card", "cardTypeBorderRadius", 16, "Card.Type.Border radius.");
      CardTypeMaskRadius = config.Bind("Card", "cardTypeMaskRadius", 13, "Card.Type.Mask radius.");
      CardTypeLabelText = config.Bind("Card", "cardTypeLabelText", "Attack", "Card.Type.Label text.");

      CardHandPosition = config.Bind("CardHand", "cardHandPosition", new Vector2(225f, 0f), "CardHand position.");
      CardHandCount = config.Bind("CardHand", "cardHandCount", 5, "CardHand count.");
      CardHandCardSpacing = config.Bind("CardHand", "cardHandCardSpacing", 100f, "CardHand.Card spacing.");
      CardHandCardTwist = config.Bind("CardHand", "cardHandCardTwist", 6f, "CardHand.Card twist.");
      CardHandCardNudge = config.Bind("CardHand", "cardHandCardNudge", 6f, "CardHand.Card nudge.");
    }
  }
}
