using UnityEngine;

namespace Core
{
    public interface IGroundable
    {
        void OnLanded(GameObject player);
    }
}
