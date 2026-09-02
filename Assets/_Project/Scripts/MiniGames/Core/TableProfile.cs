using UnityEngine;
using EcosDelAzar.AI;

namespace EcosDelAzar.MiniGames
{
    /// <summary>
    /// Who sits across the table and how the table plays. One asset per table
    /// type and floor (dice on floor 1, blackjack on floor 2, the boss...).
    /// When several dealers are listed, each table picks one by its id, so two
    /// dice tables on the same floor host different opponents.
    /// </summary>
    [CreateAssetMenu(fileName = "SO_TableProfile", menuName = "Ecos del Azar/Table Profile")]
    public class TableProfile : ScriptableObject
    {
        [System.Serializable]
        public class Dealer
        {
            public string displayName = "Rival";
            public int startingCoins = 1000;
            public StandardAIBrain brain;
            [Tooltip("RPS only: chance the dealer picks the winning hand after seeing yours.")]
            [Range(0f, 1f)] public float cheatChance;
            [Tooltip("Blackjack only: score at or above which the dealer stops. 17 is the house-optimal rule; 16 and 18 both favour the player.")]
            [Range(15, 21)] public int dealerStandsAt = 17;
        }

        [SerializeField] int minimumBet = 10;
        [SerializeField] Dealer[] dealers = { new Dealer() };

        public int MinimumBet => minimumBet;

        public Dealer DealerFor(string tableId)
        {
            if (dealers == null || dealers.Length == 0) return null;
            int hash = 0;
            foreach (char c in tableId ?? string.Empty) hash = hash * 31 + c;
            return dealers[Mathf.Abs(hash) % dealers.Length];
        }
    }
}
