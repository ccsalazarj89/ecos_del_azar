using System;
using System.Collections;
using UnityEngine;

namespace EcosDelAzar.MiniGames.Blackjack
{
    /// <summary>
    /// Blackjack (21). Player hits/stands, then the dealer plays under
    /// standard casino rules: dealer must hit while score &lt; dealerStandsAt.
    /// </summary>
    public class BlackjackGame : MiniGameBase
    {
        public override string PlayingStatusText => "Tu turno: pide o plántate";

        [Header("Rules")]
        [Tooltip("Score at or above which the dealer stops taking cards. Standard = 17.")]
        [SerializeField, Range(15, 21)] int dealerStandsAt = 17;

        [Header("Timings")]
        [SerializeField] float dealDelay = 0.4f;
        [SerializeField] float dealerActionDelay = 0.6f;
        [SerializeField] float finalRevealDelay = 0.5f;

        public BlackjackHand PlayerHand { get; } = new();
        public BlackjackHand OpponentHand { get; } = new();
        public bool AwaitingPlayerInput { get; private set; }

        public event Action OnRoundDealt;
        public event Action<Card> OnPlayerCardDealt;
        public event Action<Card> OnOpponentCardDealt;
        public event Action OnAwaitingPlayerAction;
        public event Action OnPlayerStood;
        public event Action OnOpponentHoleCardRevealed;

        Deck deck;

        protected override void OnBegin()
        {
            deck = new Deck(includeJokers: false);
        }

        public void Hit()
        {
            if (!AwaitingPlayerInput) return;

            var card = deck.Draw();
            PlayerHand.Add(card);
            OnPlayerCardDealt?.Invoke(card);

            if (PlayerHand.IsBust || PlayerHand.Score == 21)
                AwaitingPlayerInput = false;
        }

        public void Stand()
        {
            if (!AwaitingPlayerInput) return;
            AwaitingPlayerInput = false;
            OnPlayerStood?.Invoke();
        }

        protected override IEnumerator PlayRoundRoutine()
        {
            PlayerHand.Clear();
            OpponentHand.Clear();

            if (deck.Count < 15) deck.Shuffle();

            yield return new WaitForSeconds(dealDelay);
            var p1 = deck.Draw(); PlayerHand.Add(p1);
            OnPlayerCardDealt?.Invoke(p1);

            yield return new WaitForSeconds(dealDelay);
            var o1 = deck.Draw(); OpponentHand.Add(o1);
            OnOpponentCardDealt?.Invoke(o1);

            yield return new WaitForSeconds(dealDelay);
            var p2 = deck.Draw(); PlayerHand.Add(p2);
            OnPlayerCardDealt?.Invoke(p2);

            yield return new WaitForSeconds(dealDelay);
            var o2 = deck.Draw(); OpponentHand.Add(o2);
            OnOpponentCardDealt?.Invoke(o2);

            OnRoundDealt?.Invoke();

            if (!PlayerHand.IsBlackjack)
            {
                AwaitingPlayerInput = true;
                OnAwaitingPlayerAction?.Invoke();

                while (AwaitingPlayerInput)
                    yield return null;
            }

            yield return new WaitForSeconds(dealerActionDelay);
            OnOpponentHoleCardRevealed?.Invoke();

            if (!PlayerHand.IsBust)
            {
                while (OpponentHand.Score < dealerStandsAt)
                {
                    yield return new WaitForSeconds(dealerActionDelay);
                    var c = deck.Draw();
                    OpponentHand.Add(c);
                    OnOpponentCardDealt?.Invoke(c);
                }
            }

            yield return new WaitForSeconds(finalRevealDelay);
        }

        protected override RoundResult EvaluateResult()
        {
            int p = PlayerHand.Score;
            int o = OpponentHand.Score;

            RoundOutcome outcome;

            if (PlayerHand.IsBust)
                outcome = RoundOutcome.Lose;
            else if (OpponentHand.IsBust)
                outcome = RoundOutcome.Win;
            else if (PlayerHand.IsBlackjack && !OpponentHand.IsBlackjack)
                outcome = RoundOutcome.Win;
            else if (OpponentHand.IsBlackjack && !PlayerHand.IsBlackjack)
                outcome = RoundOutcome.Lose;
            else if (p > o)
                outcome = RoundOutcome.Win;
            else if (p < o)
                outcome = RoundOutcome.Lose;
            else
                outcome = RoundOutcome.Draw;

            return new RoundResult(outcome, p, o, $"{p} vs {o}");
        }

        protected override float GetResultDisplayTime() => 2.4f;

        protected override void OnEnd()
        {
            AwaitingPlayerInput = false;
        }
    }
}
