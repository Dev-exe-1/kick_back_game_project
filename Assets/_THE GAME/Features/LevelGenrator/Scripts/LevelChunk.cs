using UnityEngine;

namespace Features.Level.Scripts
{
    /// <summary>
    /// Represents a single piece of the infinite level.
    /// Holds a reference to the EndPoint where the next chunk will be connected.
    /// </summary>
    public class LevelChunk : MonoBehaviour
    {
        [Tooltip("The Transform representing the top/end of this chunk where the next one will snap.")]
        [SerializeField] private Transform _endPoint;

        /// <summary>
        /// The Transform representing where the next chunk should spawn to connect seamlessly.
        /// </summary>
        public Transform EndPoint => _endPoint;
    }
}
