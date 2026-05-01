using UnityEngine;
using Core;

namespace Player
{
    /// <summary>Tracks player height and fires an event when it increases.</summary>
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
            if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Playing)
            {
                float relativeHeight = transform.position.y - _startHeight;
                int currentHeight = Mathf.Max(0, Mathf.FloorToInt(relativeHeight));

                // Trigger event only when floor-to-int value increases to save cycles.
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
            if (state == GameState.MainMenu)
            {
                lastReportedHeight = 0;
                CaptureStartHeight();
            }
        }
    }
}
