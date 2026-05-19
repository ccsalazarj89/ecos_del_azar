using EcosDelAzar.Match;
using UnityEngine;

/// <summary>
/// Orquesta el flujo completo de un duelo:
/// CreateMatch → DrawCard (jugador) → DrawCard (NPC) → resultado.
/// Coloca este script en un GameObject vacío llamado "GameManager".
/// </summary>
public class DuelManager : MonoBehaviour
{
    [Header("Dependencias")]
    public MatchApiClient apiClient;
    public DuelUI duelUI;

    [Header("Jugador")]
    public string playerId = "a1b2c3d4-e5f6-7890-abcd-ef1234567890"; // UUID del jugador humano

    public bool DuelInProgress { get; private set; } = false;

    private string _currentMatchId;
    private DrawCardResponse _playerDraw;

    // ── Flujo principal ───────────────────────────────────────

    public void StartDuel(string opponentId)
    {
        if (DuelInProgress) return;

        DuelInProgress = true;
        Debug.Log($"[DuelManager] Iniciando duelo contra {opponentId}...");

        apiClient.CreateMatch(playerId, opponentId,
            onSuccess: match =>
            {
                _currentMatchId = match.matchId;
                Debug.Log($"[DuelManager] Partida creada — {match.matchId}");
                PlayerDraw(opponentId);
            },
            onError: err =>
            {
                Debug.LogError($"[DuelManager] Error al crear partida: {err.message}");
                DuelInProgress = false;
                if (err.error == "CONNECTION_ERROR")
                    duelUI?.ShowServerError();
            }
        );
    }

    private void PlayerDraw(string opponentId)
    {
        Debug.Log("[DuelManager] Jugador roba carta...");
        apiClient.DrawCard(_currentMatchId, playerId,
            onSuccess: draw =>
            {
                _playerDraw = draw;
                Debug.Log($"[DuelManager] Jugador robó {draw.card.rank} de {draw.card.suit}");
                NPCDraw(opponentId);
            },
            onError: err =>
            {
                Debug.LogError($"[DuelManager] Error al robar carta (jugador): {err.message}");
                DuelInProgress = false;
            }
        );
    }

    private void NPCDraw(string opponentId)
    {
        Debug.Log("[DuelManager] NPC roba carta...");
        apiClient.DrawCard(_currentMatchId, opponentId,
            onSuccess: draw =>
            {
                Debug.Log($"[DuelManager] NPC robó {draw.card.rank} de {draw.card.suit}");
                HandleResult(_playerDraw.card, draw.card, draw.result);
            },
            onError: err =>
            {
                Debug.LogError($"[DuelManager] Error al robar carta (NPC): {err.message}");
                DuelInProgress = false;
            }
        );
    }

    private void HandleResult(CardDto playerCard, CardDto npcCard, MatchResultDto result)
    {
        DuelInProgress = false;

        if (result == null)
        {
            Debug.LogWarning("[DuelManager] Partida sin resultado todavía.");
            return;
        }

        Debug.Log(result.status == "DRAW"
            ? "[DuelManager] 🤝 ¡Empate!"
            : result.winnerId == playerId
                ? "[DuelManager] 🏆 ¡El jugador gana!"
                : "[DuelManager] 💀 El NPC gana.");

        duelUI?.ShowResult(playerCard, npcCard, result, playerId);
    }
}
