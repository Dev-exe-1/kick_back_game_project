using UnityEngine;

namespace Features.Player.Data
{
    [CreateAssetMenu(fileName = "NewPlayerStats", menuName = "Player/PlayerStats")]
    public class PlayerStats : ScriptableObject
    {
        public float recoilForce = 15f;
        public int maxAirShots = 1;
        public LayerMask groundLayer;
    }
}
