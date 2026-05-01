using UnityEngine;

namespace Core
{
    /// <summary>
    /// Tracks the player's height to calculate the score.
    /// Follows clean architecture by depending on GameState and raising events when the score changes.
    /// </summary>
    [DefaultExecutionOrder(-50)]
    public class ScoreManager : MonoBehaviour
    {
        public static ScoreManager instance;
        private int currentScore = 0;
        private GameSaveData saveProfile;

        public int CurrentScore => currentScore;
        public GameSaveData SaveProfile => saveProfile;
        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }
        private void Start()
        {
            saveProfile = SaveSystem.Load<GameSaveData>();
        }

        private void OnEnable()
        {
            GameManager.OnStateChanged += HandleGameStateChanged;
            GameEvents.OnPlayerHeightChanged += HandlePlayerHeightChanged;
        }

        private void OnDisable()
        {
            GameManager.OnStateChanged -= HandleGameStateChanged;
            GameEvents.OnPlayerHeightChanged -= HandlePlayerHeightChanged;
        }

        private void HandlePlayerHeightChanged(int height)
        {
            // State-Gate: Calculate score only while playing
            if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Playing)
            {
                // Height-based scoring: Mathf.Max ensures it only represents the highest point reached
                int calculatedScore = Mathf.Max(0, height);

                // Only update and raise event if the score actually increased
                if (calculatedScore > currentScore)
                {
                    currentScore = calculatedScore;
                    GameEvents.RaiseScoreChanged(currentScore);
                }
            }
        }

        private void HandleGameStateChanged(GameState state)
        {
            if (state == GameState.GameOver)
            {
                if (currentScore > saveProfile.highScore)
                {
                    saveProfile.highScore = currentScore;
                    SaveSystem.Save(saveProfile);
                }
            }

            // Reset score to 0 when the game transitions back to the Main Menu
            if (state == GameState.MainMenu)
            {
                ResetScore();
            }
        }

        private void ResetScore()
        {
            if (currentScore != 0)
            {
                currentScore = 0;
                GameEvents.RaiseScoreChanged(currentScore);
            }
        }
    }
}
