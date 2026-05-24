namespace EcosDelAzar.MiniGames
{
    /// <summary>
    /// The 5 strategic actions available during the combat phase of a minigame round.
    /// </summary>
    public enum CombatAction
    {
        /// <summary>Plantarse — safe play, keep current bet.</summary>
        Stand,

        /// <summary>Forzar la suerte — risk/reward: re-roll or draw a new card.</summary>
        PushLuck,

        /// <summary>Subir la apuesta — double the current bet.</summary>
        RaiseBet,

        /// <summary>Blindarse — mitigate damage if you lose (50% reduction).</summary>
        Shield,

        /// <summary>Retirarse — minimal loss to avoid a fatal blow.</summary>
        Fold
    }
}
