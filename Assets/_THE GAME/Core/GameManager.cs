using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Core
{
    /// <summary>
    /// Represents the high-level flow of the application.
    /// </summary>
    public enum GameState
    {
        MainMenu,
        Playing,
        GameOver,
        Paused
    }

    /// <summary>
    /// A robust Singleton manager to handle global game states, time scaling, and scene transitions.
    /// Uses the Observer Pattern to broadcast state changes cleanly across all systems.
    /// Efficiency Note: This script intentionally lacks an Update() method to save CPU cycles.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        /// <summary>
        /// Standard Singleton instance access.
        /// </summary>
        public static GameManager Instance { get; private set; }

        /// <summary>
        /// Global Observer event fired whenever the GameState changes.
        /// </summary>
        public static event Action<GameState> OnStateChanged;

        /// <summary>
        /// The current state of the game.
        /// </summary>
        public GameState CurrentState { get; private set; }

        private void Awake()
        {
            // Enforce Singleton Pattern
            if (Instance == null)
            {
                Instance = this;
                // Optional: DontDestroyOnLoad(gameObject) if you use multiple scenes
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            // Default to MainMenu since UI is now built
            UpdateState(GameState.MainMenu);
        }
        private void OnEnable()
        {
            // الاشتراك في حدث الموت العالمي[cite: 2]
            GameEvents.OnPlayerDeath += HandlePlayerDeath;
        }

        private void OnDisable()
        {
            // إلغاء الاشتراك لضمان سلامة الذاكرة[cite: 2]
            GameEvents.OnPlayerDeath -= HandlePlayerDeath;
        }

        /// <summary>
        /// Transitions the game into a new state and broadcasts the change.
        /// </summary>
        /// <param name="newState">The target GameState to transition to.</param>
        public void UpdateState(GameState newState)
        {
            Debug.Log($"[GameManager] UpdateState requested. Transitioning from {CurrentState} to {newState}");
            CurrentState = newState;

            // Handle internal manager logic per state
            switch (CurrentState)
            {
                case GameState.MainMenu:
                    HandleMainMenu();
                    break;
                case GameState.Playing:
                    HandlePlaying();
                    break;
                case GameState.GameOver:
                    HandleGameOver();
                    break;
                case GameState.Paused:
                    HandlePaused();
                    break;
            }

            Debug.Log($"[GameManager] State variables updated. Firing OnStateChanged({CurrentState}) to listeners.");
            // Notify all decoupled systems (UI, Audio, LevelGenerator) of the new state
            OnStateChanged?.Invoke(CurrentState);
        }


        private void HandlePlayerDeath()
        {
            Debug.Log("[GameManager] Event Received: GameEvents.OnPlayerDeath! Forcing state to GameOver.");
            // تحويل حالة اللعبة فوراً عند سماع خبر الموت[cite: 1]
            UpdateState(GameState.GameOver);
        }
        private void HandleMainMenu()
        {
            Time.timeScale = 1f;
        }

        private void HandlePlaying()
        {
            Time.timeScale = 1f;
        }

        private void HandleGameOver()
        {
            // Immediately freeze physics and standard Update timers
            Time.timeScale = 0f;
        }

        private void HandlePaused()
        {
            Time.timeScale = 0f;
        }

        /// <summary>
        /// Reloads the active scene and resets global dependencies safely.
        /// </summary>
        public void RestartGame()
        {
            Debug.Log("[GameManager] RestartGame initiated. Resetting TimeScale and reloading scene.");
            // Critical: Reset timescale before reloading so the new scene isn't frozen!
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        /// <summary>
        /// Toggles the game between Playing and Paused states.
        /// </summary>
        public void TogglePause()
        {
            if (CurrentState == GameState.Playing)
            {
                Time.timeScale = 0f;
                UpdateState(GameState.Paused);
                GameEvents.RaiseGamePaused();
            }
            else if (CurrentState == GameState.Paused)
            {
                Time.timeScale = 1f;
                UpdateState(GameState.Playing);
                GameEvents.RaiseGameResumed();
            }
        }

        /// <summary>
        /// Initiates a new game loop without needing a scene reload.
        /// </summary>
        public void StartNewGame()
        {
            Debug.Log("[GameManager] StartNewGame initiated.");
            Time.timeScale = 1f;
            
            // Fire event to decouple resetting logic (Score, Difficulty, Obstacles, Player)
            GameEvents.RaiseGameReset();
            
            UpdateState(GameState.Playing);
        }

        /// <summary>
        /// Safely returns the player to the main menu.
        /// </summary>
        public void ReturnToMainMenu()
        {
            Debug.Log("[GameManager] Returning to Main Menu.");
            Time.timeScale = 1f;
            UpdateState(GameState.MainMenu);
        }

        private void OnDestroy()
        {
            // Ensure static events are cleared on scene reload to prevent Ghost Listeners!
            OnStateChanged = null;
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}
