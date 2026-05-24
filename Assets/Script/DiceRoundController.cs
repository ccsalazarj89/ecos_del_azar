using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DiceRoundController : MonoBehaviour
{
    [Header("Dice Rollers")]
    [SerializeField] private DiceRoller playerOneDiceRoller;
    [SerializeField] private DiceRoller playerTwoDiceRoller;

    [Header("UI")]
    [SerializeField] private Button rollButton;
    [SerializeField] private TMP_Text playerOneResultText;
    [SerializeField] private TMP_Text playerTwoResultText;
    [SerializeField] private TMP_Text winnerText;
    public int valor = 500;
    private bool pulsado = false;

    private DiceResult? playerOneResult;
    private DiceResult? playerTwoResult;

    private readonly DiceWinnerEvaluator winnerEvaluator = new();

    private void OnEnable()
    {
        if (playerOneDiceRoller != null)
            playerOneDiceRoller.OnRollFinished += HandlePlayerOneRollFinished;

        if (playerTwoDiceRoller != null)
            playerTwoDiceRoller.OnRollFinished += HandlePlayerTwoRollFinished;

        if (rollButton != null)
        {
            rollButton.onClick.AddListener(StartRound);
            
        }
            
    }

    private void OnDisable()
    {
        if (playerOneDiceRoller != null)
            playerOneDiceRoller.OnRollFinished -= HandlePlayerOneRollFinished;

        if (playerTwoDiceRoller != null)
            playerTwoDiceRoller.OnRollFinished -= HandlePlayerTwoRollFinished;

        if (rollButton != null)
            rollButton.onClick.RemoveListener(StartRound);
    }

    private void Start()
    {
        ResetUI();
        
    }

    public void StartRound()
    {
        if (playerOneDiceRoller == null || playerTwoDiceRoller == null)
        {
            Debug.LogError("DiceRoundController needs both DiceRollers assigned.", this);
            return;
        }

        if (playerOneDiceRoller.IsRolling || playerTwoDiceRoller.IsRolling)
            return;

        playerOneResult = null;
        playerTwoResult = null;

        SetRollButtonInteractable(false);

        playerOneResultText.text = "";
        playerTwoResultText.text = "";
        winnerText.text = "";

        playerOneDiceRoller.Roll();
        playerTwoDiceRoller.Roll();
    }

    private void HandlePlayerOneRollFinished(DiceResult result)
    {
        playerOneResult = result;

        if (playerOneResultText != null)
            playerOneResultText.text = $"{result.Value}";

        TryResolveRound();
    }

    private void HandlePlayerTwoRollFinished(DiceResult result)
    {
        playerTwoResult = result;

        if (playerTwoResultText != null)
            playerTwoResultText.text = $"{result.Value}";

        TryResolveRound();
    }

    private void TryResolveRound()
    {
        if (!playerOneResult.HasValue || !playerTwoResult.HasValue)
            return;

        DiceWinner winner = winnerEvaluator.Evaluate(
            playerOneResult.Value,
            playerTwoResult.Value
        );

        if (winnerText != null)
            winnerText.text = GetWinnerText(winner);

        SetRollButtonInteractable(true);
    }

    private void ResetUI()
    {
        if (playerOneResultText != null)
            playerOneResultText.text = "";

        if (playerTwoResultText != null)
            playerTwoResultText.text = "";

        if (winnerText != null)
            winnerText.text = "Presiona tirar dados";

        SetRollButtonInteractable(true);
    }

    private void SetRollButtonInteractable(bool interactable)
    {
        if (rollButton != null)
        {
            rollButton.interactable = interactable;
           
        }
            
    }

    private string GetWinnerText(DiceWinner winner)
    {
        return winner switch
        {
            DiceWinner.PlayerOne => "Jugador 1 ganó",
            DiceWinner.PlayerTwo => "Jugador 2 ganó",
            DiceWinner.Draw => "Empate",
            _ => "Sin resultado"
        };
    }
    public void Update()
    {
      
    }
}