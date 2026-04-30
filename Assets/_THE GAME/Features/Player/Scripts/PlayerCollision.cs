using UnityEngine;
using Features.Player.Data;
using Features.PowerUps.Scripts;
using Core;

namespace Features.Player.Scripts
{
    public class PlayerCollision : MonoBehaviour
    {
        [SerializeField] private PlayerStats _stats;
        private bool _isGrounded;

        private void Awake()
        {
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
