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

      // Health
      ProgressBar healthBar = new(healthRow.transform);

      // Stamina

      GameObject staminaRow = CreateRow(column.transform);
      staminaRow.SetName("Stamina.Row");

      staminaRow.GetComponent<HorizontalLayoutGroup>()
          .SetPadding(left: 10, right: 10)
          .SetSpacing(0f);

      GameObject staminaLabel = CreateLabel(staminaRow.transform);
      staminaLabel.SetName("Stamina.Label");
      staminaLabel.AddComponent<LayoutElement>().SetPreferred(width: 25f);
      staminaLabel.Text().SetFontSize(UIResources.AveriaSerifLibre.fontSize - 2).SetText("\u29bf");

      ProgressBar staminaBar = new(staminaRow.transform);
      staminaBar.Bar.Image().SetColor(new(0.95f, 0.76f, 0.05f, 0.95f));

      return new(healthBar.Bar.Image(), healthBar.CurrentValue.Text(), healthBar.MaxValue.Text());
    }

    public class PlayerSlot {
      readonly Image _hpBarImage;
      readonly Text _hpBarHealthText;
      readonly Text _hpBarMaxHealthText;

      float _health = 0f;
      float _maxHealth = 0f;

      Coroutine _hpFillCoroutine;

      public PlayerSlot(Image hpBarImage, Text hhpBarHealthText, Text hpBarMaxHealthText) {
        _hpBarImage = hpBarImage;
        _hpBarHealthText = hhpBarHealthText;
        _hpBarMaxHealthText = hpBarMaxHealthText;
      }

      IEnumerator LerpFillAmountCoroutine(float endValue) {
        float timeElapsed = 0f;
        float startValue = _hpBarImage.fillAmount;

        while (timeElapsed < 2f) {
          _hpBarImage.fillAmount = Mathf.Lerp(startValue, endValue, timeElapsed);
          _hpBarHealthText.text = $"<i>{_maxHealth * _hpBarImage.fillAmount:0}</i>";
          timeElapsed += Time.deltaTime;
          yield return null;
        }

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

        if (_hpFillCoroutine != null) {
          Game.m_instance.StopCoroutine(_hpFillCoroutine);
        }

        _hpFillCoroutine = Game.m_instance.StartCoroutine(LerpFillAmountCoroutine(Mathf.Clamp01(amount)));

        return this;
      }
    }
  }
}
