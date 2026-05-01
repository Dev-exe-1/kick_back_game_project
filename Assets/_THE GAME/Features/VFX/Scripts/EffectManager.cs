using System;
using System.Collections.Generic;
using UnityEngine;
using Core;
using Features.Player.Scripts;
using Features.VFX.Data;

namespace Features.VFX.Scripts
{
    [Serializable]
    public struct EffectMapping
    {
        public ImpactType impactType;
        public GameObject prefab;
    }

    /// <summary>Listens for impact events and spawns pooled particles.</summary>
    public class EffectManager : MonoBehaviour
    {
        [SerializeField] private List<EffectMapping> _effectMappings;
        private Dictionary<ImpactType, GameObject> _effectDictionary;

        private void Awake()
        {
            _effectDictionary = new Dictionary<ImpactType, GameObject>();
            
            // Build dictionary for O(1) lookups.
            if (_effectMappings != null)
            {
                foreach (var mapping in _effectMappings)
                {
                    if (!_effectDictionary.ContainsKey(mapping.impactType))
                    {
                        _effectDictionary.Add(mapping.impactType, mapping.prefab);
                    }
                }
            }
        }

        private void OnEnable()
        {
            PlayerEvents.OnImpactRequested += SpawnEffect;
        }

        private void OnDisable()
        {
            PlayerEvents.OnImpactRequested -= SpawnEffect;
        }

        private void SpawnEffect(ImpactType type, Vector3 position, Quaternion rotation)
        {
            if (_effectDictionary.TryGetValue(type, out GameObject prefab) && prefab != null)
            {
                ObjectPoolManager.Instance.GetFromPool(prefab, position, rotation);
            }
        }
    }
}
