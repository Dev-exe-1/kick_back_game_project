using UnityEngine;
using Features.Player.Scripts;

namespace Features.Platforms.Scripts
{
    public class MovingPlatform : Platform
    {
        [Header("Movement Settings")]
        [SerializeField] private float _speed = 3f;
        [SerializeField] private LayerMask _wallLayer;
        [SerializeField] private float _rayDistance = 0.2f;

        private int _direction = 1;
        private BoxCollider2D _collider;

        private void Start()
        {
            _collider = GetComponent<BoxCollider2D>();
        }

        private void Update()
        {
            if (Core.GameManager.Instance == null || Core.GameManager.Instance.CurrentState != Core.GameState.Playing) return;
            CheckForWalls();
            Move();
        }

        private void Move()
        {
            float currentSpeed = _speed * Core.DifficultyManager.GlobalSpeedMultiplier;
            transform.Translate(Vector2.right * _direction * currentSpeed * Time.deltaTime);
        }

        private void CheckForWalls()
        {
            float rayOriginX = (_direction == 1) ? _collider.bounds.max.x : _collider.bounds.min.x;
            Vector2 rayOrigin = new Vector2(rayOriginX, transform.position.y);

            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * _direction, _rayDistance, _wallLayer);

            if (hit.collider != null)
            {
                _direction *= -1;
            }
        }



        public override void OnLanded(GameObject player)
        {

            if (player.transform.position.y > transform.position.y + 0.1f)
            {
                base.OnLanded(player);
                player.transform.SetParent(this.transform);
            }
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            if (collision.gameObject.TryGetComponent<PlayerCollision>(out _))
            {

                collision.transform.SetParent(null);
                collision.transform.localScale = Vector3.one;
            }
        }
    }
}