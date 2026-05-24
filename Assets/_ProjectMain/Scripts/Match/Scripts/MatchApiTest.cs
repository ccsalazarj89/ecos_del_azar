using EcosDelAzar.Match;
using UnityEngine;

public class MatchApiTest : MonoBehaviour
{
    private MatchApiClient _api;

    private readonly string _playerOneId = "a1b2c3d4-e5f6-7890-abcd-ef1234567890";
    private readonly string _playerTwoId = "b2c3d4e5-f6a7-8901-bcde-f12345678901";

    void Start()
    {
        _api = GetComponent<MatchApiClient>();
        TestCreateMatch();
    }

    void TestCreateMatch()
    {
        Debug.Log(">>> Creando partida...");
        _api.CreateMatch(_playerOneId, _playerTwoId,
            onSuccess: match =>
            {
                Debug.Log($"✅ Partida creada — ID: {match.matchId} | Status: {match.status}");
                TestDrawCard(match.matchId);
            },
            onError: err => Debug.LogError($"❌ CreateMatch falló: {err.status} {err.error} — {err.message}")
        );
    }

    void TestDrawCard(string matchId)
    {
        Debug.Log(">>> Jugador 1 roba carta...");
        _api.DrawCard(matchId, _playerOneId,
            onSuccess: draw =>
            {
                Debug.Log($"✅ P1 robó {draw.card.rank} de {draw.card.suit} | MatchStatus: {draw.matchStatus}");

                Debug.Log(">>> Jugador 2 roba carta...");
                _api.DrawCard(matchId, _playerTwoId,
                    onSuccess: draw2 =>
                    {
                        Debug.Log($"✅ P2 robó {draw2.card.rank} de {draw2.card.suit} | MatchStatus: {draw2.matchStatus}");

                        if (draw2.result != null)
                        {
                            if (draw2.result.status == "WINNER")
                                Debug.Log($"🏆 Ganador: {draw2.result.winnerId}");
                            else
                                Debug.Log("🤝 Empate!");
                        }
                    },
                    onError: err => Debug.LogError($"❌ DrawCard P2 falló: {err.status} {err.error} — {err.message}")
                );
            },
            onError: err => Debug.LogError($"❌ DrawCard P1 falló: {err.status} {err.error} — {err.message}")
        );
    }
}
