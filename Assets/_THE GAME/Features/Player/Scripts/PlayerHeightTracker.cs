using UnityEngine;
using Core;

namespace Player
{
    /// <summary>
    /// Tracks the player's height and fires an event when it increases.
    /// Acts as the source of height data, completely decoupled from the score logic.
    /// </summary>
    public class PlayerHeightTracker : MonoBehaviour
    {
        private int lastReportedHeight = 0;
        private float _startHeight;

        private void Start()
        {
            CaptureStartHeight();
        }

        private void CaptureStartHeight()
        {
            _startHeight = transform.position.y;
        }

        private void Update()
        {
            // State-Gate: Track height only while playing
            if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Playing)
            {
                float relativeHeight = transform.position.y - _startHeight;
                int currentHeight = Mathf.Max(0, Mathf.FloorToInt(relativeHeight));

                // Optimization: Only trigger event when the floor-to-int value increases
                if (currentHeight > lastReportedHeight)
                {
                    lastReportedHeight = currentHeight;
                    GameEvents.RaisePlayerHeightChanged(currentHeight);
                }
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

        private void HandleGameStateChanged(GameState state)
        {
            // Reset the tracker when returning to the Main Menu
            if (state == GameState.MainMenu)
            {
                lastReportedHeight = 0;
                CaptureStartHeight();
            }
        }
    }
}
