using System.Collections;
using UnityEngine;
using Core;

namespace Features.Camera
{
    public class CameraShake : MonoBehaviour
    {
        [Header("Shake Settings")]
        [SerializeField] private float _shakeDuration = 0.2f;
        [SerializeField] private float _shakeMagnitude = 0.1f;

        private Coroutine _shakeCoroutine;
        private Vector3 _originalLocalPosition;
        private bool _isShaking;

        private void OnEnable()
        {
            GameEvents.OnPlayerDeath += HandlePlayerDeath;
        }

        private void OnDisable()
        {
            GameEvents.OnPlayerDeath -= HandlePlayerDeath;
            ResetCameraPosition();
        }

        private void HandlePlayerDeath()
        {
            if (_isShaking && _shakeCoroutine != null)
            {
                StopCoroutine(_shakeCoroutine);
            }
            
            _shakeCoroutine = StartCoroutine(ShakeCoroutine());
        }

        private IEnumerator ShakeCoroutine()
        {
            if (!_isShaking)
            {
                _originalLocalPosition = transform.localPosition;
                _isShaking = true;
            }

            float elapsed = 0f;

            while (elapsed < _shakeDuration)
            {
                transform.localPosition = _originalLocalPosition + Random.insideUnitSphere * _shakeMagnitude;
                elapsed += Time.deltaTime;
                yield return null;
            }

            ResetCameraPosition();
        }

        private void ResetCameraPosition()
        {
            if (_isShaking)
            {
                transform.localPosition = _originalLocalPosition;
                _isShaking = false;
            }
        }
    }
}
