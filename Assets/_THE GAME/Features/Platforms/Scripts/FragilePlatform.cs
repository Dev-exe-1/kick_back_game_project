using UnityEngine;
using System.Collections;

namespace Features.Platforms.Scripts
{

    public class FragilePlatform : Platform
    {
        [SerializeField] private float _fallDelay = 1f;
        private bool _isTriggered = false;
        private Animator _animator;
        void Awake()
        {
            _animator = GetComponent<Animator>();
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

            var sprite = GetComponent<SpriteRenderer>();
            if (sprite != null) sprite.color = Color.red;

            if (_animator != null) _animator.SetTrigger("Break");


            yield return new WaitForSeconds(_fallDelay);


            gameObject.SetActive(false);
        }
    }
}