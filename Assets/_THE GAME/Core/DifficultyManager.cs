using UnityEngine;

namespace Core
{
    /// <summary>
    /// Global Hub for difficulty scaling. Decoupled from logic objects.
    /// Exposes a static GlobalSpeedMultiplier.
    /// </summary>
    public class DifficultyManager : MonoBehaviour
    {
        [Tooltip("Assign the DifficultySettings ScriptableObject here.")]
        [SerializeField] private DifficultySettings settings;

        // Static Property for Zero-Dependency access by hazards
        public static float GlobalSpeedMultiplier { get; private set; } = 1f;

        private ScoreManager scoreManager;

        private void Awake()
        {
            if (settings == null)
            {
                Debug.LogWarning("[DifficultyManager] DifficultySettings not assigned! Creating default instance.");
                settings = ScriptableObject.CreateInstance<DifficultySettings>();
            }
        }

        private void Start()
        {
            // Cache ScoreManager reference for performance in Update
            scoreManager = ScoreManager.instance;
            if (scoreManager == null)
            {
                Debug.LogError("[DifficultyManager] ScoreManager not found in scene!");
            }
        }

        private void OnEnable()
        {
            GameManager.OnStateChanged += HandleGameStateChanged;
        }

        private void OnDisable()
        {
            GameManager.OnStateChanged -= HandleGameStateChanged;
        }

        private void Update()
        {
            // Only scale difficulty while actively playing
            if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Playing)
            {
                if (scoreManager != null)
                {
                    // Calculate multiplier based on the current score (height)
                    GlobalSpeedMultiplier = Mathf.Min(
                        settings.maxSpeedMultiplier,
                        settings.baseSpeed + (scoreManager.CurrentScore * settings.speedIncreasePerMeter)
                    );
                }
            }
        }

        private void HandleGameStateChanged(GameState state)
        {
            // Reset state to base speed on MainMenu
            if (state == GameState.MainMenu)
            {
                GlobalSpeedMultiplier = settings != null ? settings.baseSpeed : 1f;
            }
        }
    }
}
