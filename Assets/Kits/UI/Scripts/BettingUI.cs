using EcosDelAzar.Betting;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Panel de apuestas. Muestra las fichas y los 5 botones de acción.
/// Asigna las referencias en el Inspector.
/// </summary>
public class BettingUI : MonoBehaviour
{
    [Header("Panel")]
    public GameObject bettingPanel;

    [Header("Fichas")]
    public TextMeshProUGUI playerChipsText;
    public TextMeshProUGUI npcChipsText;
    public TextMeshProUGUI minimumBetText;

    [Header("Botones")]
    public Button equalButton;
    public Button doubleButton;
    public Button allInButton;
    public Button foldRoundButton;
    public Button abandonGameButton;

    [Header("Dependencias")]
    public BettingManager bettingManager;

    void Awake()
    {
        equalButton.onClick.AddListener(()       => OnAction(BetAction.Equal));
        doubleButton.onClick.AddListener(()      => OnAction(BetAction.Double));
        allInButton.onClick.AddListener(()       => OnAction(BetAction.AllIn));
        foldRoundButton.onClick.AddListener(()   => OnAction(BetAction.FoldRound));
        abandonGameButton.onClick.AddListener(() => OnAction(BetAction.AbandonGame));

        bettingManager.OnBetConfirmed  += (_, __) => HidePanel();
        bettingManager.OnRoundFolded   += HidePanel;
        bettingManager.OnGameAbandoned += HidePanel;
        bettingManager.OnGameOver      += HidePanel;

        HidePanel();
    }

    /// <summary>Muestra el panel con las fichas actualizadas.</summary>
    public void ShowPanel()
    {
        RefreshChips();
        bettingPanel.SetActive(true);
    }

    public void HidePanel()
    {
        bettingPanel.SetActive(false);
    }

    private void OnAction(BetAction action)
    {
        bettingManager.ProcessPlayerAction(action);
    }

    private void RefreshChips()
    {
        playerChipsText.text = $"Tus fichas: {bettingManager.PlayerChips}";
        npcChipsText.text    = $"NPC fichas: {bettingManager.NpcChips}";
        minimumBetText.text  = $"Apuesta mínima: {bettingManager.minimumBet}";
    }
}
