using System;

namespace EcosDelAzar.Opponent
{
    /// <summary>
    /// Context passed to an opponent when requesting a bet decision.
    /// Using a struct avoids allocations and is easy to extend with new fields.
    /// </summary>
    public readonly struct BetContext
    {
        public readonly int PlayerBet;
        public readonly int MinimumBet;
        public readonly int OwnCoins;

        public BetContext(int playerBet, int minimumBet, int ownCoins)
        {
            PlayerBet = playerBet;
            MinimumBet = minimumBet;
            OwnCoins = ownCoins;
        }
    }

    /// <summary>
    /// Abstraction for any opponent — AI, local human, or networked player.
    /// 
    /// All decisions use callbacks (Action) instead of return values so that:
    /// - AI can respond instantly (call the callback synchronously).
    /// - A local player can wait for input and call it when ready.
    /// - A networked player can send a request and call it when the response arrives.
    /// 
    /// The game systems (BettingSystem, MiniGameTable) never know or care
    /// what kind of opponent they're dealing with.
    /// </summary>
    public interface IOpponent
    {
        /// <summary>Current coin count for this opponent.</summary>
        int Coins { get; set; }

        /// <summary>
        /// Request the opponent to decide their bet amount.
        /// MUST call onDecided exactly once with the chosen bet.
        /// </summary>
        void RequestBet(BetContext context, Action<int> onDecided);

        /// <summary>Reset state for a new session.</summary>
        void ResetSession();
    }
}
