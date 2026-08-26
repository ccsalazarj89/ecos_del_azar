namespace EcosDelAzar.Core
{
    /// <summary>
    /// Per-run memory of each gambling table, keyed by the table id set on its
    /// MinigameEntryTrigger. A table remembers the opponent's bankroll and the
    /// last bet it proposed, so folding and walking back in does not reset the
    /// stakes; once the opponent is broke the table is closed for the run.
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

        /// <summary>Opponent coins left at this table, or -1 when the table has never been played.</summary>
        public static int GetOpponentCoins(string tableId) => RunPrefs.GetInt(Prefix + tableId + ".opponentCoins", -1);

        /// <summary>Minimum bet the table now demands (the opponent's last proposal), or 0 for the table default.</summary>
        public static int GetMinimumBet(string tableId) => RunPrefs.GetInt(Prefix + tableId + ".minBet", 0);

        public static void Save(string tableId, int opponentCoins, int minimumBet)
        {
            RunPrefs.SetInt(Prefix + tableId + ".opponentCoins", opponentCoins);
            RunPrefs.SetInt(Prefix + tableId + ".minBet", minimumBet);
            RunPrefs.Save();
        }
    }
}
