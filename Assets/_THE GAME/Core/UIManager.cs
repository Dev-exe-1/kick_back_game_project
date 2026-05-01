using System.Collections;
using UnityEngine;
using Core; // To access GameManager and GameState
using TMPro;
namespace UI
{
    /// <summary>Manages UI transitions and interactions.</summary>
    public class UIManager : MonoBehaviour
    {
        [Header("UI Panels (CanvasGroups)")]
        [SerializeField] private CanvasGroup startMenu;
        [SerializeField] private CanvasGroup hudPanel;
        [SerializeField] private CanvasGroup gameOverMenu;
        [SerializeField] private CanvasGroup pausePanel;

        [Header("Settings")]
        [Tooltip("Delay in seconds before showing the Game Over screen after death.")]
        [SerializeField] private float gameOverDelay = 1.0f;

        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI currentScoreTextGameOver;
        [SerializeField] private TextMeshProUGUI highScoreTextGameOver;

        [Header("Audio")]
        [SerializeField] private AudioClip buttonClickClip;

        private void OnEnable()
        {
            GameManager.OnStateChanged += HandleGameStateChanged;
            GameEvents.OnScoreChanged += UpdateScoreDisplay;
            GameEvents.OnGamePaused += HandleGamePaused;
            GameEvents.OnGameResumed += HandleGameResumed;
        }

        private void OnDisable()
        {
            // Unsubscribe to prevent memory leaks.
            GameManager.OnStateChanged -= HandleGameStateChanged;
            GameEvents.OnScoreChanged -= UpdateScoreDisplay;
            GameEvents.OnGamePaused -= HandleGamePaused;
            GameEvents.OnGameResumed -= HandleGameResumed;
        }

        private void Start()
        {
            if (GameManager.Instance != null)
            {
                Debug.Log($"[UIManager] Start() Polling GameManager. Found State: {GameManager.Instance.CurrentState}");
                HandleGameStateChanged(GameManager.Instance.CurrentState);
            }
            else
            {
                // Fallback for isolated UI testing.
                Debug.LogWarning("[UIManager] Start() - No GameManager found. Defaulting to MainMenu isolated state.");
                SetCanvasGroupState(startMenu, true);
                SetCanvasGroupState(hudPanel, false);
                SetCanvasGroupState(gameOverMenu, false);
                SetCanvasGroupState(pausePanel, false);
            }
        }

        /// <summary>Reacts to GameState changes.</summary>
        private void HandleGameStateChanged(GameState newState)
        {
            Debug.Log($"[UIManager] HandleGameStateChanged triggered for state: {newState}");
            switch (newState)
            {
                case GameState.MainMenu:
                    Debug.Log("[UIManager] Entering MainMenu block. Displaying Start Panel.");
                    SetCanvasGroupState(startMenu, true);
                    SetCanvasGroupState(hudPanel, false);
                    SetCanvasGroupState(gameOverMenu, false);
                    SetCanvasGroupState(pausePanel, false);
                    break;

                case GameState.Playing:
                    Debug.Log("[UIManager] Entering Playing block. Transitioning to HUD.");
                    SetCanvasGroupState(startMenu, false);
                    SetCanvasGroupState(hudPanel, true);
                    SetCanvasGroupState(gameOverMenu, false);
                    SetCanvasGroupState(pausePanel, false);
                    break;

                case GameState.GameOver:
                    Debug.Log("[UIManager] Entering GameOver block. Hiding HUD, initiating Delay Coroutine.");
                    SetCanvasGroupState(startMenu, false);
                    SetCanvasGroupState(hudPanel, false);

                    UpdateGameOverScores();

                    StartCoroutine(ShowGameOverMenuWithDelay());
                    break;

                case GameState.Paused:
                    break;
            }
        }

        private void HandleGamePaused()
        {
            SetCanvasGroupState(pausePanel, true);
        }

        private void HandleGameResumed()
        {
            SetCanvasGroupState(pausePanel, false);
        }

        /// <summary>Displays the game over menu after a brief delay.</summary>
        private IEnumerator ShowGameOverMenuWithDelay()
        {
            Debug.Log($"[UIManager] ShowGameOverMenuWithDelay Started. Waiting {gameOverDelay} realtime seconds...");
            // Use WaitForSecondsRealtime in case time scale was set to 0.
            yield return new WaitForSecondsRealtime(gameOverDelay);
            Debug.Log("[UIManager] Delay complete. Showing GameOver Panel.");
            SetCanvasGroupState(gameOverMenu, true);
        }

        /// <summary>Handles CanvasGroup visibility and interaction cleanly.</summary>
        private void SetCanvasGroupState(CanvasGroup group, bool visible)
        {
            if (group == null)
            {
                Debug.LogWarning("UIManager: Missing CanvasGroup reference!", this);
                return;
            }

            // If the GameObject was disabled in the editor, CanvasGroup changes won't render. Ensure it's active.
            if (visible && !group.gameObject.activeSelf)
            {
                Debug.Log($"[UIManager] CanvasGroup {group.gameObject.name} was disabled in hierarchy. Activating GameObject.");
                group.gameObject.SetActive(true);
            }

            group.alpha = visible ? 1f : 0f;
            group.interactable = visible;
            group.blocksRaycasts = visible;
        }

        /// <summary>Updates the score text display.</summary>
        private void UpdateScoreDisplay(int newScore)
        {
            if (scoreText != null)
            {
                scoreText.text = $"{newScore}m";
            }
        }

        /// <summary>Updates the Game Over screen UI.</summary>
        private void UpdateGameOverScores()
        {
            ScoreManager scoreManager = ScoreManager.instance;
            if (scoreManager != null)
            {
                int currentScore = scoreManager.CurrentScore;
                int savedHighScore = scoreManager.SaveProfile != null ? scoreManager.SaveProfile.highScore : SaveSystem.Load<GameSaveData>().highScore;

                int displayHighScore = currentScore > savedHighScore
                                       ? currentScore
                                       : savedHighScore;

                if (currentScoreTextGameOver != null)
                    currentScoreTextGameOver.text = $"Distance: {currentScore}m";

                if (highScoreTextGameOver != null)
                {
                    highScoreTextGameOver.text = $"Best: {displayHighScore}m";

                    if (currentScore > 0 && currentScore >= savedHighScore)
                    {
                        highScoreTextGameOver.color = new Color(1f, 0.84f, 0f);
                    }
                    else
                    {
                        highScoreTextGameOver.color = Color.white;
                    }
                }
            }
        }

        #region Button Event Handlers

        /// <summary>Handles Start Button press.</summary>
        public void HandleStartButtonPressed()
        {
            if (buttonClickClip != null)
                GameEvents.RaisePlaySound(buttonClickClip, 0.6f);

            if (GameManager.Instance != null)
            {
                GameManager.Instance.StartNewGame();
            }
        }

        /// <summary>Handles Restart Button press.</summary>
        public void HandleRestartButtonPressed()
        {
            if (buttonClickClip != null) GameEvents.RaisePlaySound(buttonClickClip, 0.6f);

            if (GameManager.Instance != null)
            {
                GameManager.Instance.RestartGame();
            }
        }

        /// <summary>Handles Pause Button press.</summary>
        public void HandlePauseButtonPressed()
        {
            if (buttonClickClip != null) GameEvents.RaisePlaySound(buttonClickClip, 0.6f);

            if (GameManager.Instance != null)
            {
                GameManager.Instance.TogglePause();
            }
        }

        /// <summary>Handles Resume Button press.</summary>
        public void HandleResumeButtonPressed()
        {
            if (buttonClickClip != null) GameEvents.RaisePlaySound(buttonClickClip, 0.6f);

            if (GameManager.Instance != null)
            {
                GameManager.Instance.TogglePause();
            }
        }

        /// <summary>Handles Home Button press.</summary>
        public void HandleHomeButtonPressed()
        {
            if (buttonClickClip != null) GameEvents.RaisePlaySound(buttonClickClip, 0.6f);

            if (GameManager.Instance != null)
            {
                GameManager.Instance.ReturnToMainMenu();
            }
        }

        /// <summary>Handles Exit Button press.</summary>
        public void HandleExitButtonPressed()
        {
            if (buttonClickClip != null) GameEvents.RaisePlaySound(buttonClickClip, 0.6f);

            Application.Quit();
        }


        #endregion
    }
}
