using UnityEngine;

namespace EcosDelAzar.MiniGames.Betting
{
    public static class NpcBettingAI
    {
        public static int DecideBet(int npcChips, int playerBet, int minimumBet)
        {
            float roll = Random.value;

            // Aggressive when rich
            if (npcChips > 500)
            {
                if (roll < 0.20f) return npcChips;
                if (roll < 0.55f) return Mathf.Min(playerBet * 2, npcChips);
                return Mathf.Min(playerBet, npcChips);
            }

            // Conservative when moderate
            if (npcChips > 200)
            {
                if (roll < 0.10f) return npcChips;
                if (roll < 0.35f) return Mathf.Min(playerBet * 2, npcChips);
                return Mathf.Min(playerBet, npcChips);
            }

            // Desperate when low
            if (roll < 0.40f) return npcChips;
            return Mathf.Min(minimumBet, npcChips);
        }
    }
}
