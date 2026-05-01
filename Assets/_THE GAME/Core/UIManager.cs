using System.Collections;
using UnityEngine;
using Core; // To access GameManager and GameState
using TMPro;
namespace UI
{
    /// <summary>
    /// Manages all UI transitions and interactions.
    /// Acts as a pure listener to the GameManager and other event systems, maintaining decoupling.
    /// </summary>
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
            // Subscribe to global state changes
            GameManager.OnStateChanged += HandleGameStateChanged;
            GameEvents.OnScoreChanged += UpdateScoreDisplay;
            GameEvents.OnGamePaused += HandleGamePaused;
            GameEvents.OnGameResumed += HandleGameResumed;
        }

        private void OnDisable()
        {
            // Always unsubscribe to prevent memory leaks!
            GameManager.OnStateChanged -= HandleGameStateChanged;
            GameEvents.OnScoreChanged -= UpdateScoreDisplay;
            GameEvents.OnGamePaused -= HandleGamePaused;
            GameEvents.OnGameResumed -= HandleGameResumed;
        }

        private void Start()
        {
            // Ensure UI is immediately synchronized with the actual GameState
            if (GameManager.Instance != null)
            {
                Debug.Log($"[UIManager] Start() Polling GameManager. Found State: {GameManager.Instance.CurrentState}");
                HandleGameStateChanged(GameManager.Instance.CurrentState);
            }
            else
            {
                // Fallback for isolated UI testing
                Debug.LogWarning("[UIManager] Start() - No GameManager found. Defaulting to MainMenu isolated state.");
                SetCanvasGroupState(startMenu, true);
                SetCanvasGroupState(hudPanel, false);
                SetCanvasGroupState(gameOverMenu, false);
                SetCanvasGroupState(pausePanel, false);
            }
        }

        /// <summary>
        /// Pure listener method reacting to GameState changes.
        /// </summary>
        /// <param name="newState">The new state provided by the GameManager.</param>
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
                    // Handled specifically by OnGamePaused/Resumed events.
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

        /// <summary>
        /// Displays the game over menu after a brief delay for better UX.
        /// </summary>
        private IEnumerator ShowGameOverMenuWithDelay()
        {
            Debug.Log($"[UIManager] ShowGameOverMenuWithDelay Started. Waiting {gameOverDelay} realtime seconds...");
            // Use WaitForSecondsRealtime in case time scale was set to 0 by the GameManager
            yield return new WaitForSecondsRealtime(gameOverDelay);
            Debug.Log("[UIManager] Delay complete. Showing GameOver Panel.");
            SetCanvasGroupState(gameOverMenu, true);
        }

        /// <summary>
        /// Helper to handle CanvasGroup visibility and interaction cleanly.
        /// </summary>
        /// <param name="group">Target CanvasGroup.</param>
        /// <param name="visible">True to show and enable interactions, false to hide and disable.</param>
        private void SetCanvasGroupState(CanvasGroup group, bool visible)
        {
            if (group == null)
            {
                Debug.LogWarning("UIManager: Missing CanvasGroup reference!", this);
                return;
            }

            // ROBUSTNESS FIX: If the GameObject was hard-disabled in the editor,
            // CanvasGroup changes won't render. Ensure it's active.
            if (visible && !group.gameObject.activeSelf)
            {
                Debug.Log($"[UIManager] CanvasGroup {group.gameObject.name} was disabled in hierarchy. Activating GameObject.");
                group.gameObject.SetActive(true);
            }

            group.alpha = visible ? 1f : 0f;
            group.interactable = visible;
            group.blocksRaycasts = visible;
        }

        /// <summary>
        /// Updates the score text display.
        /// </summary>
        /// <param name="newScore">The new score value.</param>
        private void UpdateScoreDisplay(int newScore)
        {
            if (scoreText != null)
            {
                scoreText.text = $"{newScore}m";
            }
        }

        /// <summary>
        /// Fetches the latest scores and updates the Game Over screen UI.
        /// </summary>
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

                    // Visual Polish: New Record highlighting
                    if (currentScore > 0 && currentScore >= savedHighScore)
                    {
                        // Gold color for new record
                        highScoreTextGameOver.color = new Color(1f, 0.84f, 0f);
                    }
                    else
                    {
                        // Default white color
                        highScoreTextGameOver.color = Color.white;
                    }
                }
            }
        }

        #region Button Event Handlers

        /// <summary>
        /// Called from the UI Button's OnClick event.
        /// Directs the flow cleanly to the GameManager.
        /// </summary>
        public void HandleStartButtonPressed()
        {
            if (buttonClickClip != null)
                GameEvents.RaisePlaySound(buttonClickClip, 0.6f);

            if (GameManager.Instance != null)
            {
                GameManager.Instance.StartNewGame();
            }
        }

        /// <summary>
        /// Called from the UI Button's OnClick event.
        /// Requests a complete restart from the GameManager.
        /// </summary>
        public void HandleRestartButtonPressed()
        {
            // Optional: You can add the button click sound here as well if you want consistency!
            if (buttonClickClip != null) GameEvents.RaisePlaySound(buttonClickClip, 0.6f);

            if (GameManager.Instance != null)
            {
                GameManager.Instance.RestartGame();
            }
        }

        /// <summary>
        /// Toggles the pause state from the UI.
        /// </summary>
        public void HandlePauseButtonPressed()
        {
            if (buttonClickClip != null) GameEvents.RaisePlaySound(buttonClickClip, 0.6f);

            if (GameManager.Instance != null)
            {
                GameManager.Instance.TogglePause();
            }
        }

        /// <summary>
        /// Resumes the game from the UI.
        /// </summary>
        public void HandleResumeButtonPressed()
        {
            if (buttonClickClip != null) GameEvents.RaisePlaySound(buttonClickClip, 0.6f);

            if (GameManager.Instance != null)
            {
                GameManager.Instance.TogglePause();
            }
        }

        /// <summary>
        /// Returns to the Main Menu from the UI.
        /// </summary>
        public void HandleHomeButtonPressed()
        {
            if (buttonClickClip != null) GameEvents.RaisePlaySound(buttonClickClip, 0.6f);

            if (GameManager.Instance != null)
            {
                GameManager.Instance.ReturnToMainMenu();
            }
        }

        #endregion
    }
}
