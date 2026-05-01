using System.Collections;
using UnityEngine;

namespace Core
{
    /// <summary>
    /// A robust Singleton manager to handle game audio independently of specific scripts.
    /// Uses GameEvents to listen for sound requests, reducing tight coupling.
    /// </summary>
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
            // Singleton implementation with DontDestroyOnLoad
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                
                // Optional robustness: dynamically add components if missing
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
            // Subscribe to the decoupled sound event
            GameEvents.OnPlaySound += HandlePlaySound;
        }

        private void OnDisable()
        {
            // Always unsubscribe to prevent memory leaks
            GameEvents.OnPlaySound -= HandlePlaySound;
        }

        /// <summary>
        /// Plays a sound effect via the static event.
        /// </summary>
        /// <param name="clip">The AudioClip to play.</param>
        /// <param name="volume">The volume level (0.0 to 1.0).</param>
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

        /// <summary>
        /// Fades into a new background music track smoothly.
        /// </summary>
        /// <param name="newClip">The new AudioClip to loop.</param>
        /// <param name="fadeDuration">Duration of the crossfade in seconds.</param>
        public void PlayMusic(AudioClip newClip, float fadeDuration = 1.0f)
        {
            if (newClip == null)
            {
                Debug.LogWarning("[SoundManager] Attempted to play a null music clip.");
                return;
            }

            // Prevent restarting the track if it's already playing
            if (musicSource.clip == newClip && musicSource.isPlaying)
            {
                return;
            }

            StartCoroutine(FadeMusicCoroutine(newClip, fadeDuration));
        }

        private IEnumerator FadeMusicCoroutine(AudioClip newClip, float fadeDuration)
        {
            float targetVolume = 1f; // Modify this if your music source max volume should be lower
            
            // Fade out the current track if it's playing
            if (musicSource.isPlaying && musicSource.clip != null)
            {
                float startVolume = musicSource.volume;
                while (musicSource.volume > 0)
                {
                    // Use unscaledDeltaTime to ensure fading works even if the game is paused
                    musicSource.volume -= startVolume * (Time.unscaledDeltaTime / fadeDuration);
                    yield return null;
                }
                musicSource.Stop();
            }

            // Prepare and play the new track
            musicSource.clip = newClip;
            musicSource.loop = true;
            musicSource.volume = 0f;
            musicSource.Play();

            // Fade in the new track
            while (musicSource.volume < targetVolume)
            {
                musicSource.volume += targetVolume * (Time.unscaledDeltaTime / fadeDuration);
                yield return null;
            }

            musicSource.volume = targetVolume;
        }
    }
}
