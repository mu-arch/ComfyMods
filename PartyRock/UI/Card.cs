using System;

using UnityEngine;
using UnityEngine.UI;

using static PartyRock.PluginConfig;
using static PartyRock.UIBuilder;

namespace PartyRock {
  public class Card {
    public GameObject Panel { get; }
    public GameObject Mask { get; }
    public GameObject Content { get; }
    public GameObject Name { get; }
    public GameObject Graphic { get; }
    public GameObject Description { get; }

    public GameObject Cost { get; }
    public GameObject Type { get; }

    public Card(Transform parentTransform) {
      Panel = CreateCardPanel(parentTransform);
      Mask = CreateCardMask(Panel.transform);
      Content = CreateCardContent(Mask.transform);
      Name = CreateCardName(Content.transform);
      Graphic = CreateCardGraphic(Content.transform);
      CreateCardDivider(Content.transform);
      Description = CreateCardDescription(Content.transform);

      Cost = CreateCardCost(Panel.transform);
      Type = CreateCardType(Panel.transform);
    }

    GameObject CreateCardPanel(Transform parentTransform) {
      GameObject panel = CreatePanel(parentTransform);
      panel.SetName("Card.Panel");

      panel.GetComponent<VerticalLayoutGroup>()
          .SetChildAlignment(TextAnchor.MiddleCenter)
          .SetChildForceExpand(width: false, height: false)
          .SetPadding(left: 5, right: 5, top: 5, bottom: 5);

      panel.GetComponent<Image>()
          .SetSprite(CreateRoundedCornerSprite(210, 310, CardPanelBorderRadius.Value))
          .SetType(Image.Type.Sliced)
          .SetColor(new(0.9f, 0.9f, 0.9f, 1f));

      return panel;
    }

    GameObject CreateCardMask(Transform parentTransform) {
      GameObject mask = new("Card.Mask", typeof(RectTransform));
      mask.SetParent(parentTransform);

      mask.AddComponent<HorizontalLayoutGroup>()
          .SetChildControl(width: true, height: true)
          .SetChildForceExpand(width: false, height: false);

      mask.AddComponent<Image>()
          .SetSprite(CreateRoundedCornerSprite(200, 300, CardBorderRadius.Value))
          .SetType(Image.Type.Sliced)
          .SetColor(new(0.259f, 0.133f, 0.094f, 1f));

      mask.AddComponent<Mask>()
          .SetShowMaskGraphic(false);

      mask.AddComponent<LayoutElement>()
          .SetFlexible(width: 1f, height: 1f);

      return mask;
    }

    GameObject CreateCardContent(Transform parentTransform) {
      GameObject content = CreatePanel(parentTransform);
      content.SetName("Card.Content");

      content.GetComponent<VerticalLayoutGroup>()
          .SetChildAlignment(TextAnchor.UpperCenter)
          .SetChildForceExpand(width: false, height: false)
          .SetSpacing(12f)
          .SetPadding(left: 10, right: 10, top: 10, bottom: 10);

      content.GetComponent<Image>()
          .SetSprite(CreateGradientSprite())
          .SetType(Image.Type.Filled)
          .SetColor(new(0.259f, 0.133f, 0.094f, 1f));

      return content;
    }

    GameObject CreateCardName(Transform parentTransform) {
      GameObject name = CreateLabel(parentTransform);
      name.SetName("Card.Name");

      name.GetComponent<Text>()
          .SetFontSize(24)
          .SetAlignment(TextAnchor.MiddleCenter)
          .SetText(CardName.Value);

      name.AddComponent<LayoutElement>()
          .SetFlexible(width: 1f);

      return name;
    }

    GameObject CreateCardGraphic(Transform parentTransform) {
      GameObject graphicMask = new("Card.Graphic.Mask", typeof(RectTransform));
      graphicMask.SetParent(parentTransform);

      graphicMask.AddComponent<Image>()
          .SetSprite(CreateRoundedCornerSprite(120, 120, 30))
          .SetType(Image.Type.Sliced)
          .SetColor(new(0f, 0f, 0f, 0.3f));

      graphicMask.AddComponent<Mask>()
          .SetShowMaskGraphic(true);

      graphicMask.AddComponent<GridLayoutGroup>()
          .SetCellSize(new(120f, 120f))
          .SetPadding(5, 5, 5, 5);

      GameObject graphicImage = new("Card.Graphic.Image", typeof(RectTransform));
      graphicImage.SetParent(graphicMask.transform);

      graphicImage.AddComponent<Image>()
          .SetSprite(UIResources.GetSprite(CardGraphicSpriteName.Value))
          .SetType(Image.Type.Simple)
          .SetPreserveAspect(true);

      return graphicMask;
    }

    GameObject CreateCardDivider(Transform parentTransform) {
      GameObject divider = new("Card.Divider", typeof(RectTransform));
      divider.SetParent(parentTransform);

      divider.AddComponent<Image>()
          .SetColor(Color.white);

      divider.AddComponent<LayoutElement>()
          .SetFlexible(width: 1f)
          .SetPreferred(height: 2f);

      return divider;
    }

    GameObject CreateCardDescription(Transform parentTransform) {
      GameObject description = CreateLabel(parentTransform);
      description.SetName("Card.Description");

      description.GetComponent<Text>()
          .SetFontSize(18)
          .SetAlignment(TextAnchor.UpperCenter)
          .SetText(CardDescription.Value);

      description.AddComponent<LayoutElement>()
          .SetFlexible(width: 1f, height: 1f);

      return description;
    }

    GameObject CreateCardCost(Transform parentTransform) {
      GameObject cardCostBorder = new("Card.Cost.Border", typeof(RectTransform));
      cardCostBorder.SetParent(parentTransform);

      cardCostBorder.AddComponent<LayoutElement>()
          .SetIgnoreLayout(true);

      cardCostBorder.RectTransform()
          .SetAnchorMin(new(0f, 1f))
          .SetAnchorMax(new(0f, 1f))
          .SetPivot(new(0f, 1f))
          .SetPosition(new(-5f, 5f))
          .SetSizeDelta(new(41f, 41f));

      cardCostBorder.AddComponent<Image>()
          .SetSprite(CreateRoundedCornerSprite(100, 100, CardCostBorderRadius.Value))
          .SetType(Image.Type.Sliced)
          .SetColor(new(0.9f, 0.9f, 0.9f, 1f));

      GameObject cardCostMask = new("Card.Cost.Mask", typeof(RectTransform));
      cardCostMask.SetParent(cardCostBorder.transform);

      cardCostMask.RectTransform()
          .SetAnchorMin(new(0.5f, 0.5f))
          .SetAnchorMax(new(0.5f, 0.5f))
          .SetPivot(new(0.5f, 0.5f))
          .SetPosition(Vector2.zero)
          .SetSizeDelta(new(35f, 35f));

      cardCostMask.AddComponent<Image>()
          .SetSprite(CreateRoundedCornerSprite(100, 100, CardCostMaskRadius.Value))
          .SetType(Image.Type.Sliced)
          .SetColor(new(0.259f, 0.133f, 0.094f, 1f));

      cardCostMask.AddComponent<Mask>()
          .SetShowMaskGraphic(true);

      GameObject cardCostLabel = new("Card.Cost.Label", typeof(RectTransform));
      cardCostLabel.SetParent(cardCostBorder.transform);

      cardCostLabel.RectTransform()
          .SetAnchorMin(Vector2.zero)
          .SetAnchorMax(Vector2.one)
          .SetPivot(new(0.5f, 0.5f))
          .SetSizeDelta(new(35f, 35f));

      cardCostLabel.AddComponent<Text>()
          .SetSupportRichText(true)
          .SetFont(UIResources.AveriaSerifLibre)
          .SetFontSize(26)
          .SetAlignment(TextAnchor.MiddleCenter)
          .SetColor(Color.white)
          .SetText(CardCostLabelText.Value);

      cardCostLabel.AddComponent<Outline>()
          .SetEffectColor(Color.black);

      return cardCostBorder;
    }

    GameObject CreateCardType(Transform parentTransform) {
      GameObject cardTypeBorder = new("Card.Type.Border", typeof(RectTransform));
      cardTypeBorder.SetParent(parentTransform);

      cardTypeBorder.AddComponent<LayoutElement>()
          .SetIgnoreLayout(true);

      cardTypeBorder.RectTransform()
          .SetAnchorMin(new(0.5f, 0f))
          .SetAnchorMax(new(0.5f, 0f))
          .SetPivot(new(0.5f, 0f))
          .SetPosition(new(0f, -5f))
          .SetSizeDelta(new(86f, 36f));

      cardTypeBorder.AddComponent<Image>()
          .SetSprite(CreateRoundedCornerSprite(264, 105, CardTypeBorderRadius.Value))
          .SetType(Image.Type.Sliced)
          .SetColor(new(0.9f, 0.9f, 0.9f, 1f));

      GameObject cardTypeMask = new("Card.Type.Mask", typeof(RectTransform));
      cardTypeMask.SetParent(cardTypeBorder.transform);

      cardTypeMask.RectTransform()
          .SetAnchorMin(new(0.5f, 0.5f))
          .SetAnchorMax(new(0.5f, 0.5f))
          .SetPivot(new(0.5f, 0.5f))
          .SetPosition(Vector2.zero)
          .SetSizeDelta(new(80f, 30f));

      cardTypeMask.AddComponent<Image>()
          .SetSprite(CreateRoundedCornerSprite(240, 90, CardTypeMaskRadius.Value))
          .SetType(Image.Type.Sliced)
          .SetColor(new(0.259f, 0.133f, 0.094f, 1f));

      cardTypeMask.AddComponent<Mask>()
          .SetShowMaskGraphic(true);

      GameObject cardTypeLabel = new("Card.Type.Label", typeof(RectTransform));
      cardTypeLabel.SetParent(cardTypeMask.transform);

      cardTypeLabel.RectTransform()
          .SetAnchorMin(Vector2.zero)
          .SetAnchorMax(Vector2.one)
          .SetPivot(new(0.5f, 0.5f))
          .SetSizeDelta(new(80f, 30f));

      cardTypeLabel.AddComponent<Text>()
          .SetSupportRichText(true)
          .SetFont(UIResources.AveriaSerifLibre)
          .SetFontSize(18)
          .SetAlignment(TextAnchor.MiddleCenter)
          .SetColor(Color.white)
          .SetText(CardTypeLabelText.Value);

      cardTypeLabel.AddComponent<Outline>()
          .SetEffectColor(Color.black);

      return cardTypeBorder;
    }
  }
}
