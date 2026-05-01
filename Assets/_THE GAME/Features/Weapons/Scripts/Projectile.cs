using System.Collections;
using UnityEngine;
using Core;
using Features.Weapons.Data;

namespace Features.Weapons.Scripts
{
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class Projectile : MonoBehaviour
    {
        private ProjectileData _data;
        private Rigidbody2D _rb;
        private Collider2D _col;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _col = GetComponent<Collider2D>();

            // Collision Detection Fix: Prevent tunneling through thin colliders at high speed
            _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }

        public void Initialize(ProjectileData data, Vector2 direction, Collider2D playerCollider)
        {
            _data = data;

            // Ghost Collision Fix: Explicitly ignore player collider
            if (playerCollider != null && _col != null)
            {
                Physics2D.IgnoreCollision(_col, playerCollider);
            }

            // Optional Speed Decay: Applying linear drag from the data scriptable object
            // (Use _rb.linearDamping instead of drag if you are on Unity 6 strict mode)
            _rb.linearDamping = _data.linearDrag;

            // Velocity Consistency: Set velocity exactly once
            _rb.linearVelocity = direction * _data.speed;

            // Validation: Ensure clean Coroutine state upon pulling from pool
            StopAllCoroutines();
            StartCoroutine(LifetimeRoutine());
        }

        private IEnumerator LifetimeRoutine()
        {
            yield return new WaitForSeconds(_data.lifetime);
            ReturnToPool();
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            // Bitwise layer check
            if (((1 << collision.gameObject.layer) & _data.hittableLayers) != 0)
            {
                // Decoupled Visual Communication: 
                // We ask the Event System to spawn an impact effect. We do not spawn it ourselves.
                Features.Player.Scripts.PlayerEvents.RaiseImpactRequested(
                    Features.VFX.Data.ImpactType.DefaultBullet,
                    transform.position,
                    Quaternion.identity
                );

                ReturnToPool();
            }
        }

        private void ReturnToPool()
        {
            _rb.linearVelocity = Vector2.zero; // Clean reset
            ObjectPoolManager.Instance.ReturnToPool(gameObject);
        }
    }
}
