using UnityEngine;

namespace Features.LevelGenerator.Scripts
{
    /// <summary>Represents a single piece of the infinite level.</summary>
    public class LevelChunk : MonoBehaviour
    {
        [Tooltip("The Transform representing the top/end of this chunk where the next one will snap.")]
        [SerializeField] private Transform _endPoint;

        /// <summary>Transform where the next chunk connects.</summary>
        public Transform EndPoint => _endPoint;
    }
}
