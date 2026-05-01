using UnityEngine;
using System.Collections;

namespace Features.Platforms.Scripts
{
    public class FragilePlatform : Platform
    {
        [SerializeField] private float _fallDelay = 1f;
        [SerializeField] private float _sinkDistance = 0.1f;
        [SerializeField] private float _fallSpeed = 8f;
        [SerializeField] private float _cleanupDelay = 2f;

        private bool _isTriggered = false;
        private bool _isFalling = false;

        private Animator _animator;
        private SpriteRenderer _spriteRenderer;
        private BoxCollider2D _boxCollider;

        // Use hash to avoid string allocations and improve performance.
        private readonly int _platformOnHash = Animator.StringToHash("platform_on");
        private readonly int _platformOffHash = Animator.StringToHash("platform_off");

        private Vector3 _initialLocalPosition;

        void Awake()
        {
            _animator = GetComponentInChildren<Animator>();
            _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            _boxCollider = GetComponent<BoxCollider2D>();
            _initialLocalPosition = transform.localPosition;

            if (_animator != null)
            {
                _animator.Play(_platformOnHash);
            }
        }

        void OnEnable()
        {
            _isTriggered = false;
            _isFalling = false;
            transform.localPosition = _initialLocalPosition;

            if (_spriteRenderer != null)
            {
                _spriteRenderer.enabled = true;
            }
            if (_boxCollider != null)
            {
                _boxCollider.enabled = true;
            }
            if (_animator != null)
            {
                _animator.Play(_platformOnHash);
            }
        }

        void Update()
        {
            if (_isFalling)
            {
                transform.Translate(Vector3.down * _fallSpeed * Time.deltaTime);
            }
        }

        public override void OnLanded(GameObject player)
        {
            base.OnLanded(player);

            if (!_isTriggered)
            {
                _isTriggered = true;
                StartCoroutine(FallSequence());
            }
        }

        private IEnumerator FallSequence()
        {
            transform.Translate(Vector3.down * _sinkDistance);

            yield return new WaitForSeconds(_fallDelay - 0.5f);

            float blinkDuration = 0.5f;
            float blinkInterval = 0.1f;
            float elapsed = 0f;

            while (elapsed < blinkDuration)
            {
                if (_spriteRenderer != null)
                {
                    _spriteRenderer.enabled = !_spriteRenderer.enabled;
                }
                yield return new WaitForSeconds(blinkInterval);
                elapsed += blinkInterval;
            }

            if (_spriteRenderer != null)
            {
                _spriteRenderer.enabled = true;
            }

            if (_animator != null)
            {
                _animator.Play(_platformOffHash);
            }
            if (_boxCollider != null)
            {
                _boxCollider.enabled = false;
            }
            _isFalling = true;

            yield return new WaitForSeconds(_cleanupDelay);
            if (_spriteRenderer != null)
            {
                _spriteRenderer.enabled = false;
            }
            _isFalling = false;
        }
    }
}