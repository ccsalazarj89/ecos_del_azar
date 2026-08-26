using System.Collections.Generic;

namespace EcosDelAzar.MiniGames.Boss
{
    /// <summary>
    /// What the boss layer (oxygen suit rules, forced win) needs from a minigame,
    /// so the same BossOxygenModifier works over High Card or Blackjack.
    /// </summary>
    public interface IBossGame
    {
        /// <summary>Cards the player held when the round resolved.</summary>
        IReadOnlyList<Card> PlayerRoundCards { get; }

        /// <summary>True when the player lost by their own decision (bust in Blackjack).</summary>
        bool PlayerBusted { get; }

        bool IsForceWinQueued { get; }
        void QueueForceWin();
    }
}
