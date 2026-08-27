using UnityEngine;
using EcosDelAzar.MiniGames;

namespace EcosDelAzar.AI
{
    /// <summary>
    /// Standard AI brain driven by an AIBrainConfig ScriptableObject.
    /// Can be assigned to any NPC table via the Inspector.
    /// </summary>
    [CreateAssetMenu(fileName = "SO_StandardAIBrain", menuName = "Ecos del Azar/AI/Standard Brain")]
    public class StandardAIBrain : ScriptableObject, IBrain
    {
        [SerializeField] AIBrainConfig config;

        public int DecideBet(int opponentBet, int minimumBet, int ownCoins)
        {
            if (ownCoins <= 0) return 0;

            float roll = Random.value;
            int maxProposal = Mathf.Min(opponentBet * 2, ownCoins);

            // Aggressive: raise (capped at double)
            if (roll < config.aggressiveness)
            {
                int raised = Mathf.RoundToInt(opponentBet * Mathf.Lerp(1.2f, 2f, config.aggressiveness));
                return Mathf.Clamp(raised, minimumBet, maxProposal);
            }

            // Bluff: slight raise
            if (roll < config.aggressiveness + config.bluffFrequency)
            {
                int bluffBet = Mathf.RoundToInt(opponentBet * 1.5f);
                return Mathf.Clamp(bluffBet, minimumBet, maxProposal);
            }

            // Default: match the previous bet
            return Mathf.Clamp(opponentBet, minimumBet, ownCoins);
        }
    }
}
