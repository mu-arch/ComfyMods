using ComfyLib;

using UnityEngine;

namespace Enhuddlement {
  public class EnemyHudPanel : MonoBehaviour {
    Character _character;
    BaseAI _baseAI;

    void Awake() {
      _character = GetComponent<Character>();
      _baseAI = _character.Ref()?.GetComponent<BaseAI>();
    }

    bool ShouldShowHud(EnemyHud enemyHud) {
      float distance = Vector3.Distance(_character.transform.position, enemyHud.m_refPoint);

      if (_character.m_boss && distance < enemyHud.m_maxShowDistanceBoss) {
        return true;
      } else if (distance < enemyHud.m_maxShowDistance) {
        return !(_character.IsPlayer() && _character.IsCrouching());
      }

      return false;
    }
  }
}
