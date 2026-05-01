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

        // Future Shield Support
        public bool IsInvulnerable { get; set; } = false;

        private void Awake()
        {
        }

        private void OnEnable()
        {
            GameEvents.OnPlayerDeath += HandleDeath;
        }

        private void OnDisable()
        {
            GameEvents.OnPlayerDeath -= HandleDeath;
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
            // IPowerUp check
            IPowerUp powerUp = collider.GetComponent<IPowerUp>();
            if (powerUp != null)
            {
                powerUp.Apply(gameObject);
                return;
            }


        }
    }
}
