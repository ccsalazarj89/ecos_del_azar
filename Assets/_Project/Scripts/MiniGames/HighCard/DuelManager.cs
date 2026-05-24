using UnityEngine;
using EcosDelAzar.AI;

namespace EcosDelAzar.MiniGames
{
    public class DuelManager : MonoBehaviour
    {
        [SerializeField] BettingManager bettingManager;

        public string PlayerId { get; set; } = "player";
        public string OpponentId { get; set; } = "opponent";
        public bool DuelInProgress { get; private set; }

        GameSession session;
        Deck deck;

        // Expose last drawn cards for UI
        public Card LastPlayerCard { get; private set; }
        public Card LastOpponentCard { get; private set; }
        public MatchResult LastResult { get; private set; }

        void Awake()
        {
            deck = new Deck();

            bettingManager.OnBetConfirmed += OnBetConfirmed;
            bettingManager.OnRoundFolded += OnRoundFolded;
            bettingManager.OnGameAbandoned += OnGameAbandoned;
            bettingManager.OnGameOver += OnGameOver;
        }

        void OnDestroy()
        {
            bettingManager.OnBetConfirmed -= OnBetConfirmed;
            bettingManager.OnRoundFolded -= OnRoundFolded;
            bettingManager.OnGameAbandoned -= OnGameAbandoned;
            bettingManager.OnGameOver -= OnGameOver;
        }

        public void StartDuel(string npcId)
        {
            if (DuelInProgress) return;
            OpponentId = npcId;
            DuelInProgress = true;
        }

        void OnBetConfirmed(int playerBet, int npcBet)
        {
            if (!DuelInProgress) return;

            if (deck.Count < 2) deck.Reset();

            session = new GameSession(PlayerId, OpponentId, deck);
            LastPlayerCard = session.DrawCard(PlayerId);
            LastOpponentCard = session.DrawCard(OpponentId);
            LastResult = session.ResolveResult();

            RoundOutcome outcome = LastResult.Status == MatchResultStatus.Draw
                ? RoundOutcome.Draw
                : LastResult.WinnerId == PlayerId
                    ? RoundOutcome.Win
                    : RoundOutcome.Lose;

            bettingManager.ResolveResult(outcome);
            DuelInProgress = false;
        }

        void OnRoundFolded() => DuelInProgress = false;
        void OnGameAbandoned() => DuelInProgress = false;
        void OnGameOver() => DuelInProgress = false;
    }
}
