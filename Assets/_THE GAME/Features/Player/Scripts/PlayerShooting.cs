using UnityEngine;
using Core;
using Features.Player.Data;
using Features.Weapons.Data;
using Features.Weapons.Scripts;

namespace Features.Player.Scripts
{
    public class PlayerShooting : MonoBehaviour
    {
        [SerializeField] private PlayerStats _stats;
        [SerializeField] private ProjectileData _projectileData;
        [SerializeField] private Transform _firePoint;
        [Header("Audio")]
        [SerializeField] private AudioClip shootClip;

        private bool _canShoot = true;
        private bool _isGrounded = true;
        private float _softlockGraceTimer = 0f;
        private GameObject _currentProjectile;
        private Vector2 _currentAimDirection;
        private Collider2D _playerCollider;

        private void Awake()
        {
            _playerCollider = GetComponent<Collider2D>();
        }

        private void OnEnable()
        {
            PlayerEvents.OnAim += UpdateAimDirection;
            PlayerEvents.OnTryShoot += TryShoot;
            PlayerEvents.OnGrounded += ResetCanShoot;
            PlayerEvents.OnAirborne += HandleAirborne;
        }

        private void OnDisable()
        {
            PlayerEvents.OnAim -= UpdateAimDirection;
            PlayerEvents.OnTryShoot -= TryShoot;
            PlayerEvents.OnGrounded -= ResetCanShoot;
            PlayerEvents.OnAirborne -= HandleAirborne;
        }

        private void Update()
        {
            if (_softlockGraceTimer > 0f)
            {
                _softlockGraceTimer -= Time.deltaTime;
                return;
            }

            // Detect softlock: player is stuck on ground with no ammo and no active bullet.
            if (!_canShoot && _isGrounded)
            {
                if (_currentProjectile == null || !_currentProjectile.activeInHierarchy)
                {
                    GameEvents.RaisePlayerDeath();
                }
            }
        }

        private void UpdateAimDirection(Vector2 aimDir)
        {
            _currentAimDirection = aimDir;
        }

        private void TryShoot()
        {
            if (_canShoot)
            {
                Shoot();
            }
        }

        private void Shoot()
        {

            if (shootClip != null)
            {
                GameEvents.RaisePlaySound(shootClip, 0.8f);
            }

            _canShoot = false;
            _softlockGraceTimer = 0.25f;

            if (_projectileData != null && _projectileData.prefab != null && _firePoint != null)
            {
                GameObject bulletObj = ObjectPoolManager.Instance.GetFromPool(
                    _projectileData.prefab,
                    _firePoint.position,
                    Quaternion.identity
                );

                _currentProjectile = bulletObj;

                bulletObj.transform.up = _currentAimDirection;

                Projectile projectile = bulletObj.GetComponent<Projectile>();
                if (projectile != null)
                {
                    projectile.Initialize(_projectileData, _currentAimDirection, _playerCollider);
                }
            }

            Vector2 recoilDirection = -_currentAimDirection.normalized;
            PlayerEvents.RaiseRecoilApplied(recoilDirection);
        }

        private void ResetCanShoot()
        {
            _canShoot = true;
            _isGrounded = true;
        }

        private void HandleAirborne()
        {
            _isGrounded = false;
        }
    }
}
