using Features.VFX.Data;
using UnityEngine;

namespace Features.Player.Scripts
{
    /// <summary>Handles state-based player animations via code to avoid transition overhead.</summary>
    public class PlayerAnimation : MonoBehaviour
    {
        [SerializeField] private Animator _animator;
        [SerializeField] private Vector3 _vfxOffset = new Vector3(0f, -0.5f, 0f);
        private Rigidbody2D _rb;

        // Cache animation hashes for zero-allocation performance.
        private static readonly int IdleStateHash = Animator.StringToHash("Idle");
        private static readonly int JumpStateHash = Animator.StringToHash("Jump");
        private static readonly int FallStateHash = Animator.StringToHash("Fall");

        // Track current state to prevent redundant Play calls.
        private int _currentState;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
        }

        private void Update()
        {
            UpdateAnimationState();
        }

        /// <summary>Updates animator state based on physics.</summary>
        private void UpdateAnimationState()
        {
            int newStateHash = DetermineStateHash();

            // Only trigger transition if entering a new state.
            if (newStateHash != _currentState)
            {
                if (_currentState == IdleStateHash && newStateHash == JumpStateHash)
                {
                    Vector3 feetPosition = transform.position + _vfxOffset;
                    PlayerEvents.RaiseImpactRequested(ImpactType.Jump, feetPosition, Quaternion.identity);
                }
                else if (_currentState == FallStateHash && newStateHash == IdleStateHash)
                {
                    Vector3 feetPosition = transform.position + _vfxOffset;
                    PlayerEvents.RaiseImpactRequested(ImpactType.Landing, feetPosition, Quaternion.identity);
                }

                _animator.Play(newStateHash);
                _currentState = newStateHash;
            }
        }

        /// <summary>Determines appropriate state hash based on vertical velocity.</summary>
        private int DetermineStateHash()
        {
            if (_rb.linearVelocity.y > 0.1f)
            {
                return JumpStateHash;
            }

            if (_rb.linearVelocity.y < -0.1f)
            {
                return FallStateHash;
            }

            return IdleStateHash;
        }
    }
}
