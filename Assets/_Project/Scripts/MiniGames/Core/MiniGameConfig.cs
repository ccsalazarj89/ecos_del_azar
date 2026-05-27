using UnityEngine;

namespace EcosDelAzar.MiniGames
{
    [CreateAssetMenu(fileName = "SO_MiniGameConfig", menuName = "Ecos del Azar/MiniGame Config")]
    public class MiniGameConfig : ScriptableObject
    {
        public string gameId;
        public string displayName;
        [TextArea] public string description;
        public int minimumBet = 10;
        public float baseDifficulty = 1f;
    }
}
