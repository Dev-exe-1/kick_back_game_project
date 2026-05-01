using UnityEngine;

namespace Core
{
    /// <summary>
    /// ScriptableObject defining the difficulty scaling parameters.
    /// Allows designers to tune the "feel" from a single asset.
    /// </summary>
    [CreateAssetMenu(fileName = "DifficultySettings", menuName = "KickBack/Difficulty Settings")]
    public class DifficultySettings : ScriptableObject
    {
        [Tooltip("The base speed multiplier applied at the start of a run.")]
        public float baseSpeed = 1f;

        [Tooltip("The maximum speed multiplier allowed.")]
        public float maxSpeedMultiplier = 3f;

        [Tooltip("How much the multiplier increases per meter (score). e.g., 0.01 means 1% increase per meter.")]
        public float speedIncreasePerMeter = 0.01f;
    }
}
