using System;
using UnityEngine;
using EcosDelAzar.Core;
using EcosDelAzar.Opponent;

namespace EcosDelAzar.MiniGames.Betting
{
    public enum BetResponse { Match, Double, Fold }

    public class BettingSystem : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] int minimumBet = 10;

        [Header("Opponent")]
        [SerializeField] OpponentBase opponent;

        public int PlayerCoins { get; private set; }
        public int OpponentCoins { get; private set; }
        public int MinimumBet => minimumBet;
        public int MaxBet => opponent != null ? Mathf.Min(PlayerCoins, OpponentCoins) : PlayerCoins;
        public int LastBet { get; private set; }
        public int NpcProposedBet { get; private set; }
        public int LastWinnings { get; private set; }
        public bool IsPlayerBroke => PlayerCoins < minimumBet;
        public bool IsOpponentBroke => OpponentCoins <= 0;

        public event Action OnCoinsUpdated;
        public event Action<int> OnNpcProposal;
        public event Action<bool> OnGameOver; // true = player won, false = player lost
        public event Action<RoundOutcome, int> OnRoundSettled; // outcome + signed winnings, fired after coins are applied

        Wallet wallet;

        public void Initialize()
        {
            wallet = GameManager.Instance?.Wallet;
            SyncFromWallet();
            LastBet = minimumBet;

            if (opponent != null)
            {
                opponent.ResetSession();
                OpponentCoins = opponent.Coins;
            }
        }

        /// <summary>
        /// Player and opponent ante up the bet before the round plays. The
        /// combined pot is paid out to the winner on resolution.
        /// </summary>
        public void PlaceBets(int playerBet)
        {
            playerBet = Mathf.Clamp(playerBet, minimumBet, MaxBet);
            LastBet = playerBet;

            PlayerCoins -= playerBet;
            if (opponent != null)
            {
                opponent.Coins -= playerBet;
                OpponentCoins = opponent.Coins;
            }

            SyncToWallet();
            OnCoinsUpdated?.Invoke();
        }

        /// <summary>
        /// After round resolves, apply win/loss and sync to wallet.
        /// Then ask NPC if they want to continue.
        /// </summary>
        public void ResolveResult(RoundOutcome outcome)
        {
            int bet = LastBet;
            int pot = bet * 2; // both sides anted in PlaceBets
            LastWinnings = 0;

            switch (outcome)
            {
                case RoundOutcome.Win:
                    PlayerCoins += pot;   // take own ante back + opponent's
                    LastWinnings = bet;   // net gain over the ante paid
                    break;

                case RoundOutcome.Lose:
                    if (opponent != null) opponent.Coins += pot;
                    LastWinnings = -bet;
                    break;

                case RoundOutcome.Draw:
                    PlayerCoins += bet;   // each side reclaims its ante
                    if (opponent != null) opponent.Coins += bet;
                    LastWinnings = 0;
                    break;
            }

            if (opponent != null)
                OpponentCoins = opponent.Coins;

            SyncToWallet();
            OnCoinsUpdated?.Invoke();
            OnRoundSettled?.Invoke(outcome, LastWinnings);

            if (IsPlayerBroke || IsOpponentBroke)
            {
                OnGameOver?.Invoke(IsOpponentBroke);
                return;
            }

            GenerateNpcProposal();
        }

        /// <summary>
        /// Player folds when NPC proposes continuation. Minimal penalty.
        /// </summary>
        public void PlayerFolds()
        {
            SyncToWallet();
        }

        public void ForceGameOver(bool playerWon)
        {
            OnGameOver?.Invoke(playerWon);
        }

        void GenerateNpcProposal()
        {
            if (opponent == null)
            {
                NpcProposedBet = LastBet;
                OnNpcProposal?.Invoke(NpcProposedBet);
                return;
            }

            var context = new BetContext(LastBet, minimumBet, opponent.Coins);
            opponent.RequestBet(context, OnNpcBetDecided);
        }

        void OnNpcBetDecided(int proposedBet)
        {
            int maxAllowed = Mathf.Min(PlayerCoins, OpponentCoins);
            NpcProposedBet = Mathf.Clamp(proposedBet, minimumBet, maxAllowed);
            OnNpcProposal?.Invoke(NpcProposedBet);
        }

        void SyncFromWallet()
        {
            PlayerCoins = wallet != null ? wallet.Coins : 0;
        }

        void SyncToWallet()
        {
            if (wallet == null) return;
            int diff = PlayerCoins - wallet.Coins;
            if (diff > 0)
                wallet.Add(diff);
            else if (diff < 0)
                wallet.TrySpend(-diff);
        }
    }
}
