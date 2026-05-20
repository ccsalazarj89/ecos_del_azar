using UnityEngine;

namespace EcosDelAzar.Betting
{
    /// <summary>
    /// IA simple del NPC para decidir su apuesta.
    /// Estrategia basada en el estado actual de sus fichas.
    /// </summary>
    public static class NpcBettingAI
    {
        public static int DecideBet(int npcChips, int playerBet, int minimumBet)
        {
            // Agresivo: muchas fichas → apuesta fuerte
            if (npcChips > 500)
            {
                float roll = Random.value;
                if (roll < 0.20f) return npcChips;              // All-in
                if (roll < 0.55f) return Mathf.Min(playerBet * 2, npcChips); // Doblar
                return Mathf.Min(playerBet, npcChips);           // Igual
            }

            // Moderado: fichas equilibradas → apuesta conservadora
            if (npcChips > 200)
            {
                float roll = Random.value;
                if (roll < 0.10f) return npcChips;              // All-in (raro)
                if (roll < 0.35f) return Mathf.Min(playerBet * 2, npcChips); // Doblar
                return Mathf.Min(playerBet, npcChips);           // Igual
            }

            // Desesperado: pocas fichas → all-in o mínimo
            {
                float roll = Random.value;
                if (roll < 0.40f) return npcChips;              // All-in desesperado
                return Mathf.Min(minimumBet, npcChips);          // Mínimo para sobrevivir
            }
        }
    }
}
