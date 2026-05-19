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

    [Header("Jugador")]
    public string playerId = "a1b2c3d4-e5f6-7890-abcd-ef1234567890"; // UUID del jugador humano

    public bool DuelInProgress { get; private set; } = false;

    private string _currentMatchId;

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
            }
        );
    }

    private void PlayerDraw(string opponentId)
    {
        Debug.Log("[DuelManager] Jugador roba carta...");
        apiClient.DrawCard(_currentMatchId, playerId,
            onSuccess: draw =>
            {
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
                HandleResult(draw.result);
            },
            onError: err =>
            {
                Debug.LogError($"[DuelManager] Error al robar carta (NPC): {err.message}");
                DuelInProgress = false;
            }
        );
    }

    private void HandleResult(MatchResultDto result)
    {
        DuelInProgress = false;

        if (result == null)
        {
            Debug.LogWarning("[DuelManager] Partida sin resultado todavía.");
            return;
        }

        if (result.status == "DRAW")
        {
            Debug.Log("[DuelManager] 🤝 ¡Empate!");
            OnDraw();
        }
        else if (result.winnerId == playerId)
        {
            Debug.Log("[DuelManager] 🏆 ¡El jugador gana!");
            OnPlayerWin();
        }
        else
        {
            Debug.Log("[DuelManager] 💀 El NPC gana.");
            OnPlayerLose();
        }
    }

    // ── Callbacks de resultado (ampliar con UI/animaciones) ───

    private void OnPlayerWin()  { /* TODO: mostrar pantalla de victoria */ }
    private void OnPlayerLose() { /* TODO: mostrar pantalla de derrota  */ }
    private void OnDraw()       { /* TODO: mostrar pantalla de empate   */ }
}
