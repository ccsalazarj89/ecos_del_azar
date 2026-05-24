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
        public int LastBet { get; private set; }
        public int NpcProposedBet { get; private set; }
        public int LastWinnings { get; private set; }
        public bool IsPlayerBroke => PlayerCoins < minimumBet;
        public bool IsOpponentBroke => OpponentCoins <= 0;

        public event Action OnCoinsUpdated;
        public event Action<int> OnNpcProposal;
        public event Action<bool> OnGameOver; // true = player won, false = player lost

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
        /// Player places bet, NPC matches or raises. Called before dice roll.
        /// </summary>
        public void PlaceBets(int playerBet)
        {
            playerBet = Mathf.Clamp(playerBet, minimumBet, PlayerCoins);
            LastBet = playerBet;
        }

        /// <summary>
        /// After round resolves, apply win/loss and sync to wallet.
        /// Then ask NPC if they want to continue.
        /// </summary>
        public void ResolveResult(RoundOutcome outcome)
        {
            int amount = LastBet;
            LastWinnings = 0;

            switch (outcome)
            {
                case RoundOutcome.Win:
                    LastWinnings = amount;
                    PlayerCoins += amount;
                    if (opponent != null) opponent.Coins -= amount;
                    break;

                case RoundOutcome.Lose:
                    LastWinnings = -amount;
                    PlayerCoins -= amount;
                    if (opponent != null) opponent.Coins += amount;
                    break;

                case RoundOutcome.Draw:
                    LastWinnings = 0;
                    break;
            }

            if (opponent != null)
                OpponentCoins = opponent.Coins;

            SyncToWallet();
            OnCoinsUpdated?.Invoke();

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
