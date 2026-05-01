using System;

namespace Core
{
    [Serializable]
    public class GameSaveData
    {
        public int highScore;
        public string playerName;

        public GameSaveData()
        {
            highScore = 0;
            playerName = "Player";
        }
    }
}
