using System.Collections.Generic;
using UnityEngine;
using Core;

namespace Features.LevelGenerator.Scripts
{
    /// <summary>Manages infinite generation and pooling of vertical level chunks.</summary>
    public class LevelGenerator : MonoBehaviour
    {
        [Header("Chunk Configurations")]
        [Tooltip("Index 0 MUST be the starting chunk. Indices 1 to N are random vertical chunks.")]
        [SerializeField] private List<GameObject> _chunkPrefabs;
        
        [Header("Generation Settings")]
        [Tooltip("Optional: Manually placed starting chunk in the scene.")]
        [SerializeField] private LevelChunk _initialChunk;

        [Tooltip("Reference to the player to calculate relative spawn/despawn distances.")]
        [SerializeField] private Transform _playerTransform;
        
        [Tooltip("Number of chunks to keep loaded ahead of the player at the start.")]
        [SerializeField] private int _initialChunksToSpawn = 3;
        
        [Tooltip("How far above the player should we generate new chunks?")]
        [SerializeField] private float _spawnThreshold = 20f;
        
        [Tooltip("How far below the player should chunks be recycled back to the pool?")]
        [SerializeField] private float _despawnThreshold = 15f;

        private Queue<GameObject> _activeChunks = new Queue<GameObject>();
        private LevelChunk _lastSpawnedChunk;

        private void Start()
        {
            if (_chunkPrefabs == null || _chunkPrefabs.Count == 0)
            {
                Debug.LogError("LevelGenerator has no chunk prefabs assigned!");
                return;
            }

            if (_initialChunk != null)
            {
                _activeChunks.Enqueue(_initialChunk.gameObject);
                _lastSpawnedChunk = _initialChunk;
            }
            else
            {
                SpawnSpecificChunk(0);
            }

            int chunksToSpawn = _initialChunk != null ? _initialChunksToSpawn : _initialChunksToSpawn - 1;
            for (int i = 0; i < chunksToSpawn; i++)
            {
                SpawnRandomChunk();
            }
        }

        private void Update()
        {
            if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing) return;
            if (_playerTransform == null) return;

            ManageChunkSpawning();
            ManageChunkRecycling();
        }

        /// <summary>Checks if the player is near highest chunk to request new one.</summary>
        private void ManageChunkSpawning()
        {
            float checkY = _lastSpawnedChunk != null && _lastSpawnedChunk.EndPoint != null ? _lastSpawnedChunk.EndPoint.position.y : 0f;
            
            if (checkY - _playerTransform.position.y < _spawnThreshold)
            {
                SpawnRandomChunk();
            }
        }

        /// <summary>Recycles oldest chunk if below despawn threshold.</summary>
        private void ManageChunkRecycling()
        {
            if (_activeChunks.Count == 0) return;

            GameObject oldestChunk = _activeChunks.Peek();
            
            if (_playerTransform.position.y - oldestChunk.transform.position.y > _despawnThreshold)
            {
                oldestChunk = _activeChunks.Dequeue();

                // Handle manual scene objects vs pooled objects.
                if (oldestChunk.GetComponent<PooledObject>() != null)
                {
                    ObjectPoolManager.Instance.ReturnToPool(oldestChunk);
                }
                else
                {
                    // Only destroy instantiated scene objects.
                    if (oldestChunk.scene.IsValid())
                    {
                        Destroy(oldestChunk);
                    }
                }
            }
        }

        /// <summary>Spawns specific chunk by index.</summary>
        private void SpawnSpecificChunk(int index)
        {
            if (index < 0 || index >= _chunkPrefabs.Count) return;
            SpawnChunk(_chunkPrefabs[index]);
        }

        /// <summary>Spawns random chunk excluding starter.</summary>
        private void SpawnRandomChunk()
        {
            if (_chunkPrefabs.Count <= 1) return;

            int randomIndex = Random.Range(1, _chunkPrefabs.Count);
            SpawnChunk(_chunkPrefabs[randomIndex]);
        }

        /// <summary>Handles dependency injection and snapping.</summary>
        private void SpawnChunk(GameObject prefab)
        {
            Vector3 spawnPos = Vector3.zero;
            
            if (_lastSpawnedChunk != null)
            {
                if (_lastSpawnedChunk.EndPoint != null)
                {
                    spawnPos = _lastSpawnedChunk.EndPoint.position;
                }
                else
                {
                    Debug.LogWarning($"Chunk {_lastSpawnedChunk.name} has no EndPoint! Snapping may fail.");
                    spawnPos = _lastSpawnedChunk.transform.position + Vector3.up * 10f;
                }
            }

            GameObject newChunkObj = ObjectPoolManager.Instance.GetFromPool(prefab, spawnPos, Quaternion.identity);
            
            _activeChunks.Enqueue(newChunkObj);

            LevelChunk levelChunk = newChunkObj.GetComponent<LevelChunk>();
            if (levelChunk != null)
            {
                _lastSpawnedChunk = levelChunk;
            }
            else
            {
                Debug.LogWarning($"Chunk {newChunkObj.name} is missing a LevelChunk script! Sequence broken.");
            }
        }
    }
}
