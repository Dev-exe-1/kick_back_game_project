using System;
using UnityEngine;

namespace Core
{
    public static class GameEvents
    {
        public static event Action<int> OnScoreChanged;
        public static void RaiseScoreChanged(int score)
        {
            OnScoreChanged?.Invoke(score);
        }

        public static event Action<int> OnPlayerHeightChanged;
        public static void RaisePlayerHeightChanged(int height) => OnPlayerHeightChanged?.Invoke(height);

        public static event Action OnPlayerDeath;

        public static void RaisePlayerDeath()
        {
            OnPlayerDeath?.Invoke();
        }

        public static event Action OnGamePaused;
        public static void RaiseGamePaused() => OnGamePaused?.Invoke();

        public static event Action OnGameResumed;
        public static void RaiseGameResumed() => OnGameResumed?.Invoke();

        public static event Action OnGameReset;
        public static void RaiseGameReset() => OnGameReset?.Invoke();

        public static event Action<AudioClip, float> OnPlaySound;
        public static void RaisePlaySound(AudioClip clip, float volume = 1f) => OnPlaySound?.Invoke(clip, volume);
    }
}
