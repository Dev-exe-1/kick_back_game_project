using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    public class ObjectPoolManager : MonoBehaviour
    {
        public static ObjectPoolManager Instance { get; private set; }

        private Dictionary<EntityId, Queue<GameObject>> _poolDictionary = new Dictionary<EntityId, Queue<GameObject>>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                // Removed DontDestroyOnLoad to avoid stale references.
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void OnDestroy()
        {
            // Clear dictionary to avoid holding destroyed scene objects.
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


            EntityId instanceId = prefab.GetEntityId();
            if (!_poolDictionary.ContainsKey(instanceId))
            {
                _poolDictionary.Add(instanceId, new Queue<GameObject>());
            }

            while (_poolDictionary[instanceId].Count > 0)
            {
                GameObject objToSpawn = _poolDictionary[instanceId].Dequeue();

                if (objToSpawn != null)
                {
                    objToSpawn.transform.position = position;
                    objToSpawn.transform.rotation = rotation;
                    objToSpawn.SetActive(true);
                    return objToSpawn;
                }
            }

            GameObject newObj = Instantiate(prefab, position, rotation);

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

    public class PooledObject : MonoBehaviour
    {
        public EntityId PrefabId { get; set; }
    }
}
