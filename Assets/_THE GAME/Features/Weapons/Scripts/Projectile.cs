using System.Collections;
using UnityEngine;
using Core;
using Features.Weapons.Data;

namespace Features.Weapons.Scripts
{
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class Projectile : MonoBehaviour
    {
        [SerializeField] private AudioClip shootClip;
        private ProjectileData _data;
        private Rigidbody2D _rb;
        private Collider2D _col;


        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _col = GetComponent<Collider2D>();

            // Use continuous collision to prevent tunneling at high speed.
            _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }

        public void Initialize(ProjectileData data, Vector2 direction, Collider2D playerCollider)
        {
            _data = data;

            // Explicitly ignore player collider to prevent self-collision.
            if (playerCollider != null && _col != null)
            {
                Physics2D.IgnoreCollision(_col, playerCollider);
            }

            // Apply linear damping for optional speed decay.
            _rb.linearDamping = _data.linearDrag;

            _rb.linearVelocity = direction * _data.speed;

            // Ensure clean coroutine state before starting lifetime routine.
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
            if (((1 << collision.gameObject.layer) & _data.hittableLayers) != 0)
            {
                if (shootClip != null)
                {
                    GameEvents.RaisePlaySound(shootClip, 0.8f);
                }
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
            _rb.linearVelocity = Vector2.zero;
            ObjectPoolManager.Instance.ReturnToPool(gameObject);
        }
    }
}
