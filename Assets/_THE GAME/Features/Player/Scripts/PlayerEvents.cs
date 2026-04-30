using System;
using UnityEngine;
using Features.VFX.Data;

namespace Features.Player.Scripts
{
    public static class PlayerEvents
    {
        public static event Action<Vector2> OnAim;
        public static void RaiseAim(Vector2 aim)
        {
            OnAim?.Invoke(aim);
        }

        public static event Action OnTryShoot;
        public static void RaiseTryShoot()
        {
            OnTryShoot?.Invoke();
        }

        public static event Action<Vector2> OnRecoilApplied;
        public static void RaiseRecoilApplied(Vector2 force)
        {
            OnRecoilApplied?.Invoke(force);
        }

        public static event Action OnGrounded;
        public static void RaiseGrounded()
        {
            OnGrounded?.Invoke();
        }

        public static event Action OnAirborne;
        public static void RaiseAirborne()
        {
            OnAirborne?.Invoke();
        }

        public static event Action<ImpactType, Vector3, Quaternion> OnImpactRequested;
        public static void RaiseImpactRequested(ImpactType impactType, Vector3 position, Quaternion rotation)
        {
            OnImpactRequested?.Invoke(impactType, position, rotation);
        }
    }
}
