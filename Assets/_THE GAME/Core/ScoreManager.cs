using UnityEngine;

namespace Core
{
    /// <summary>Tracks player height to calculate score.</summary>
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
            if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Playing)
            {
                // Use Max to track only the highest point reached.
                int calculatedScore = Mathf.Max(0, height);

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

        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }
    }
}
