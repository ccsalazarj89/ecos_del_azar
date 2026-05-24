using System.Collections.Generic;
using UnityEngine;

namespace EcosDelAzar.MiniGames
{
    public class CardSpriteMapper : MonoBehaviour
    {
        [SerializeField] Sprite[] cardSprites;

        static readonly Dictionary<Rank, string> RankNames = new()
        {
            { Rank.Two, "TWO" }, { Rank.Three, "THREE" }, { Rank.Four, "FOUR" },
            { Rank.Five, "FIVE" }, { Rank.Six, "SIX" }, { Rank.Seven, "SEVEN" },
            { Rank.Eight, "EIGHT" }, { Rank.Nine, "NINE" }, { Rank.Ten, "TEN" },
            { Rank.Jack, "J" }, { Rank.Queen, "Q" }, { Rank.King, "K" }, { Rank.Ace, "ACE" }
        };

        static readonly Dictionary<Suit, string> SuitNames = new()
        {
            { Suit.Hearts, "HEARTS" }, { Suit.Diamonds, "DIAMONDS" },
            { Suit.Clubs, "CLUBS" }, { Suit.Spades, "SPADES" }
        };

        public Sprite GetSprite(Card card)
        {
            if (card == null) return null;

            string name = card.Rank == Rank.Joker
                ? (card.Suit is Suit.None or Suit.Spades or Suit.Clubs ? "BLACK_JOKER" : "RED_JOKER")
                : SuitNames.TryGetValue(card.Suit, out var s) && RankNames.TryGetValue(card.Rank, out var r)
                    ? $"{s}_{r}"
                    : null;

            if (name == null) return null;

            foreach (var sprite in cardSprites)
                if (sprite.name == name)
                    return sprite;

            return null;
        }
    }
}
