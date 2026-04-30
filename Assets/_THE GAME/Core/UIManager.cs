using System.Collections;
using UnityEngine;
using Core; // To access GameManager and GameState

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

        [Header("Settings")]
        [Tooltip("Delay in seconds before showing the Game Over screen after death.")]
        [SerializeField] private float gameOverDelay = 1.0f;

        private void OnEnable()
        {
            // Subscribe to global state changes
            GameManager.OnStateChanged += HandleGameStateChanged;
        }

        private void OnDisable()
        {
            // Always unsubscribe to prevent memory leaks!
            GameManager.OnStateChanged -= HandleGameStateChanged;
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
                    break;

                case GameState.Playing:
                    Debug.Log("[UIManager] Entering Playing block. Transitioning to HUD.");
                    SetCanvasGroupState(startMenu, false);
                    SetCanvasGroupState(hudPanel, true);
                    SetCanvasGroupState(gameOverMenu, false);
                    break;

                case GameState.GameOver:
                    Debug.Log("[UIManager] Entering GameOver block. Hiding HUD, initiating Delay Coroutine.");
                    SetCanvasGroupState(startMenu, false);
                    SetCanvasGroupState(hudPanel, false);
                    StartCoroutine(ShowGameOverMenuWithDelay());
                    break;

                case GameState.Paused:
                    break;
            }
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

        #region Button Event Handlers

        /// <summary>
        /// Called from the UI Button's OnClick event.
        /// Directs the flow cleanly to the GameManager.
        /// </summary>
        public void HandleStartButtonPressed()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.UpdateState(GameState.Playing);
            }
        }

        /// <summary>
        /// Called from the UI Button's OnClick event.
        /// Requests a complete restart from the GameManager.
        /// </summary>
        public void HandleRestartButtonPressed()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.RestartGame();
            }
        }

        #endregion
    }
}
