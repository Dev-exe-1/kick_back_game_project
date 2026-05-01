using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Core
{
    /// <summary>Represents the high-level flow of the application.</summary>
    public enum GameState
    {
        MainMenu,
        Playing,
        GameOver,
        Paused
    }

    /// <summary>Singleton manager handling global states, time scaling, and scene transitions.</summary>
    public class GameManager : MonoBehaviour
    {
        /// <summary>Standard Singleton instance access.</summary>
        public static GameManager Instance { get; private set; }

        /// <summary>Observer event fired when GameState changes.</summary>
        public static event Action<GameState> OnStateChanged;

        /// <summary>Current state of the game.</summary>
        public GameState CurrentState { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            UpdateState(GameState.MainMenu);
        }
        private void OnEnable()
        {
            GameEvents.OnPlayerDeath += HandlePlayerDeath;
        }

        private void OnDisable()
        {
            GameEvents.OnPlayerDeath -= HandlePlayerDeath;
        }

        /// <summary>Transitions game state and broadcasts changes.</summary>
        public void UpdateState(GameState newState)
        {
            Debug.Log($"[GameManager] UpdateState requested. Transitioning from {CurrentState} to {newState}");
            CurrentState = newState;

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
            OnStateChanged?.Invoke(CurrentState);
        }


        private void HandlePlayerDeath()
        {
            Debug.Log("[GameManager] Event Received: GameEvents.OnPlayerDeath! Forcing state to GameOver.");
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
            Time.timeScale = 0f;
        }

        private void HandlePaused()
        {
            Time.timeScale = 0f;
        }

        /// <summary>Reloads active scene and resets dependencies.</summary>
        public void RestartGame()
        {
            Debug.Log("[GameManager] RestartGame initiated. Resetting TimeScale and reloading scene.");
            // Reset timescale before reloading to prevent frozen scene.
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        /// <summary>Toggles game between Playing and Paused.</summary>
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

        /// <summary>Initiates new game loop without scene reload.</summary>
        public void StartNewGame()
        {
            Debug.Log("[GameManager] StartNewGame initiated.");
            Time.timeScale = 1f;
            
            GameEvents.RaiseGameReset();
            
            UpdateState(GameState.Playing);
        }

        /// <summary>Returns player to main menu.</summary>
        public void ReturnToMainMenu()
        {
            Debug.Log("[GameManager] Returning to Main Menu.");
            Time.timeScale = 1f;
            UpdateState(GameState.MainMenu);
        }

        private void OnDestroy()
        {
            // Clear static events on reload to prevent ghost listeners.
            OnStateChanged = null;
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}
