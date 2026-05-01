using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Core;

namespace Features.VFX
{
    [RequireComponent(typeof(Image))]
    public class ScreenFlashEffect : MonoBehaviour
    {
        [SerializeField] private float _flashDuration = 0.2f;

        private Image _flashImage;
        private Coroutine _flashCoroutine;

        private void Awake()
        {
            _flashImage = GetComponent<Image>();
            
            _flashImage.raycastTarget = false;
            
            SetAlpha(0f);
        }

        private void OnEnable()
        {
            GameEvents.OnPlayerDeath += TriggerFlash;
        }

        private void OnDisable()
        {
            GameEvents.OnPlayerDeath -= TriggerFlash;
        }

        private void TriggerFlash()
        {
            if (_flashCoroutine != null)
            {
                StopCoroutine(_flashCoroutine);
            }
            
            _flashCoroutine = StartCoroutine(FlashRoutine());
        }

        private IEnumerator FlashRoutine()
        {
            SetAlpha(1f);

            float elapsedTime = 0f;

            // Use unscaled delta time to animate while paused.
            while (elapsedTime < _flashDuration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                
                float alpha = Mathf.Lerp(1f, 0f, elapsedTime / _flashDuration);
                SetAlpha(alpha);
                
                yield return null;
            }

            SetAlpha(0f);
            _flashCoroutine = null;
        }

        private void SetAlpha(float alpha)
        {
            Color color = _flashImage.color;
            color.a = alpha;
            _flashImage.color = color;
        }
    }
}
