using EcosDelAzar.Betting;
using EcosDelAzar.Match;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Orquesta el flujo de carta alta dentro de su propia escena.
/// Al cargar la escena muestra automáticamente el BettingPanel.
/// Tras cada ronda muestra el resultado; si el jugador pulsa Continuar
/// se abre otra ronda. Fold / Abandon / GameOver / NpcBankrupt
/// devuelven al jugador a la escena de origen.
/// </summary>
public class DuelManager : MonoBehaviour
{
    [Header("Dependencias")]
    public DuelUI         duelUI;
    public BettingManager bettingManager;
    public BettingUI      bettingUI;

    [Header("Jugador")]
    public string playerId   = "a1b2c3d4-e5f6-7890-abcd-ef1234567890";
    public string opponentId = "b2c3d4e5-f6a7-8901-bcde-f12345678901";

    [Header("Navegación")]
    public string returnScene = "SampleScene"; // escena a la que volver al salir

    public bool DuelInProgress { get; private set; } = false;

    private GameSession _session;
    private Deck        _deck;

    void Awake()
    {
        _deck = new Deck();
    }

    void Start()
    {
        // Obtener dependencias desde el GameManager persistente
        var gm = GameManager.GameManagerInstance;
        if (gm == null)
        {
            Debug.LogError("[DuelManager] GameManager no encontrado. " +
                           "Asegúrate de arrancar desde SampleScene.");
            return;
        }

        if (bettingManager == null) bettingManager = gm.GetComponentInChildren<BettingManager>(true);
        if (bettingUI == null)      bettingUI      = gm.GetComponentInChildren<BettingUI>(true);
        if (duelUI == null)         duelUI         = gm.GetComponentInChildren<DuelUI>(true);

        if (bettingManager == null)
        {
            Debug.LogError("[DuelManager] BettingManager no encontrado en GameManager.");
            return;
        }

        Debug.Log($"[DuelManager] Dependencias OK — BettingUI:{bettingUI != null} DuelUI:{duelUI != null}");

        bettingManager.OnBetConfirmed  += OnBetConfirmed;
        bettingManager.OnRoundFolded   += OnRoundFolded;
        bettingManager.OnGameAbandoned += OnGameAbandoned;
        bettingManager.OnGameOver      += OnGameOver;
        bettingManager.OnNpcBankrupt   += OnNpcBankrupt;

        if (duelUI != null)
            duelUI.OnContinue += OnPlayerContinue;

        // Iniciar la primera ronda
        StartRound();
    }

    void OnDestroy()
    {
        if (bettingManager != null)
        {
            bettingManager.OnBetConfirmed  -= OnBetConfirmed;
            bettingManager.OnRoundFolded   -= OnRoundFolded;
            bettingManager.OnGameAbandoned -= OnGameAbandoned;
            bettingManager.OnGameOver      -= OnGameOver;
            bettingManager.OnNpcBankrupt   -= OnNpcBankrupt;
        }

        if (duelUI != null)
            duelUI.OnContinue -= OnPlayerContinue;
    }

    // ── Flujo de ronda ────────────────────────────────────────

    private void StartRound()
    {
        DuelInProgress = true;
        bettingUI?.ShowPanel();
        Debug.Log("[DuelManager] Nueva ronda — esperando apuesta del jugador...");
    }

    /// <summary>El jugador pulsa Continuar tras ver el resultado → nueva ronda.</summary>
    private void OnPlayerContinue()
    {
        StartRound();
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

        RoundOutcome outcome = matchResult.Status == MatchResultStatus.DRAW
            ? RoundOutcome.Draw
            : matchResult.WinnerId == playerId
                ? RoundOutcome.Win
                : RoundOutcome.Lose;

        bettingManager.ResolveResult(outcome);

        DuelInProgress = false;
        duelUI?.ShowResult(playerCard, opponentCard, matchResult, playerId);
        // Desde aquí el jugador pulsa "Continuar" → OnPlayerContinue → StartRound
    }

    private void OnRoundFolded()
    {
        DuelInProgress = false;
        Debug.Log("[DuelManager] Jugador se retiró — volviendo al mundo.");
        ReturnToWorld();
    }

    private void OnGameAbandoned()
    {
        DuelInProgress = false;
        Debug.Log("[DuelManager] Partida abandonada — volviendo al mundo.");
        ReturnToWorld();
    }

    private void OnGameOver()
    {
        DuelInProgress = false;
        Debug.Log("[DuelManager] Game Over.");
        ReturnToWorld();
    }

    private void OnNpcBankrupt()
    {
        DuelInProgress = false;
        Debug.Log("[DuelManager] ¡El jugador ganó todo! El NPC quebró.");
        ReturnToWorld();
    }

    private void ReturnToWorld()
    {
        SceneManager.LoadScene(returnScene);
    }
}
