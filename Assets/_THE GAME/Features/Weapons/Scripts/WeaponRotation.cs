using UnityEngine;
using Features.Player.Scripts;

namespace Features.Weapons.Scripts
{
    public class WeaponRotation : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _weaponSprite;
        
        private void OnEnable()
        {
            PlayerEvents.OnAim += RotateWeapon;
        }

        private void OnDisable()
        {
            PlayerEvents.OnAim -= RotateWeapon;
        }

        private void RotateWeapon(Vector2 aimDirection)
        {
            float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);

            if (_weaponSprite != null)
            {
                if (Mathf.Abs(angle) > 90f)
                {
                    _weaponSprite.flipY = true;
                }
                else
                {
                    _weaponSprite.flipY = false;
                }
            }
        }
    }
}
