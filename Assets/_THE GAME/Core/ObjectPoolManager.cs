using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    public class ObjectPoolManager : MonoBehaviour
    {
        public static ObjectPoolManager Instance { get; private set; }

        private Dictionary<int, Queue<GameObject>> _poolDictionary = new Dictionary<int, Queue<GameObject>>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                // Removed DontDestroyOnLoad to ensure per-scene fresh start, avoiding stale references.
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void OnDestroy()
        {
            // Crucial: Clear dictionary to avoid holding onto destroyed scene objects
            if (Instance == this)
            {
                _poolDictionary.Clear();
                Instance = null;
            }
        }

        public GameObject GetFromPool(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            if (prefab == null)
            {
                Debug.LogError("[ObjectPoolManager] Attempted to spawn a null prefab!");
                return null;
            }

            // Using GetInstanceID() is the native Unity way, replacing GetEntityId if it was a typo
            int instanceId = prefab.GetInstanceID();

            if (!_poolDictionary.ContainsKey(instanceId))
            {
                _poolDictionary.Add(instanceId, new Queue<GameObject>());
            }

            // Cleanly handle MissingReferenceException by discarding destroyed (null) objects
            while (_poolDictionary[instanceId].Count > 0)
            {
                GameObject objToSpawn = _poolDictionary[instanceId].Dequeue();
                
                // If it's not null, it's safe to use
                if (objToSpawn != null)
                {
                    objToSpawn.transform.position = position;
                    objToSpawn.transform.rotation = rotation;
                    objToSpawn.SetActive(true);
                    return objToSpawn;
                }
            }

            // If queue was empty or only contained nulls, instantiate a new one
            GameObject newObj = Instantiate(prefab, position, rotation);

            // Add a helper component to know which pool it belongs to when returning
            PooledObject pooledObj = newObj.AddComponent<PooledObject>();
            pooledObj.PrefabId = instanceId;

            return newObj;
        }

        public void ReturnToPool(GameObject obj)
        {
            PooledObject pooledObj = obj.GetComponent<PooledObject>();
            if (pooledObj != null)
            {
                obj.SetActive(false);
                if (_poolDictionary.ContainsKey(pooledObj.PrefabId))
                {
                    _poolDictionary[pooledObj.PrefabId].Enqueue(obj);
                }
                else
                {
                    Debug.LogWarning("Pool not found for returning object. Destroying instead.");
                    Destroy(obj);
                }
            }
            else
            {
                Debug.LogWarning("Attempted to return a non-pooled object to the ObjectPoolManager.");
                Destroy(obj);
            }
        }
    }

    // Helper component attached dynamically to spawned objects
    public class PooledObject : MonoBehaviour
    {
        public int PrefabId { get; set; }
    }
}
