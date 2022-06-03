using UnityEngine;
using UnityEngine.UI;

using static PartyRock.UIBuilder;

namespace PartyRock {
  public class PlayerListPanel {
    public GameObject Panel { get; private set; }
    public GameObject Viewport { get; private set; }
    public GameObject Content { get; private set; }
    public ScrollRect ScrollRect { get; private set; }

    public PlayerListPanel(Transform parentTransform) {
      Panel = CreatePanel(parentTransform);
      Viewport = CreateViewport(Panel.transform);
      Content = CreateContent(Viewport.transform);
      ScrollRect = CreateScrollRect(Panel, Viewport, Content);
    }

    public void ClearList() {
      foreach (GameObject child in Content.Children()) {
        Object.Destroy(child);
      }
    }

    public void CreatePlayerSlot(string playerName) {
      GameObject column = CreateColumn(Content.transform);
      column.SetName("Player.Slot");

      GameObject nameLabel = CreateLabel(column.transform);
      nameLabel.AddComponent<LayoutElement>().SetFlexible(width: 1f);
      nameLabel.Text().SetText(playerName);

      GameObject healthRow = CreateRow(column.transform);
      healthRow.SetName("Health.Row");

      healthRow.GetComponent<HorizontalLayoutGroup>()
          .SetPadding(left: 10)
          .SetSpacing(0f);

      GameObject hpLabel = CreateLabel(healthRow.transform);
      hpLabel.SetName("Health.Label");
      hpLabel.AddComponent<LayoutElement>().SetPreferred(width: 25f);
      hpLabel.Text().SetText("\u2661");

      GameObject hpBar = CreateRow(healthRow.transform);
      hpBar.SetName("Health.Bar");

      hpBar.GetComponent<HorizontalLayoutGroup>()
          .SetPadding(10, 2, 2, 2)
          .SetChildAlignment(TextAnchor.MiddleLeft);

      Image hpBarImage = hpBar.AddComponent<Image>();
      hpBarImage.color = new Color(0f, 0.6f, 0f, 0.95f);
      hpBarImage.type = Image.Type.Filled;
      hpBarImage.fillMethod = Image.FillMethod.Horizontal;
      hpBarImage.fillOrigin = (int) Image.OriginHorizontal.Left;
      hpBarImage.fillAmount = Random.Range(0.25f, 1f);
      hpBarImage.sprite = CreateGradientSprite();

      hpBar.AddComponent<LayoutElement>()
          .SetPreferred(width: 200f);

      hpBar.AddComponent<Outline>()
          .SetEffectColor(new Color(0f, 0f, 0f, 0.6f))
          .SetEffectDistance(new(2, -2));

      GameObject hpBarText = CreateLabel(hpBar.transform);
      hpBarText.SetName("Health.Bar.Text");
      hpBarText.AddComponent<LayoutElement>().SetFlexible(width: 1f);
      hpBarText.Text().SetText("50");
    }

    public PlayerListItem CreatePlayerListItem() {
      return new(Content.transform);
    }

    public class PlayerListItem {
      public GameObject Row { get; }
      public Text Name { get; }
      public Text Health { get; }
      public Text Stamina { get; }
      public Text Distance { get; }

      public PlayerListItem(Transform parentTransform) {
        Row = CreateRow(parentTransform);

        GameObject nameLabel = CreateLabel(Row.transform);
        nameLabel.AddComponent<LayoutElement>().SetPreferred(width: 150f);
        Name = nameLabel.Text();

        CreateSpacer(Row.transform);

        GameObject healthLabel = CreateLabel(Row.transform);
        healthLabel.AddComponent<LayoutElement>().SetPreferred(width: 50f);
        Health = healthLabel.Text().SetColor(Color.green);

        GameObject staminaLabel = CreateLabel(Row.transform);
        staminaLabel.AddComponent<LayoutElement>().SetPreferred(width: 50f);
        Stamina = staminaLabel.Text().SetColor(Color.yellow);

        GameObject distanceLabel = CreateLabel(Row.transform);
        distanceLabel.AddComponent<LayoutElement>().SetPreferred(width: 75f);
        Distance = distanceLabel.Text();
      }
    }
  }
}
