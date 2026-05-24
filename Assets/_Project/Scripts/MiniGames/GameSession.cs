using System.Collections.Generic;

namespace EcosDelAzar.Match
{
    public enum MatchResultStatus { WINNER, DRAW }

    public class MatchResult
    {
        public MatchResultStatus Status { get; }
        public string WinnerId { get; }  // null si DRAW

        private MatchResult(MatchResultStatus status, string winnerId = null)
        {
            Status   = status;
            WinnerId = winnerId;
        }

        public static MatchResult Winner(string playerId) =>
            new(MatchResultStatus.WINNER, playerId);

        public static MatchResult Draw() =>
            new(MatchResultStatus.DRAW);
    }

    /// <summary>
    /// Sesión de juego 1v1. Portado de GameSession.java.
    /// Gestiona el mazo, las cartas robadas y la resolución del resultado.
    /// </summary>
    public class GameSession
    {
        private readonly Deck _deck;
        private readonly Dictionary<string, Card> _drawnCards = new();
        private readonly HashSet<string> _players;

        public GameSession(string playerOneId, string playerTwoId, Deck deck)
        {
            _players = new HashSet<string> { playerOneId, playerTwoId };
            _deck    = deck;
        }

        /// <summary>El jugador roba una carta del mazo.</summary>
        public Card DrawCard(string playerId)
        {
            if (!_players.Contains(playerId))
                throw new System.ArgumentException($"Player {playerId} does not belong to this session");

            if (_drawnCards.ContainsKey(playerId))
                throw new System.InvalidOperationException($"Player {playerId} has already drawn a card");

            var card = _deck.Draw();
            _drawnCards[playerId] = card;
            return card;
        }

        /// <summary>Resuelve el resultado una vez ambos jugadores han robado.</summary>
        public MatchResult ResolveResult()
        {
            if (!IsRoundComplete())
                throw new System.InvalidOperationException("Round is not complete yet");

            var entries = new List<KeyValuePair<string, Card>>(_drawnCards);
            var e1 = entries[0];
            var e2 = entries[1];

            int cmp = e1.Value.CompareTo(e2.Value);

            if (cmp == 0) return MatchResult.Draw();

            return cmp > 0
                ? MatchResult.Winner(e1.Key)
                : MatchResult.Winner(e2.Key);
        }

        public bool IsRoundComplete() => _drawnCards.Count == _players.Count;

        public Card GetDrawnCard(string playerId) =>
            _drawnCards.TryGetValue(playerId, out var card) ? card : null;
    }
}
