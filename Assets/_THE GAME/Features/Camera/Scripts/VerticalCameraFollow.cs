using UnityEngine;

namespace Features.Camera.Scripts
{
    /// <summary>
    /// Controls the Main Camera to strictly follow the target upwards.
    /// Utilizes SmoothDamp for fluid movement and prevents downward tracking
    /// to support the endless vertical game design.
    /// </summary>
    public class VerticalCameraFollow : MonoBehaviour
    {
        [Header("Target Tracking")]
        [Tooltip("The Transform the camera should follow (e.g., The Player).")]
        [SerializeField] private Transform _target;
        
        [Tooltip("Offset applied to the target's Y position to frame them appropriately.")]
        [SerializeField] private float _verticalOffset = 2f;

        [Header("Smoothing")]
        [Tooltip("Approximate time it takes to reach the target. A smaller value reaches the target faster.")]
        [SerializeField] private float _smoothTime = 0.3f;

        // Reference velocity required by Vector3.SmoothDamp
        private Vector3 _velocity = Vector3.zero;

        // Tracks the highest Y point the camera's target has reached to prevent moving down
        private float _highestTargetY;

        private void Start()
        {
            if (_target == null)
            {
                Debug.LogWarning("VerticalCameraFollow has no target assigned!");
                return;
            }

            // Initialize the highest Y tracking based on current setup, minus offset to prevent snap
            _highestTargetY = transform.position.y;
        }

        private void LateUpdate()
        {
            if (_target == null) return;

            // Calculate the desired Y position including the vertical framing offset
            float targetY = _target.position.y + _verticalOffset;

            // One-Way Movement Logic: Only allow the camera target Y to increase, never decrease.
            if (targetY > _highestTargetY)
            {
                _highestTargetY = targetY;
            }

            // Define the desired target position keeping the camera's original X and Z.
            Vector3 targetPosition = new Vector3(transform.position.x, _highestTargetY, transform.position.z);

            // Apply fluid smoothing towards the strictly ascending target position
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref _velocity, _smoothTime);
        }
    }
}
