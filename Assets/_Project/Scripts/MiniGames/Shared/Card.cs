namespace EcosDelAzar.MiniGames
{
    public enum Suit { Hearts, Diamonds, Spades, Clubs, None }

    public enum Rank
    {
        Joker = 0,
        Two = 2, Three = 3, Four = 4, Five = 5, Six = 6,
        Seven = 7, Eight = 8, Nine = 9, Ten = 10,
        Jack = 11, Queen = 12, King = 13, Ace = 14
    }

    public class Card
    {
        public Suit Suit { get; }
        public Rank Rank { get; }
        public int Value => (int)Rank;

        public Card(Suit suit, Rank rank)
        {
            Suit = suit;
            Rank = rank;
        }

        public int CompareTo(Card other) => Value.CompareTo(other.Value);
        public override string ToString() => $"{Rank} of {Suit}";
    }
}
