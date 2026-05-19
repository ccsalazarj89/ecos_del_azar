using System.Collections.Generic;
using EcosDelAzar.Match;
using UnityEngine;

/// <summary>
/// Mapea suit+rank de la API al nombre del sprite correspondiente en el spritesheet.
/// </summary>
public class CardSpriteMapper : MonoBehaviour
{
    [Header("Spritesheet")]
    public Sprite[] cardSprites; // Arrastra aquí todos los sprites del spritesheet

    // Traducción de ranks de la API → sufijo en el sprite
    private static readonly Dictionary<string, string> RankMap = new()
    {
        { "TWO",   "TWO"   },
        { "THREE", "THREE" },
        { "FOUR",  "FOUR"  },
        { "FIVE",  "FIVE"  },
        { "SIX",   "SIX"   },
        { "SEVEN", "SEVEN" },
        { "EIGHT", "EIGHT" },
        { "NINE",  "NINE"  },
        { "TEN",   "TEN"   },
        { "JACK",  "J"     },
        { "QUEEN", "Q"     },
        { "KING",  "K"     },
        { "ACE",   "ACE"   },
    };

    // Traducción de suits de la API → prefijo en el sprite
    private static readonly Dictionary<string, string> SuitMap = new()
    {
        { "HEARTS",   "HEARTS"   },
        { "DIAMONDS", "DIAMONDS" },
        { "CLUBS",    "CLUBS"    },
        { "SPADES",   "SPADES"   },
    };

    public Sprite GetSprite(CardDto card)
    {
        if (card == null) return null;

        string spriteName;

        // Joker
        if (card.rank == "JOKER")
        {
            spriteName = (card.suit == "NONE" || card.suit == "SPADES" || card.suit == "CLUBS")
                ? "BLACK_JOKER"
                : "RED_JOKER";
        }
        else
        {
            if (!SuitMap.TryGetValue(card.suit, out string suit) ||
                !RankMap.TryGetValue(card.rank, out string rank))
            {
                Debug.LogWarning($"[CardSpriteMapper] Combinación no reconocida: {card.suit}_{card.rank}");
                return null;
            }
            spriteName = $"{suit}_{rank}";
        }

        // Buscar el sprite por nombre
        foreach (var sprite in cardSprites)
        {
            if (sprite.name == spriteName)
                return sprite;
        }

        // Log de diagnóstico: muestra nombres disponibles
        Debug.LogWarning($"[CardSpriteMapper] Sprite no encontrado: '{spriteName}'");
        Debug.Log($"[CardSpriteMapper] Sprites disponibles: {string.Join(", ", System.Array.ConvertAll(cardSprites, s => s.name))}");
        return null;
    }
}
