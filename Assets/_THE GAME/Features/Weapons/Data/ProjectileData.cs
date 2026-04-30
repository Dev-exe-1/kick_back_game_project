using UnityEngine;

namespace Features.Weapons.Data
{
    [CreateAssetMenu(fileName = "NewProjectileData", menuName = "Weapons/ProjectileData")]
    public class ProjectileData : ScriptableObject
    {
        public GameObject prefab;
        public float speed = 20f;
        public float lifetime = 2f;
        public LayerMask hittableLayers;
        public float linearDrag = 0f;

    }
}
