using UnityEngine;
using Core;
using System.Collections;

namespace Features.Platforms.Scripts
{
    [RequireComponent(typeof(Collider2D))]

    public class TimedTogglePlatform : MonoBehaviour, IGroundable
    {
        [Header("Timing Settings (Base)")]
        [SerializeField] private float startDelay = 0f;
        [SerializeField] private float originalAppearanceDuration = 3f;
        [SerializeField] private float originalDisappearanceDuration = 2f;

        [Header("Flicker Warning")]
        [SerializeField] private float flickerWarningDuration = 0.5f;
        [SerializeField] private float flickerInterval = 0.1f;

        private Renderer _renderer;
        private Collider2D _collider;

        private void Awake()
        {
            _collider = GetComponent<Collider2D>();
            

            _renderer = GetComponentInChildren<Renderer>();

            if (_renderer == null)
            {
                Debug.LogError($"TimedTogglePlatform on {gameObject.name} needs a Renderer in itself or its children!");
            }
        }

        private void OnEnable()
        {
            if (_renderer != null) StartCoroutine(ToggleCycle());
        }

        private void OnDisable()
        {
            StopAllCoroutines();
        }

        private IEnumerator ToggleCycle()
        {
            if (startDelay > 0f) yield return new WaitForSeconds(startDelay);

            while (true)
            {
                float currentMultiplier = Mathf.Max(DifficultyManager.GlobalSpeedMultiplier, 0.1f);
                float appearanceDuration = originalAppearanceDuration / currentMultiplier;
                float disappearanceDuration = originalDisappearanceDuration / currentMultiplier;

                float actualFlickerDuration = Mathf.Min(flickerWarningDuration, appearanceDuration * 0.5f);
                float stableActiveDuration = appearanceDuration - actualFlickerDuration;

                // --- 1. ACTIVE STATE ---
                SetActiveState(true);
                yield return new WaitForSeconds(stableActiveDuration);

                // --- 2. FLICKER WARNING ---
                float flickerTimer = 0f;
                bool isVisible = true;
                
                while (flickerTimer < actualFlickerDuration)
                {
                    isVisible = !isVisible;
                    _renderer.enabled = isVisible;
                    yield return new WaitForSeconds(flickerInterval);
                    flickerTimer += flickerInterval;
                }

                // --- 3. INACTIVE STATE ---
                SetActiveState(false);
                yield return new WaitForSeconds(disappearanceDuration);
            }
        }

        private void SetActiveState(bool isActive)
        {
            if (_renderer != null) _renderer.enabled = isActive;
            if (_collider != null) _collider.enabled = isActive;
        }

        public void OnLanded(GameObject player) { /* IGroundable Implementation */ }
    }
}