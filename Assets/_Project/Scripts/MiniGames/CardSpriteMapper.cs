using System.Collections.Generic;
using EcosDelAzar.Match;
using UnityEngine;

/// <summary>
/// Mapea suit+rank al nombre del sprite correspondiente en el spritesheet.
/// </summary>
public class CardSpriteMapper : MonoBehaviour
{
    [Header("Spritesheet")]
    public Sprite[] cardSprites;

    // Rank → sufijo en el sprite
    private static readonly Dictionary<Rank, string> RankMap = new()
    {
        { Rank.TWO,   "TWO"   },
        { Rank.THREE, "THREE" },
        { Rank.FOUR,  "FOUR"  },
        { Rank.FIVE,  "FIVE"  },
        { Rank.SIX,   "SIX"   },
        { Rank.SEVEN, "SEVEN" },
        { Rank.EIGHT, "EIGHT" },
        { Rank.NINE,  "NINE"  },
        { Rank.TEN,   "TEN"   },
        { Rank.JACK,  "J"     },
        { Rank.QUEEN, "Q"     },
        { Rank.KING,  "K"     },
        { Rank.ACE,   "ACE"   },
    };

    // Suit → prefijo en el sprite
    private static readonly Dictionary<Suit, string> SuitMap = new()
    {
        { Suit.HEARTS,   "HEARTS"   },
        { Suit.DIAMONDS, "DIAMONDS" },
        { Suit.CLUBS,    "CLUBS"    },
        { Suit.SPADES,   "SPADES"   },
    };

    public Sprite GetSprite(Card card)
    {
        if (card == null) return null;

        string spriteName;

        if (card.Rank == Rank.JOKER)
        {
            spriteName = (card.Suit == Suit.NONE || card.Suit == Suit.SPADES || card.Suit == Suit.CLUBS)
                ? "BLACK_JOKER"
                : "RED_JOKER";
        }
        else
        {
            if (!SuitMap.TryGetValue(card.Suit, out string suit) ||
                !RankMap.TryGetValue(card.Rank, out string rank))
            {
                Debug.LogWarning($"[CardSpriteMapper] Combinación no reconocida: {card.Suit}_{card.Rank}");
                return null;
            }
            spriteName = $"{suit}_{rank}";
        }

        foreach (var sprite in cardSprites)
        {
            if (sprite.name == spriteName)
                return sprite;
        }

        Debug.LogWarning($"[CardSpriteMapper] Sprite no encontrado: '{spriteName}'");
        return null;
    }
}
