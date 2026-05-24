using System;
using System.Collections.Generic;
using UnityEngine;

namespace EcosDelAzar.MiniGames
{
    public class Deck
    {
        readonly List<Card> allCards = new();
        readonly Queue<Card> drawPile = new();

        public int Count => drawPile.Count;

        public Deck(bool includeJokers = false)
        {
            Build(includeJokers);
            Shuffle();
        }

        public Card Draw()
        {
            if (drawPile.Count == 0) Shuffle();
            return drawPile.Dequeue();
        }

        public void Shuffle()
        {
            var temp = new List<Card>(allCards);
            for (int i = temp.Count - 1; i > 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                (temp[i], temp[j]) = (temp[j], temp[i]);
            }

            drawPile.Clear();
            foreach (var card in temp)
                drawPile.Enqueue(card);
        }

        void Build(bool includeJokers)
        {
            allCards.Clear();
            foreach (Suit suit in Enum.GetValues(typeof(Suit)))
            {
                if (suit == Suit.None) continue;
                foreach (Rank rank in Enum.GetValues(typeof(Rank)))
                {
                    if (rank == Rank.Joker) continue;
                    allCards.Add(new Card(suit, rank));
                }
            }

            if (includeJokers)
                for (int i = 0; i < 4; i++)
                    allCards.Add(new Card(Suit.None, Rank.Joker));
        }
    }
}
