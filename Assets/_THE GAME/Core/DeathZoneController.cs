using UnityEngine;
using Features.Player.Scripts;

namespace Core
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class DeathZoneController : MonoBehaviour
    {
        [SerializeField] private Transform _cameraTransform;
        [SerializeField] private float _yOffset = -10f;

        private void Update()
        {
            if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing) return;
            if (_cameraTransform == null) return;

            float targetY = _cameraTransform.position.y + _yOffset;
            if (targetY > transform.position.y)
            {
                transform.position = new Vector3(transform.position.x, targetY, transform.position.z);
            }
        }

        // داخل DeathZoneController.cs
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.TryGetComponent<PlayerMovement>(out PlayerMovement player))
            {
                HandleGameOver(player.gameObject);

            }
        }

        private void HandleGameOver(GameObject playerObj)
        {
            Debug.Log("<color=red>💀 Game Over </color>");
            GameEvents.RaisePlayerDeath();
        }
    }
}