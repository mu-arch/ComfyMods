using System.Collections;

using UnityEngine;

namespace ComfyLib {
  public class PanelFader : MonoBehaviour {
    CanvasGroup _canvasGroup;
    Coroutine _showOrHideCoroutine;

    void Awake() {
      _canvasGroup = GetComponent<CanvasGroup>();
    }

    public void Show(float fadeDuration = 0f) {
      gameObject.SetActive(true);
      FadePanel(1f, fadeDuration);
    }

    public void Hide(float fadeDuration = 0f) {
      FadePanel(0f, fadeDuration);
    }

    public void FadePanel(float targetAlpha, float fadeDuration = 0f) {
      if (_showOrHideCoroutine != null) {
        StopCoroutine(_showOrHideCoroutine);
      }

      if (fadeDuration > 0f && gameObject.activeSelf) {
        _showOrHideCoroutine = StartCoroutine(LerpCanvasGroupAlpha(targetAlpha, fadeDuration));
      } else {
        _showOrHideCoroutine = default;
        _canvasGroup.alpha = targetAlpha;
        gameObject.SetActive(targetAlpha > 0f);
      }
    }

    IEnumerator LerpCanvasGroupAlpha(float targetAlpha, float lerpDuration) {
      float timeElapsed = 0f;
      float sourceAlpha = _canvasGroup.alpha;

      while (timeElapsed < lerpDuration) {
        _canvasGroup.alpha = Mathf.Lerp(sourceAlpha, targetAlpha, (timeElapsed / lerpDuration));
        timeElapsed += Time.deltaTime;

        yield return null;
      }

      _canvasGroup.alpha = targetAlpha;
      gameObject.SetActive(targetAlpha > 0f);
    }
  }
}
