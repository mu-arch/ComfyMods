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

    GameObject CreateCardCost(Transform parentTransform) {
      GameObject cost = new("Card.Cost", typeof(RectTransform));
      cost.SetParent(parentTransform);

      cost.AddComponent<LayoutElement>()
          .ignoreLayout = true;

      cost.RectTransform()
          .SetAnchorMin(new(0f, 1f))
          .SetAnchorMax(new(0f, 1f))
          .SetPivot(new(0f, 1f))
          .SetPosition(new(-20f, 15f))
          .SetSizeDelta(new(40f, 40f));

      cost.AddComponent<Image>()
          .SetSprite(CreateRoundedCornerSprite(100, 100, 20))
          .SetType(Image.Type.Sliced)
          .SetColor(new(0.509f, 0.435f, 0.400f, 1f));

      GameObject label = new("Card.Cost.Label", typeof(RectTransform));
      label.SetParent(cost.transform);

      label.RectTransform()
          .SetAnchorMin(Vector2.zero)
          .SetAnchorMax(Vector2.one)
          .SetPivot(new(0.5f, 0.5f))
          .SetSizeDelta(new(40, 40));

      label.AddComponent<Text>()
          .SetSupportRichText(true)
          .SetFont(UIResources.AveriaSerifLibre)
          .SetFontSize(26)
          .SetAlignment(TextAnchor.MiddleCenter)
          .SetColor(Color.white)
          .SetText("3");

      label.AddComponent<Outline>()
          .SetEffectColor(Color.black);

      return cost;
    }

    GameObject CreateCardType(Transform parentTransform) {
      GameObject cardType = new("Card.Type", typeof(RectTransform));
      cardType.SetParent(parentTransform);

      cardType.AddComponent<LayoutElement>()
          .ignoreLayout = true;

      cardType.RectTransform()
          .SetAnchorMin(new(0.5f, 0f))
          .SetAnchorMax(new(0.5f, 0f))
          .SetPivot(new(0.5f, 0f))
          .SetPosition(new(0f, -15f))
          .SetSizeDelta(new(80f, 35f));

      cardType.AddComponent<Image>()
          .SetSprite(CreateRoundedCornerSprite(160, 70, 20))
          .SetType(Image.Type.Sliced)
          .SetColor(new(0.509f, 0.435f, 0.400f, 1f));

      GameObject label = new("Card.Type.Label", typeof(RectTransform));
      label.SetParent(cardType.transform);

      label.RectTransform()
          .SetAnchorMin(Vector2.zero)
          .SetAnchorMax(Vector2.one)
          .SetPivot(new(0.5f, 0.5f))
          .SetSizeDelta(new(80f, 35f));

      label.AddComponent<Text>()
          .SetSupportRichText(true)
          .SetFont(UIResources.AveriaSerifLibre)
          .SetFontSize(18)
          .SetAlignment(TextAnchor.MiddleCenter)
          .SetColor(Color.white)
          .SetText("Power");

      label.AddComponent<Outline>()
          .SetEffectColor(Color.black);

      return cardType;
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
          .showMaskGraphic = false;

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
  }
}
