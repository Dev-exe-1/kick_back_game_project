using UnityEngine;
using Core;

namespace Features.VFX.Scripts
{
    /// <summary>Returns particle systems to the pool after simulation stops.</summary>
    [RequireComponent(typeof(ParticleSystem))]
    public class PooledParticle : MonoBehaviour
    {
        private ParticleSystem _particleSystem;

        private void Awake()
        {
            _particleSystem = GetComponent<ParticleSystem>();

            // Enforce Stop Action to Callback to guarantee OnParticleSystemStopped triggers.
            var main = _particleSystem.main;
            main.stopAction = ParticleSystemStopAction.Callback;
        }

        private void OnEnable()
        {
            if (_particleSystem != null)
            {
                _particleSystem.Play(true);
            }
        }

        private void OnParticleSystemStopped()
        {
            if (ObjectPoolManager.Instance != null)
            {
                ObjectPoolManager.Instance.ReturnToPool(gameObject);
            }
            else
            {
                // Fallback disable to prevent dangling objects if pool manager is destroyed.
                gameObject.SetActive(false);
            }
        }
    }
}
