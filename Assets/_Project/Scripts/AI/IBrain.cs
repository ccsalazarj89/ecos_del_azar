using EcosDelAzar.MiniGames;

namespace EcosDelAzar.AI
{
    /// <summary>
    /// Abstraction for NPC decision-making.
    /// </summary>
    public interface IBrain
    {
        /// <summary>
        /// Decides how much the NPC should bet, given the current game state.
        /// </summary>
        /// <param name="opponentBet">The player's current bet.</param>
        /// <param name="minimumBet">The table's minimum bet.</param>
        /// <param name="ownCoins">How many coins the NPC has left.</param>
        /// <returns>The NPC's chosen bet amount.</returns>
        int DecideBet(int opponentBet, int minimumBet, int ownCoins);

        /// <summary>
        /// Decides which combat action the NPC should take.
        /// </summary>
        /// <param name="winProbability">Estimated probability of winning (0..1).</param>
        /// <param name="ownCoins">NPC's remaining coins.</param>
        /// <param name="currentBet">The current round bet.</param>
        /// <returns>The NPC's chosen combat action.</returns>
        CombatAction DecideAction(float winProbability, int ownCoins, int currentBet);
    }
}
