using System.Collections.Generic;

namespace EcosDelAzar.MiniGames.Blackjack
{
    /// <summary>
    /// A hand of cards with Blackjack scoring rules:
    /// - 2..10 = pip value
    /// - J, Q, K = 10
    /// - Ace = 11, or 1 if that avoids a bust
    /// </summary>
    public class BlackjackHand
    {
        readonly List<Card> cards = new();

        public IReadOnlyList<Card> Cards => cards;
        public int Score => ComputeScore();
        public bool IsBust => Score > 21;
        public bool IsBlackjack => cards.Count == 2 && Score == 21;
        public int Count => cards.Count;

        public void Add(Card card) => cards.Add(card);
        public void Clear() => cards.Clear();

        int ComputeScore()
        {
            int total = 0;
            int aces = 0;

            foreach (var card in cards)
            {
                int v = CardValue(card);
                if (v == 11) aces++;
                total += v;
            }

            while (total > 21 && aces > 0)
            {
                total -= 10;
                aces--;
            }

            return total;
        }

        public static int CardValue(Card card)
        {
            if (card == null) return 0;
            return card.Rank switch
            {
                Rank.Ace => 11,
                Rank.King or Rank.Queen or Rank.Jack => 10,
                _ => (int)card.Rank
            };
        }
    }
}
