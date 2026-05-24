using System;
using System.Collections.Generic;
using UnityEngine;

namespace EcosDelAzar.MiniGames
{
    public class Deck
    {
        readonly Queue<Card> cards = new();

        public int Count => cards.Count;

        public Deck() => Reset();

        public void Reset()
        {
            cards.Clear();
            var fullDeck = CreateFullDeck();
            Shuffle(fullDeck);
            foreach (var card in fullDeck)
                cards.Enqueue(card);
        }

        public Card Draw()
        {
            if (cards.Count == 0)
                throw new InvalidOperationException("Deck is empty");
            return cards.Dequeue();
        }

        static List<Card> CreateFullDeck()
        {
            var deck = new List<Card>();

            foreach (Suit suit in Enum.GetValues(typeof(Suit)))
            {
                if (suit == Suit.None) continue;
                foreach (Rank rank in Enum.GetValues(typeof(Rank)))
                {
                    if (rank == Rank.Joker) continue;
                    deck.Add(new Card(suit, rank));
                }
            }

            for (int i = 0; i < 4; i++)
                deck.Add(new Card(Suit.None, Rank.Joker));

            return deck;
        }

        static void Shuffle(List<Card> deck)
        {
            for (int i = deck.Count - 1; i > 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                (deck[i], deck[j]) = (deck[j], deck[i]);
            }
        }
    }
}
