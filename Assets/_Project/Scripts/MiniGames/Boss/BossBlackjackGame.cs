using System.Collections.Generic;
using EcosDelAzar.MiniGames.Blackjack;

namespace EcosDelAzar.MiniGames.Boss
{
    /// <summary>Blackjack against the boss: same rules as the floor game, exposed to the boss layer.</summary>
    public class BossBlackjackGame : BlackjackGame, IBossGame
    {
        public IReadOnlyList<Card> PlayerRoundCards => PlayerHand.Cards;
        public bool PlayerBusted => PlayerHand.IsBust;
    }
}
