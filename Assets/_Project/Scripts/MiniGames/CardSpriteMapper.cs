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

            string spriteName;

            if (card.Rank == Rank.Joker)
            {
                spriteName = (card.Suit == Suit.None || card.Suit == Suit.Spades || card.Suit == Suit.Clubs)
                    ? "BLACK_JOKER" : "RED_JOKER";
            }
            else
            {
                if (!SuitNames.TryGetValue(card.Suit, out string suit) ||
                    !RankNames.TryGetValue(card.Rank, out string rank))
                    return null;
                spriteName = $"{suit}_{rank}";
            }

            foreach (var sprite in cardSprites)
                if (sprite.name == spriteName)
                    return sprite;

            return null;
        }
    }
}
