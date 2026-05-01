using UnityEngine;

namespace Core
{
    /// <summary>Global Hub for difficulty scaling.</summary>
    public class DifficultyManager : MonoBehaviour
    {
        [Tooltip("Assign the DifficultySettings ScriptableObject here.")]
        [SerializeField] private DifficultySettings settings;

        public static float GlobalSpeedMultiplier { get; private set; } = 1f;

        private void Awake()
        {
            if (settings == null)
            {
                Debug.LogWarning("[DifficultyManager] DifficultySettings not assigned! Creating default instance.");
                settings = ScriptableObject.CreateInstance<DifficultySettings>();
            }
        }

        private void OnEnable()
        {
            GameManager.OnStateChanged += HandleGameStateChanged;
            GameEvents.OnScoreChanged += HandleScoreChanged;
        }

        private void OnDisable()
        {
            GameManager.OnStateChanged -= HandleGameStateChanged;
            GameEvents.OnScoreChanged -= HandleScoreChanged;
        }

        private void HandleScoreChanged(int newScore)
        {
            if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Playing)
            {
                if (settings != null) {
                    GlobalSpeedMultiplier = Mathf.Min(
                        settings.maxSpeedMultiplier,
                        settings.baseSpeed + (newScore * settings.speedIncreasePerMeter)
                    );
                }
            }
        }

        private void HandleGameStateChanged(GameState state)
        {
            if (state == GameState.MainMenu)
            {
                GlobalSpeedMultiplier = settings != null ? settings.baseSpeed : 1f;
            }
        }
    }
}
