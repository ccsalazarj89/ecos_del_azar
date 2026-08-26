using System;
using System.Collections;
using UnityEngine;

namespace EcosDelAzar.MiniGames.HighCard
{
    public class HighCardGame : MiniGameBase
    {
        public override string PlayingStatusText => "Repartiendo cartas...";

        [SerializeField] float revealDelay = 0.8f;
        [SerializeField] bool includeJokers;

        public Card PlayerCard { get; private set; }
        public Card OpponentCard { get; private set; }

        public event Action<Card> OnPlayerCardDrawn;
        public event Action<Card> OnOpponentCardRevealed;

        Deck deck;

        protected override void OnBegin()
        {
            deck = new Deck(includeJokers);
        }

        protected override IEnumerator PlayRoundRoutine()
        {
            if (deck.Count < 2) deck.Shuffle();

            PlayerCard = deck.Draw();
            OnPlayerCardDrawn?.Invoke(PlayerCard);

            yield return new WaitForSeconds(revealDelay);

            OpponentCard = deck.Draw();
            OnOpponentCardRevealed?.Invoke(OpponentCard);
        }

        protected override RoundResult EvaluateResult()
        {
            int cmp = PlayerCard.CompareTo(OpponentCard);

            RoundOutcome outcome = cmp > 0 ? RoundOutcome.Win
                                 : cmp < 0 ? RoundOutcome.Lose
                                 : RoundOutcome.Draw;

            return new RoundResult(outcome, PlayerCard.Value, OpponentCard.Value,
                $"{PlayerCard} vs {OpponentCard}");
        }
    }
}
