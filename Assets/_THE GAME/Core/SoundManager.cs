using System.Collections;
using UnityEngine;

namespace Core
{
    /// <summary>Singleton manager for independent audio handling.</summary>
    public class SoundManager : MonoBehaviour
    {
        public static SoundManager Instance { get; private set; }

        [Header("Audio Sources")]
        [Tooltip("Audio source for playing looping music.")]
        [SerializeField] private AudioSource musicSource;
        
        [Tooltip("Audio source for playing one-shot sound effects.")]
        [SerializeField] private AudioSource sfxSource;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                
                if (musicSource == null)
                {
                    musicSource = gameObject.AddComponent<AudioSource>();
                    musicSource.loop = true;
                }
                
                if (sfxSource == null)
                {
                    sfxSource = gameObject.AddComponent<AudioSource>();
                }
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void OnEnable()
        {
            GameEvents.OnPlaySound += HandlePlaySound;
        }

        private void OnDisable()
        {
            // Unsubscribe to prevent memory leaks.
            GameEvents.OnPlaySound -= HandlePlaySound;
        }

        /// <summary>Plays a sound effect.</summary>
        private void HandlePlaySound(AudioClip clip, float volume)
        {
            if (clip != null)
            {
                sfxSource.PlayOneShot(clip, volume);
            }
            else
            {
                Debug.LogWarning("[SoundManager] Attempted to play a null AudioClip.");
            }
        }

        /// <summary>Fades smoothly into a new background music track.</summary>
        public void PlayMusic(AudioClip newClip, float fadeDuration = 1.0f)
        {
            if (newClip == null)
            {
                Debug.LogWarning("[SoundManager] Attempted to play a null music clip.");
                return;
            }

            if (musicSource.clip == newClip && musicSource.isPlaying)
            {
                return;
            }

            StartCoroutine(FadeMusicCoroutine(newClip, fadeDuration));
        }

        private IEnumerator FadeMusicCoroutine(AudioClip newClip, float fadeDuration)
        {
            float targetVolume = 1f; 
            
            if (musicSource.isPlaying && musicSource.clip != null)
            {
                float startVolume = musicSource.volume;
                while (musicSource.volume > 0)
                {
                    // Use unscaledDeltaTime to allow fading while paused.
                    musicSource.volume -= startVolume * (Time.unscaledDeltaTime / fadeDuration);
                    yield return null;
                }
                musicSource.Stop();
            }

            musicSource.clip = newClip;
            musicSource.loop = true;
            musicSource.volume = 0f;
            musicSource.Play();

            while (musicSource.volume < targetVolume)
            {
                musicSource.volume += targetVolume * (Time.unscaledDeltaTime / fadeDuration);
                yield return null;
            }

            musicSource.volume = targetVolume;
        }
    }
}
