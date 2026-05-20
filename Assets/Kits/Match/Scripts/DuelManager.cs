using EcosDelAzar.Betting;
using EcosDelAzar.Match;
using UnityEngine;

/// <summary>
/// Orquesta el flujo de carta alta:
/// Apuesta → DrawCard (jugador + NPC) → Resultado → ResolveResult (genérico).
/// </summary>
public class DuelManager : MonoBehaviour
{
    [Header("Dependencias")]
    public DuelUI         duelUI;
    public BettingManager bettingManager;

    [Header("Jugador")]
    public string playerId   = "a1b2c3d4-e5f6-7890-abcd-ef1234567890";
    public string opponentId = "b2c3d4e5-f6a7-8901-bcde-f12345678901";

    public bool DuelInProgress { get; private set; } = false;

    private GameSession _session;
    private Deck        _deck;

    void Awake()
    {
        _deck = new Deck();

        bettingManager.OnBetConfirmed  += OnBetConfirmed;
        bettingManager.OnRoundFolded   += OnRoundFolded;
        bettingManager.OnGameAbandoned += OnGameAbandoned;
        bettingManager.OnGameOver      += OnGameOver;
    }

    void OnDestroy()
    {
        bettingManager.OnBetConfirmed  -= OnBetConfirmed;
        bettingManager.OnRoundFolded   -= OnRoundFolded;
        bettingManager.OnGameAbandoned -= OnGameAbandoned;
        bettingManager.OnGameOver      -= OnGameOver;
    }

    // ── Entrada ───────────────────────────────────────────────

    public void StartDuel(string npcId)
    {
        if (DuelInProgress) return;
        opponentId     = npcId;
        DuelInProgress = true;
        Debug.Log("[DuelManager] Esperando apuesta del jugador...");
    }

    // ── Eventos de BettingManager ─────────────────────────────

    private void OnBetConfirmed(int playerBet, int npcBet)
    {
        if (!DuelInProgress) return;

        Debug.Log("[DuelManager] Apuesta confirmada — iniciando duelo...");

        if (_deck.Count < 2)
        {
            Debug.Log("[DuelManager] Mazo agotado — reseteando...");
            _deck.Reset();
        }

        Debug.Log($"[DuelManager] Cartas restantes: {_deck.Count}");

        _session = new GameSession(playerId, opponentId, _deck);

        var playerCard   = _session.DrawCard(playerId);
        var opponentCard = _session.DrawCard(opponentId);

        Debug.Log($"[DuelManager] Jugador robó {playerCard}");
        Debug.Log($"[DuelManager] NPC robó {opponentCard}");

        var matchResult = _session.ResolveResult();

        // Traducir a RoundOutcome genérico para BettingManager
        RoundOutcome outcome = matchResult.Status == MatchResultStatus.DRAW
            ? RoundOutcome.Draw
            : matchResult.WinnerId == playerId
                ? RoundOutcome.Win
                : RoundOutcome.Lose;

        bettingManager.ResolveResult(outcome);

        DuelInProgress = false;
        duelUI?.ShowResult(playerCard, opponentCard, matchResult, playerId);
    }

    private void OnRoundFolded()
    {
        DuelInProgress = false;
        Debug.Log("[DuelManager] Ronda cancelada por retiro del jugador.");
    }

    private void OnGameAbandoned()
    {
        DuelInProgress = false;
        Debug.Log("[DuelManager] Partida abandonada.");
        // TODO: volver al menú principal
    }

    private void OnGameOver()
    {
        DuelInProgress = false;
        Debug.Log("[DuelManager] Game Over.");
        // TODO: mostrar pantalla de Game Over
    }
}
