using UnityEngine;
using Features.Player.Data;

namespace Features.Player.Scripts
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerMovement : MonoBehaviour
    {
        [SerializeField] private PlayerStats _stats;
        private Rigidbody2D _rb;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
        }

        private void OnEnable()
        {
            PlayerEvents.OnRecoilApplied += ApplyRecoil;
        }

        private void OnDisable()
        {
            PlayerEvents.OnRecoilApplied -= ApplyRecoil;
        }

        private void ApplyRecoil(Vector2 direction)
        {
            if (_stats == null) return;
            
            // Reset velocity before applying impulse for consistent game feel
            _rb.linearVelocity = Vector2.zero; 
            _rb.AddForce(direction * _stats.recoilForce, ForceMode2D.Impulse);
        }
    }
}
