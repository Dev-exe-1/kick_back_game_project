using UnityEngine;
using Core;
using Features.Player.Scripts;
using System.Collections;

namespace Features.Hazards.Scripts
{
    /// <summary>
    /// A decoupled laser hazard that scales its toggle cycle based on global difficulty.
    /// Uses a flicker warning before activating, and instantly kills the player on contact.
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
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

        private BoxCollider2D parentCollider;
        private SpriteRenderer childRenderer;

        private void Awake()
        {
            parentCollider = GetComponent<BoxCollider2D>();
            childRenderer = GetComponentInChildren<SpriteRenderer>();

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
            // Safety: Ensure all coroutines stop to prevent memory leaks during chunk cleanup
            StopAllCoroutines();
            SetActiveState(false);
        }

        private IEnumerator LaserCycle()
        {
            if (startDelay > 0f) yield return new WaitForSeconds(startDelay);

            while (true)
            {
                // 1. Difficulty Scaling
                float currentMultiplier = Mathf.Max(DifficultyManager.GlobalSpeedMultiplier, 0.1f);
                float activeDuration = appearanceDuration / currentMultiplier;
                float inactiveDuration = disappearanceDuration / currentMultiplier;

                // Safely cap flicker duration if the inactive duration gets extremely short
                float actualFlickerDuration = Mathf.Min(flickerWarningDuration, inactiveDuration * 0.5f);
                float safeInactiveDuration = inactiveDuration - actualFlickerDuration;

                // --- 2. INACTIVE STATE ---
                SetActiveState(false);
                yield return new WaitForSeconds(safeInactiveDuration);

                // --- 3. FLICKER WARNING STATE (Preparing to Fire) ---
                float flickerTimer = 0f;
                bool isVisible = false;
                
                while (flickerTimer < actualFlickerDuration)
                {
                    isVisible = !isVisible;
                    if (childRenderer != null) childRenderer.enabled = isVisible;
                    
                    yield return new WaitForSeconds(flickerInterval);
                    flickerTimer += flickerInterval;
                }

                // --- 4. ACTIVE STATE (Firing) ---
                SetActiveState(true);
                yield return new WaitForSeconds(activeDuration);
            }
        }

        private void SetActiveState(bool isActive)
        {
            if (childRenderer != null) childRenderer.enabled = isActive;
            if (parentCollider != null) parentCollider.enabled = isActive;
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            // Search for Player components on the colliding object
            PlayerCollision playerCollision = collision.GetComponent<PlayerCollision>();
            PlayerMovement playerMovement = collision.GetComponent<PlayerMovement>();

            if (playerCollision != null || playerMovement != null)
            {
                // Check for future shield mechanic
                if (playerCollision != null && playerCollision.IsInvulnerable) return;

                // Instantly trigger the global death event
                GameEvents.RaisePlayerDeath();
            }
        }
    }
}
