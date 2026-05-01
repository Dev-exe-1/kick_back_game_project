using UnityEngine;
using Core;

namespace Features.Platforms.Scripts
{
    public class Platform : MonoBehaviour, IGroundable
    {

        public virtual void OnLanded(GameObject player)
        {
        }
    }
}