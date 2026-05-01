using UnityEngine;
using Features.Player.Data;
using Features.PowerUps.Scripts;
using Core;

namespace Features.Player.Scripts
{
    public class PlayerCollision : MonoBehaviour
    {
        [SerializeField] private PlayerStats _stats;
        [Header("Audio")]
        [SerializeField] private AudioClip deathClip;

        private bool _isGrounded;

        public bool IsInvulnerable { get; set; } = false;

        private void Awake()
        {
        }

        private void OnEnable()
        {
            GameEvents.OnPlayerDeath += HandleDeath;
            GameEvents.OnGameReset += HandleGameReset;
        }

        private void OnDisable()
        {
            GameEvents.OnPlayerDeath -= HandleDeath;
            GameEvents.OnGameReset -= HandleGameReset;
        }

        private void HandleGameReset()
        {
            // Unparent player on game reset to avoid moving platform offset issues.
            transform.SetParent(null);
            transform.localScale = Vector3.one;
        }

        private void HandleDeath()
        {
            if (deathClip != null)
            {
                GameEvents.RaisePlaySound(deathClip, 1f);
            }
            gameObject.SetActive(false);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.TryGetComponent<IGroundable>(out var platform))
            {
                if (!_isGrounded)
                {
                    _isGrounded = true;
                    PlayerEvents.RaiseGrounded();
                    platform.OnLanded(gameObject);
                }
            }
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            if (collision.gameObject.GetComponent<IGroundable>() != null)
            {
                if (_isGrounded)
                {
                    _isGrounded = false;
                    PlayerEvents.RaiseAirborne();
                }
            }
        }

        private void OnTriggerEnter2D(Collider2D collider)
        {
            if (collider.TryGetComponent<IPowerUp>(out var powerUp))
            {
                powerUp.Apply(gameObject);
                return;
            }


        }
    }
}
