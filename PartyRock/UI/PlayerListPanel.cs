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
