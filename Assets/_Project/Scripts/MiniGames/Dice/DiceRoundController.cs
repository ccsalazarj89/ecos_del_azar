using UnityEngine;
using UnityEngine.UIElements;

namespace EcosDelAzar.MiniGames
{
    [RequireComponent(typeof(UIDocument))]
    public class DiceRoundController : MonoBehaviour
    {
        [SerializeField] DiceRoller playerDiceRoller;
        [SerializeField] DiceRoller opponentDiceRoller;

        Label playerResultLabel;
        Label opponentResultLabel;
        Label winnerLabel;
        Button rollButton;

        DiceResult? playerResult;
        DiceResult? opponentResult;
        readonly DiceWinnerEvaluator evaluator = new();

        void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            playerResultLabel = root.Q<Label>("player-result");
            opponentResultLabel = root.Q<Label>("opponent-result");
            winnerLabel = root.Q<Label>("winner-label");
            rollButton = root.Q<Button>("roll-button");

            rollButton.clicked += StartRound;

            if (playerDiceRoller != null)
                playerDiceRoller.OnRollFinished += OnPlayerRollFinished;
            if (opponentDiceRoller != null)
                opponentDiceRoller.OnRollFinished += OnOpponentRollFinished;

            ResetUI();
        }

        void OnDisable()
        {
            rollButton.clicked -= StartRound;

            if (playerDiceRoller != null)
                playerDiceRoller.OnRollFinished -= OnPlayerRollFinished;
            if (opponentDiceRoller != null)
                opponentDiceRoller.OnRollFinished -= OnOpponentRollFinished;
        }

        void StartRound()
        {
            if (playerDiceRoller == null || opponentDiceRoller == null) return;
            if (playerDiceRoller.IsRolling || opponentDiceRoller.IsRolling) return;

            playerResult = null;
            opponentResult = null;
            rollButton.SetEnabled(false);
            playerResultLabel.text = "";
            opponentResultLabel.text = "";
            winnerLabel.text = "";

            playerDiceRoller.Roll();
            opponentDiceRoller.Roll();
        }

        void OnPlayerRollFinished(DiceResult result)
        {
            playerResult = result;
            playerResultLabel.text = result.Value.ToString();
            TryResolve();
        }

        void OnOpponentRollFinished(DiceResult result)
        {
            opponentResult = result;
            opponentResultLabel.text = result.Value.ToString();
            TryResolve();
        }

        void TryResolve()
        {
            if (!playerResult.HasValue || !opponentResult.HasValue) return;

            DiceWinner winner = evaluator.Evaluate(playerResult.Value, opponentResult.Value);
            winnerLabel.text = winner switch
            {
                DiceWinner.PlayerOne => "Ganaste!",
                DiceWinner.PlayerTwo => "Perdiste...",
                DiceWinner.Draw => "Empate",
                _ => ""
            };

            rollButton.SetEnabled(true);
        }

        void ResetUI()
        {
            playerResultLabel.text = "";
            opponentResultLabel.text = "";
            winnerLabel.text = "Tira los dados";
            rollButton.SetEnabled(true);
        }
    }
}
