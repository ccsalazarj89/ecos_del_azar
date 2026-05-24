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
            // Umbrales relativos al total de fichas en juego para que escalen con la partida
            int totalChips = npcChips + playerBet; // aproximación del bote total
            float ratio = totalChips > 0 ? (float)npcChips / totalChips : 0.5f;

            // Agresivo: NPC tiene más del 50% de las fichas totales
            if (ratio > 0.50f)
            {
                float roll = Random.value;
                if (roll < 0.20f) return npcChips;                              // All-in
                if (roll < 0.55f) return Mathf.Min(playerBet * 2, npcChips);   // Doblar
                return Mathf.Min(playerBet, npcChips);                           // Igualar
            }

            // Moderado: NPC tiene entre el 25% y el 50%
            if (ratio > 0.25f)
            {
                float roll = Random.value;
                if (roll < 0.10f) return npcChips;                              // All-in (raro)
                if (roll < 0.35f) return Mathf.Min(playerBet * 2, npcChips);   // Doblar
                return Mathf.Min(playerBet, npcChips);                           // Igualar
            }

            // Desesperado: NPC tiene menos del 25% → all-in o mínimo
            {
                float roll = Random.value;
                if (roll < 0.40f) return npcChips;                              // All-in desesperado
                return Mathf.Min(minimumBet, npcChips);                          // Mínimo para sobrevivir
            }
        }
    }
}
