using UnityEngine;
using Core;
using Features.Player.Scripts;
using System.Collections;

namespace Features.Hazards.Scripts
{
    /// <summary>Laser hazard that scales with difficulty and flashes before killing.</summary>
    public class TimedLaserHazard : MonoBehaviour
    {
        [Header("Timing Settings (Base)")]
        [Tooltip("Initial delay before the cycle starts, useful for staggered laser patterns.")]
        [SerializeField] private float startDelay = 0f;

        [Tooltip("Original duration the laser is active and deadly.")]
        [SerializeField] private float appearanceDuration = 2f;

        [Tooltip("Original duration the laser is off and safe.")]
        [SerializeField] private float disappearanceDuration = 3f;

        [Header("Flicker Warning")]
        [Tooltip("Duration of the warning flicker BEFORE the laser activates.")]
        [SerializeField] private float flickerWarningDuration = 0.5f;

        [Tooltip("How fast the laser flickers during the warning phase.")]
        [SerializeField] private float flickerInterval = 0.1f;

        private EdgeCollider2D parentCollider;
        private SpriteRenderer childRenderer;
        private LineRenderer _lineRenderer;

        private void Awake()
        {
            parentCollider = GetComponent<EdgeCollider2D>();
            childRenderer = GetComponentInChildren<SpriteRenderer>();
            _lineRenderer = GetComponentInChildren<LineRenderer>();

            if (childRenderer == null)
            {
                Debug.LogError($"[TimedLaserHazard] Missing SpriteRenderer in children of {gameObject.name}!");
            }

            if (parentCollider == null)
            {
                Debug.LogError($"[TimedLaserHazard] Missing BoxCollider2D on {gameObject.name}!");
            }
            else if (!parentCollider.isTrigger)
            {
                Debug.LogWarning($"[TimedLaserHazard] BoxCollider2D on {gameObject.name} must be a Trigger. Setting automatically.");
                parentCollider.isTrigger = true;
            }
        }

        private void OnEnable()
        {
            if (childRenderer != null && parentCollider != null)
            {
                StartCoroutine(LaserCycle());
            }
        }

        private void OnDisable()
        {
            // Stop coroutines to prevent memory leaks.
            StopAllCoroutines();
            SetActiveState(false);
        }

        private IEnumerator LaserCycle()
        {
            if (startDelay > 0f) yield return new WaitForSeconds(startDelay);

            while (true)
            {
                float currentMultiplier = Mathf.Max(DifficultyManager.GlobalSpeedMultiplier, 0.1f);
                float activeDuration = appearanceDuration / currentMultiplier;
                float inactiveDuration = disappearanceDuration / currentMultiplier;

                // Cap flicker duration if inactive duration is short.
                float actualFlickerDuration = Mathf.Min(flickerWarningDuration, inactiveDuration * 0.5f);
                float safeInactiveDuration = inactiveDuration - actualFlickerDuration;

                SetActiveState(false);
                yield return new WaitForSeconds(safeInactiveDuration);

                float flickerTimer = 0f;
                bool isVisible = false;

                while (flickerTimer < actualFlickerDuration)
                {
                    isVisible = !isVisible;
                    if (childRenderer != null) childRenderer.enabled = isVisible;
                    if (_lineRenderer != null) _lineRenderer.enabled = isVisible;

                    yield return new WaitForSeconds(flickerInterval);
                    flickerTimer += flickerInterval;
                }

                SetActiveState(true);
                yield return new WaitForSeconds(activeDuration);
            }
        }

        private void SetActiveState(bool isActive)
        {
            if (childRenderer != null) childRenderer.enabled = isActive;
            if (_lineRenderer != null) _lineRenderer.enabled = isActive;
            if (parentCollider != null) parentCollider.enabled = isActive;
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            bool isPlayer = collision.TryGetComponent<PlayerCollision>(out var playerCollision);

            if (isPlayer)
            {
                if (playerCollision != null && playerCollision.IsInvulnerable) return;

                GameEvents.RaisePlayerDeath();
            }
        }
    }
}
