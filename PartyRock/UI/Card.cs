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

    public Card(Transform parentTransform) {
      Panel = CreateCardPanel(parentTransform);
      Mask = CreateCardMask(Panel.transform);
      Content = CreateCardContent(Mask.transform);
      Name = CreateCardName(Content.transform);
      Graphic = CreateCardGraphic(Content.transform);
      CreateCardDivider(Content.transform);
      Description = CreateCardDescription(Content.transform);
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
          //.SetSprite(CreateRoundedCornerSprite(200, 300, CardBorderRadius.Value))
          //.SetType(Image.Type.Sliced)
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
      GameObject graphic = new("Card.Graphic", typeof(RectTransform));
      graphic.SetParent(parentTransform);

      Image image = graphic.AddComponent<Image>()
          .SetSprite(UIResources.GetSprite(CardGraphicSpriteName.Value))
          .SetType(Image.Type.Simple);

      image.preserveAspect = true;

      graphic.AddComponent<LayoutElement>()
          .SetFlexible(width: 1f)
          .SetPreferred(height: 120f);

      return graphic;
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
