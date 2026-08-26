using System;
using System.Collections;
using UnityEngine;

namespace EcosDelAzar.MiniGames.RPS
{
    /// <summary>
    /// Rock, Paper, Scissors — Piedra, Papel o Tijera.
    /// Round waits for the player's choice, then reveals the opponent's.
    /// </summary>
    public class RPSGame : MiniGameBase
    {
        public override string PlayingStatusText => "Elige tu jugada";

        [Header("Reveal Timings")]
        [SerializeField] float suspenseDelay = 0.35f;
        [SerializeField] float revealDelay = 0.6f;

        [Header("Opponent Bias")]
        [Tooltip("Chance in [0,1] that the opponent picks a choice that BEATS the player's choice (unfair difficulty). 0 = fully random.")]
        [Range(0f, 1f)]
        [SerializeField] float opponentCheatChance = 0f;

        public RPSChoice PlayerChoice { get; private set; }
        public RPSChoice OpponentChoice { get; private set; }
        public bool AwaitingPlayerInput { get; private set; }

        public event Action OnAwaitingPlayerChoice;
        public event Action<RPSChoice> OnPlayerChoiceLocked;
        public event Action<RPSChoice, RPSChoice> OnChoicesRevealed;

        public void SubmitPlayerChoice(RPSChoice choice)
        {
            if (!AwaitingPlayerInput) return;
            if (choice == RPSChoice.None) return;

            PlayerChoice = choice;
            AwaitingPlayerInput = false;
            OnPlayerChoiceLocked?.Invoke(choice);
        }

        protected override IEnumerator PlayRoundRoutine()
        {
            PlayerChoice = RPSChoice.None;
            OpponentChoice = RPSChoice.None;
            AwaitingPlayerInput = true;
            OnAwaitingPlayerChoice?.Invoke();

            while (AwaitingPlayerInput)
                yield return null;

            yield return new WaitForSeconds(suspenseDelay);

            OpponentChoice = PickOpponentChoice(PlayerChoice);

            yield return new WaitForSeconds(revealDelay);

            OnChoicesRevealed?.Invoke(PlayerChoice, OpponentChoice);
        }

        protected override RoundResult EvaluateResult()
        {
            RoundOutcome outcome = Compare(PlayerChoice, OpponentChoice);
            return new RoundResult(outcome, (int)PlayerChoice, (int)OpponentChoice,
                $"{PlayerChoice} vs {OpponentChoice}");
        }

        /// <summary>
        /// Override to plug a custom brain. Default: random with a cheat chance
        /// that picks the choice which beats the player's.
        /// </summary>
        protected virtual RPSChoice PickOpponentChoice(RPSChoice playerChoice)
        {
            if (opponentCheatChance > 0f && UnityEngine.Random.value < opponentCheatChance)
                return CounterOf(playerChoice);

            return (RPSChoice)UnityEngine.Random.Range(1, 4);
        }

        static RPSChoice CounterOf(RPSChoice choice) => choice switch
        {
            RPSChoice.Rock => RPSChoice.Paper,
            RPSChoice.Paper => RPSChoice.Scissors,
            RPSChoice.Scissors => RPSChoice.Rock,
            _ => RPSChoice.Rock
        };

        static RoundOutcome Compare(RPSChoice player, RPSChoice opponent)
        {
            if (player == opponent) return RoundOutcome.Draw;

            bool playerWins =
                (player == RPSChoice.Rock && opponent == RPSChoice.Scissors) ||
                (player == RPSChoice.Paper && opponent == RPSChoice.Rock) ||
                (player == RPSChoice.Scissors && opponent == RPSChoice.Paper);

            return playerWins ? RoundOutcome.Win : RoundOutcome.Lose;
        }
    }
}
