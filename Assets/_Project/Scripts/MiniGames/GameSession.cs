using System.Collections.Generic;

namespace EcosDelAzar.MiniGames
{
    public enum MatchResultStatus { Winner, Draw }

    public class MatchResult
    {
        public MatchResultStatus Status { get; }
        public string WinnerId { get; }

        MatchResult(MatchResultStatus status, string winnerId = null)
        {
            Status = status;
            WinnerId = winnerId;
        }

        public static MatchResult Winner(string playerId) => new(MatchResultStatus.Winner, playerId);
        public static MatchResult Draw() => new(MatchResultStatus.Draw);
    }

    public class GameSession
    {
        readonly Deck deck;
        readonly Dictionary<string, Card> drawnCards = new();
        readonly HashSet<string> players;

        public GameSession(string playerOneId, string playerTwoId, Deck deck)
        {
            players = new HashSet<string> { playerOneId, playerTwoId };
            this.deck = deck;
        }

        public Card DrawCard(string playerId)
        {
            if (!players.Contains(playerId))
                throw new System.ArgumentException($"Player {playerId} not in session");
            if (drawnCards.ContainsKey(playerId))
                throw new System.InvalidOperationException($"Player {playerId} already drew");

            var card = deck.Draw();
            drawnCards[playerId] = card;
            return card;
        }

        public MatchResult ResolveResult()
        {
            if (!IsRoundComplete())
                throw new System.InvalidOperationException("Round not complete");

            var entries = new List<KeyValuePair<string, Card>>(drawnCards);
            int cmp = entries[0].Value.CompareTo(entries[1].Value);

            if (cmp == 0) return MatchResult.Draw();
            return cmp > 0 ? MatchResult.Winner(entries[0].Key) : MatchResult.Winner(entries[1].Key);
        }

        public bool IsRoundComplete() => drawnCards.Count == players.Count;
        public Card GetDrawnCard(string playerId) => drawnCards.TryGetValue(playerId, out var c) ? c : null;
    }
}
