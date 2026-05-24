using System;
using System.Collections.Generic;

namespace EcosDelAzar.Match
{
    // ── Requests ──────────────────────────────────────────────

    [Serializable]
    public class CreateMatchRequest
    {
        public string playerOneId;
        public string playerTwoId;
    }

    [Serializable]
    public class DrawCardRequest
    {
        public string playerId;
    }

    // ── Responses ─────────────────────────────────────────────

    /// POST /matches → 201
    [Serializable]
    public class CreateMatchResponse
    {
        public string matchId;
        public string playerOneId;
        public string playerTwoId;
        public string status;           // WAITING
    }

    /// POST /matches/{matchId}/draw → 200
    [Serializable]
    public class DrawCardResponse
    {
        public string       playerId;
        public CardDto      card;
        public string       matchStatus;    // WAITING | COMPLETED
        public MatchResultDto result;       // null si matchStatus == WAITING
    }

    /// GET /matches/{matchId} → 200
    [Serializable]
    public class MatchStateResponse
    {
        public string             matchId;
        public string             playerOneId;
        public string             playerTwoId;
        public string             status;     // WAITING | COMPLETED
        public List<PlayerDrawDto> draws;
        public MatchResultDto     result;     // null si status == WAITING
    }

    /// Error genérico de la API (400, 404, 409, 422)
    [Serializable]
    public class ApiError
    {
        public int    status;
        public string error;
        public string message;
    }

    // ── Shared ────────────────────────────────────────────────

    [Serializable]
    public class CardDto
    {
        public string suit;     // HEARTS | DIAMONDS | CLUBS | SPADES | NONE
        public string rank;     // TWO … ACE | JOKER
    }

    [Serializable]
    public class PlayerDrawDto
    {
        public string  playerId;
        public CardDto card;
    }

    [Serializable]
    public class MatchResultDto
    {
        public string status;       // WINNER | DRAW
        public string winnerId;     // null si DRAW
    }
}
