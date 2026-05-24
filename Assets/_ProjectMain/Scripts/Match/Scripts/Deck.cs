using System.Collections.Generic;
using UnityEngine;

namespace EcosDelAzar.Match
{
    /// <summary>
    /// Mazo completo de 56 cartas (52 + 4 Jokers).
    /// Lógica portada de Deck.java — misma estructura de datos.
    /// </summary>
    public class Deck
    {
        private readonly Queue<Card> _cards = new();

        public Deck()
        {
            Reset();
        }

        public void Reset()
        {
            _cards.Clear();
            var fullDeck = CreateFullDeck();
            Shuffle(fullDeck);
            foreach (var card in fullDeck)
                _cards.Enqueue(card);
        }

        public Card Draw()
        {
            if (_cards.Count == 0)
                throw new System.InvalidOperationException("Deck is empty");
            return _cards.Dequeue();
        }

        public int Count => _cards.Count;

        // ── Privados ──────────────────────────────────────────

        private static List<Card> CreateFullDeck()
        {
            var deck = new List<Card>();

            foreach (Suit suit in System.Enum.GetValues(typeof(Suit)))
            {
                if (suit == Suit.NONE) continue;

                foreach (Rank rank in System.Enum.GetValues(typeof(Rank)))
                {
                    if (rank == Rank.JOKER) continue;
                    deck.Add(new Card(suit, rank));
                }
            }

            // 4 Jokers con suit NONE — igual que en el Java
            for (int i = 0; i < 4; i++)
                deck.Add(new Card(Suit.NONE, Rank.JOKER));

            return deck;
        }

        /// <summary>Fisher-Yates shuffle usando UnityEngine.Random.</summary>
        private static void Shuffle(List<Card> deck)
        {
            for (int i = deck.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (deck[i], deck[j]) = (deck[j], deck[i]);
            }
        }
    }
}
