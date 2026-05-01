using UnityEngine;

namespace Features.Camera.Scripts
{
    /// <summary>Controls camera to strictly follow target upwards.</summary>
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

        private Vector3 _velocity = Vector3.zero;

        private float _highestTargetY;

        private void Start()
        {
            if (_target == null)
            {
                Debug.LogWarning("VerticalCameraFollow has no target assigned!");
                return;
            }

            // Initialize using current position to prevent snapping.
            _highestTargetY = transform.position.y;
        }

        private void LateUpdate()
        {
            if (_target == null) return;

            float targetY = _target.position.y + _verticalOffset;

            if (targetY > _highestTargetY)
            {
                _highestTargetY = targetY;
            }

            Vector3 targetPosition = new Vector3(transform.position.x, _highestTargetY, transform.position.z);

            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref _velocity, _smoothTime);
        }
    }
}
