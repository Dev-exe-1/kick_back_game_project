using UnityEngine;
using Core;

namespace Features.VFX.Scripts
{
    /// <summary>
    /// A lightweight memory guard for Particle Systems.
    /// Returns the GameObject to the ObjectPoolManager once the particle simulation stops.
    /// IMPORTANT: The ParticleSystem's 'Stop Action' must be set to 'Callback' in the Unity Inspector!
    /// </summary>
    [RequireComponent(typeof(ParticleSystem))]
    public class PooledParticle : MonoBehaviour
    {
        private void OnParticleSystemStopped()
        {
            ObjectPoolManager.Instance.ReturnToPool(gameObject);
        }
    }
}
