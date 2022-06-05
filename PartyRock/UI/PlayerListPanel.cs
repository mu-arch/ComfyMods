using System.Collections;

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

    public PlayerSlot CreatePlayerSlot(string playerName) {
      GameObject column = CreateColumn(Content.transform);
      column.SetName("Player.Slot");

      GameObject nameLabel = CreateLabel(column.transform);
      nameLabel.AddComponent<LayoutElement>().SetFlexible(width: 1f);
      nameLabel.Text().SetText(playerName);

      GameObject healthRow = CreateRow(column.transform);
      healthRow.SetName("Health.Row");

      healthRow.GetComponent<HorizontalLayoutGroup>()
          .SetPadding(left: 10, right: 10)
          .SetSpacing(0f);

      GameObject hpLabel = CreateLabel(healthRow.transform);
      hpLabel.SetName("Health.Label");
      hpLabel.AddComponent<LayoutElement>().SetPreferred(width: 25f);
      hpLabel.Text().SetText("\u2661");

      GameObject hpBarBackground = CreateRow(healthRow.transform);
      hpBarBackground.SetName("Health.Bar.Background");

      hpBarBackground.GetComponent<HorizontalLayoutGroup>()
          .SetPadding(2, 2, 2, 2);

      hpBarBackground.AddComponent<Image>()
          .SetColor(new Color(0f, 0f, 0f, 0.4f));

      hpBarBackground.AddComponent<LayoutElement>()
          .SetFlexible(width: 1f);

      GameObject hpBar = CreateRow(hpBarBackground.transform);
      hpBar.SetName("Health.Bar");

      hpBar.GetComponent<HorizontalLayoutGroup>()
          .SetPadding(10, 10, 3, 3)
          .SetChildAlignment(TextAnchor.MiddleLeft)
          .SetSpacing(10f);

      Image hpBarImage = hpBar.AddComponent<Image>();
      hpBarImage.color = new Color(0f, 0.6f, 0f, 0.95f);
      hpBarImage.type = Image.Type.Filled;
      hpBarImage.fillMethod = Image.FillMethod.Horizontal;
      hpBarImage.fillOrigin = (int) Image.OriginHorizontal.Left;
      hpBarImage.fillAmount = 1f;
      hpBarImage.sprite = CreateGradientSprite();

      hpBar.AddComponent<LayoutElement>()
          .SetPreferred(width: 150f);

      GameObject hpBarHealthLabel = CreateLabel(hpBar.transform);
      hpBarHealthLabel.SetName("Health.Bar.CurrentHealth");
      hpBarHealthLabel.AddComponent<LayoutElement>().SetFlexible(width: 1f);
      hpBarHealthLabel.Text()
          .SetFontSize(hpBarHealthLabel.Text().fontSize - 1)
          .SetAlignment(TextAnchor.MiddleRight)
          .SetText("50");

      GameObject hpBarDividerLabel = CreateLabel(hpBar.transform);
      hpBarDividerLabel.SetName("Health.Bar.Divider");
      hpBarDividerLabel.Text()
          .SetFontSize(hpBarDividerLabel.Text().fontSize - 1)
          .SetAlignment(TextAnchor.MiddleCenter)
          .SetText("<i>/</i>");

      GameObject hpBarMaxHealthLabel = CreateLabel(hpBar.transform);
      hpBarMaxHealthLabel.SetName("Health.Bar.MaxHealth");
      hpBarMaxHealthLabel.AddComponent<LayoutElement>().SetFlexible(width: 1f);
      hpBarMaxHealthLabel.Text()
          .SetFontSize(hpBarMaxHealthLabel.Text().fontSize - 1)
          .SetAlignment(TextAnchor.MiddleLeft)
          .SetText("50");

      return new(hpBarImage, hpBarHealthLabel.Text(), hpBarMaxHealthLabel.Text());
    }

    public class PlayerSlot {
      readonly Image _hpBarImage;
      readonly Text _hpBarHealthText;
      readonly Text _hpBarMaxHealthText;

      float _health = 0f;
      float _maxHealth = 0f;

      Coroutine _fillCoroutine;

      public PlayerSlot(Image hpBarImage, Text hhpBarHealthText, Text hpBarMaxHealthText) {
        _hpBarImage = hpBarImage;
        _hpBarHealthText = hhpBarHealthText;
        _hpBarMaxHealthText = hpBarMaxHealthText;
      }

      IEnumerator LerpFillAmountCoroutine(float endValue) {
        float timeElapsed = 0f;
        float startValue = _hpBarImage.fillAmount;
        ZLog.Log($"Starting fill: {startValue} to {endValue}");
        while (timeElapsed < 2f) {
          _hpBarImage.fillAmount = Mathf.Lerp(startValue, endValue, timeElapsed);
          _hpBarHealthText.text = $"<i>{_maxHealth * _hpBarImage.fillAmount:0}</i>";
          timeElapsed += Time.deltaTime;
          yield return null;
        }

        ZLog.Log("done");
        _hpBarHealthText.text = $"<i>{_health:0}</i>";
        _hpBarImage.fillAmount = endValue;
      }

      public PlayerSlot SetHealthValues(float health, float maxHealth) {
        if (_health == health && _maxHealth == maxHealth) {
          return this;
        }

        _health = health;
        _hpBarHealthText.text = $"<i>{health:0}</i>";

        _maxHealth = maxHealth;
        _hpBarMaxHealthText.text = $"<i>{maxHealth:0}</i>";

        float amount = health / maxHealth;

        if (_hpBarImage.fillAmount == amount) {
          return this;
        }

        if (_fillCoroutine != null) {
          Game.m_instance.StopCoroutine(_fillCoroutine);
        }

        _fillCoroutine = Game.m_instance.StartCoroutine(LerpFillAmountCoroutine(Mathf.Clamp01(amount)));

        return this;
      }
    }
  }
}
