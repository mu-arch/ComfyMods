using UnityEngine;

namespace Dramamist {
  public class FadeOutParticleMist : MonoBehaviour {
    Demister _demister;
    Collider _collider;

    void Awake() {
      _demister = GetComponentInChildren<Demister>();

      _collider =
          _demister.m_forceField.GetOrAddComponent<SphereCollider>()
              .SetRadius(_demister.m_forceField.endRange)
              .SetIsTrigger(true);          
    }

    void OnEnable() {
      _collider.SetEnabled(true);

      ParticleSystem.TriggerModule trigger = ParticleMist.m_instance.m_ps.trigger;
      trigger.AddCollider(_collider);
    }

    void OnDisable() {
      _collider.SetEnabled(false);

      ParticleSystem.TriggerModule trigger = ParticleMist.m_instance.m_ps.trigger;
      trigger.RemoveCollider(_collider);
    }
  }
}
