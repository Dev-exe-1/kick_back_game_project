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
        
        private bool _canShoot = true;
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
        }

        private void OnDisable()
        {
            PlayerEvents.OnAim -= UpdateAimDirection;
            PlayerEvents.OnTryShoot -= TryShoot;
            PlayerEvents.OnGrounded -= ResetCanShoot;
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
            _canShoot = false;
            
            if (_projectileData != null && _projectileData.prefab != null && _firePoint != null)
            {
                GameObject bulletObj = ObjectPoolManager.Instance.GetFromPool(
                    _projectileData.prefab, 
                    _firePoint.position, 
                    Quaternion.identity
                );

                bulletObj.transform.up = _currentAimDirection;

                Projectile projectile = bulletObj.GetComponent<Projectile>();
                if (projectile != null)
                {
                    projectile.Initialize(_projectileData, _currentAimDirection, _playerCollider);
                }
            }

            // Apply Recoil (Opposite of aim)
            Vector2 recoilDirection = -_currentAimDirection.normalized;
            PlayerEvents.RaiseRecoilApplied(recoilDirection);
        }

        private void ResetCanShoot()
        {
            _canShoot = true;
        }
    }
}
