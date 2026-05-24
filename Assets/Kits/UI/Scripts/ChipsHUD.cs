using EcosDelAzar.Betting;
using TMPro;
using UnityEngine;

/// <summary>
/// HUD persistente de fichas. Siempre visible en pantalla.
/// Crea un Canvas > Panel > dos TextMeshProUGUI y asigna las referencias aquí.
/// </summary>
public class ChipsHUD : MonoBehaviour
{
    [Header("Referencias UI")]
    public TextMeshProUGUI playerChipsText;
    public TextMeshProUGUI npcChipsText;
    public TextMeshProUGUI npcBetText;      // muestra la apuesta del NPC tras cada ronda

    [Header("Dependencias")]
    public BettingManager bettingManager;

    void Awake()
    {
        if (bettingManager == null)
            bettingManager = FindFirstObjectByType<BettingManager>();

        if (bettingManager == null)
        {
            Debug.LogError("[ChipsHUD] No se encontró BettingManager en la escena.");
            return;
        }

        bettingManager.OnChipsChanged  += RefreshChips;
        bettingManager.OnDuelPrepared  += OnDuelPrepared;
        bettingManager.OnBetConfirmed  += OnBetConfirmed;
        bettingManager.OnRoundFolded   += ClearBet;
        bettingManager.OnGameAbandoned += ClearBet;
        bettingManager.OnGameOver      += ClearBet;
    }

    void Start()
    {
        RefreshChips();
        ClearBet();
    }

    void OnDestroy()
    {
        if (bettingManager == null) return;
        bettingManager.OnChipsChanged  -= RefreshChips;
        bettingManager.OnDuelPrepared  -= OnDuelPrepared;
        bettingManager.OnBetConfirmed  -= OnBetConfirmed;
        bettingManager.OnRoundFolded   -= ClearBet;
        bettingManager.OnGameAbandoned -= ClearBet;
        bettingManager.OnGameOver      -= ClearBet;
    }

    private void RefreshChips()
    {
        if (playerChipsText != null)
            playerChipsText.text = $"Fichas: {bettingManager.PlayerChips}";

        if (npcChipsText != null)
            npcChipsText.text = $"NPC: {bettingManager.NpcChips}";
    }

    private void OnDuelPrepared(int npcOpeningBet)
    {
        if (npcBetText != null)
            npcBetText.text = $"NPC apuesta: {npcOpeningBet}";
    }

    private void OnBetConfirmed(int playerBet, int npcBet)
    {
        if (npcBetText != null)
            npcBetText.text = $"NPC: {npcBet}  |  Tú: {playerBet}";
    }

    private void ClearBet()
    {
        if (npcBetText != null)
            npcBetText.text = "";
    }
}
