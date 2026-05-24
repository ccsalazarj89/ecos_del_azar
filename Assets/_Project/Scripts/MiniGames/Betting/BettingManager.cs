using System;
using UnityEngine;

namespace EcosDelAzar.MiniGames.Betting
{
    public enum BetAction { Equal, Double, AllIn, FoldRound, AbandonGame }

    public class BettingManager : MonoBehaviour
    {
        [SerializeField] int startingChips = 1000;
        public int minimumBet = 10;

        public int PlayerChips { get; private set; }
        public int NpcChips { get; private set; }
        public int CurrentBet { get; private set; }
        public int NpcBet { get; private set; }
        public int EffectiveBet { get; private set; }

        public event Action<int, int> OnBetConfirmed;
        public event Action OnRoundFolded;
        public event Action OnGameAbandoned;
        public event Action OnGameOver;

        int lastBet;

        void Awake()
        {
            PlayerChips = startingChips;
            NpcChips = startingChips;
            lastBet = minimumBet;
        }

        public void ProcessPlayerAction(BetAction action)
        {
            switch (action)
            {
                case BetAction.Equal:
                    ConfirmBet(minimumBet);
                    break;
                case BetAction.Double:
                    ConfirmBet(Mathf.Min(lastBet * 2, PlayerChips));
                    break;
                case BetAction.AllIn:
                    ConfirmBet(PlayerChips);
                    break;
                case BetAction.FoldRound:
                    FoldRound();
                    break;
                case BetAction.AbandonGame:
                    AbandonGame();
                    break;
            }
        }

        public void ResolveResult(RoundOutcome outcome)
        {
            switch (outcome)
            {
                case RoundOutcome.Win:
                    PlayerChips += EffectiveBet;
                    NpcChips -= EffectiveBet;
                    break;
                case RoundOutcome.Lose:
                    PlayerChips -= EffectiveBet;
                    NpcChips += EffectiveBet;
                    break;
            }

            lastBet = CurrentBet;
            CheckGameOver();
        }

        public void Reset()
        {
            PlayerChips = startingChips;
            NpcChips = startingChips;
            lastBet = minimumBet;
        }

        void ConfirmBet(int playerBet)
        {
            CurrentBet = playerBet;
            NpcBet = NpcBettingAI.DecideBet(NpcChips, playerBet, minimumBet);
            EffectiveBet = Mathf.Min(CurrentBet, NpcBet);
            OnBetConfirmed?.Invoke(CurrentBet, NpcBet);
        }

        void FoldRound()
        {
            int penalty = Mathf.Min(minimumBet, PlayerChips);
            PlayerChips -= penalty;
            NpcChips += penalty;
            lastBet = minimumBet;
            OnRoundFolded?.Invoke();
            CheckGameOver();
        }

        void AbandonGame()
        {
            int penalty = Mathf.RoundToInt(PlayerChips * 0.1f);
            PlayerChips -= penalty;
            OnGameAbandoned?.Invoke();
        }

        void CheckGameOver()
        {
            if (PlayerChips <= 0)
                OnGameOver?.Invoke();
        }
    }
}
