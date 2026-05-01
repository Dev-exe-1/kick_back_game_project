using UnityEngine;
using Core;

namespace Features.Hazards
{
    /// <summary>
    /// Generic movement script that scales with global difficulty.
    /// Can be applied to platforms, enemies, or lasers.
    /// </summary>
    public class MovingHazard : MonoBehaviour
    {
        [Tooltip("The local direction this hazard moves.")]
        [SerializeField] private Vector3 direction = Vector3.down;

        [Tooltip("The base speed of this specific hazard.")]
        [SerializeField] private float individualBaseSpeed = 5f;

        private void Update()
        {
            // Zero-Dependency: Reads the static GlobalSpeedMultiplier directly.
            float currentSpeed = individualBaseSpeed * DifficultyManager.GlobalSpeedMultiplier;
            transform.Translate(direction.normalized * currentSpeed * Time.deltaTime);
        }
    }
}
