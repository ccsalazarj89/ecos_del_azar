
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
        /// <param name="startingCoins">Coins the NPC sat down with (its "air level" is ownCoins / startingCoins).</param>
        /// <returns>The NPC's chosen bet amount.</returns>
        int DecideBet(int opponentBet, int minimumBet, int ownCoins, int startingCoins);
    }
}
