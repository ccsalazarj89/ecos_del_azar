namespace EcosDelAzar.Match
{
    public enum Suit
    {
        HEARTS,
        DIAMONDS,
        SPADES,
        CLUBS,
        NONE  // Jokers
    }

    public enum Rank
    {
        JOKER = 0,
        TWO   = 2,
        THREE = 3,
        FOUR  = 4,
        FIVE  = 5,
        SIX   = 6,
        SEVEN = 7,
        EIGHT = 8,
        NINE  = 9,
        TEN   = 10,
        JACK  = 11,
        QUEEN = 12,
        KING  = 13,
        ACE   = 14
    }

    /// <summary>
    /// Modelo de carta. La comparación se hace solo por Rank (valor numérico),
    /// el Suit no afecta al resultado — igual que en la API Java.
    /// </summary>
    public class Card
    {
        public Suit Suit { get; }
        public Rank Rank { get; }

        public Card(Suit suit, Rank rank)
        {
            Suit = suit;
            Rank = rank;
        }

        /// <summary>Compara por valor de rank. Positivo = this gana.</summary>
        public int CompareTo(Card other) => ((int)Rank).CompareTo((int)other.Rank);

        public override string ToString() => $"{Rank} of {Suit}";
    }
}
