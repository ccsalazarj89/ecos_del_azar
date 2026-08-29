namespace EcosDelAzar.Core
{
    /// <summary>
    /// Per-run memory of each gambling table, keyed by the table id set on its
    /// MinigameEntryTrigger. A table remembers the last bet its dealer proposed —
    /// he recalls your face, not your money — and whether he has been beaten.
    /// His bankroll is NOT remembered: every seating is a closed duel.
    /// </summary>
    public static class TableState
    {
        const string Prefix = "table.";

        public static bool IsBeaten(string tableId) => RunPrefs.GetInt(Prefix + tableId + ".beaten", 0) == 1;

        public static void MarkBeaten(string tableId)
        {
            RunPrefs.SetInt(Prefix + tableId + ".beaten", 1);
            RunPrefs.Save();
        }

        /// <summary>Minimum bet the table now demands (the opponent's last proposal), or 0 for the table default.</summary>
        public static int GetMinimumBet(string tableId) => RunPrefs.GetInt(Prefix + tableId + ".minBet", 0);

        /// <summary>True when the player walked away with the dealer's offer still on the table.</summary>
        public static bool HasStandingProposal(string tableId) => RunPrefs.GetInt(Prefix + tableId + ".proposal", 0) == 1;

        public static void Save(string tableId, int minimumBet, bool standingProposal)
        {
            RunPrefs.SetInt(Prefix + tableId + ".minBet", minimumBet);
            RunPrefs.SetInt(Prefix + tableId + ".proposal", standingProposal ? 1 : 0);
            RunPrefs.Save();
        }
    }
}
