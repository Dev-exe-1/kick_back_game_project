using System;
using UnityEngine;

namespace Core
{
    public static class GameEvents
    {
        public static event Action<int> OnScoreUpdated;
        public static void RaiseScoreUpdated(int score)
        {
            OnScoreUpdated?.Invoke(score);
        }

        public static event Action OnPlayerDeath;

        public static void RaisePlayerDeath()
        {
            OnPlayerDeath?.Invoke();

        }
    }
}
