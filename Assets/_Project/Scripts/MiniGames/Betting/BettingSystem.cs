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
        public int MinimumBet => Mathf.Max(minimumBet, tableMinimumBet);
        public int MaxBet => opponent != null ? Mathf.Min(PlayerCoins, OpponentCoins) : PlayerCoins;
        public int LastBet { get; private set; }
        public int NpcProposedBet { get; private set; }
        public int LastWinnings { get; private set; }
        // Broke = unable to cover the table minimum, on either side. With table memory the
        // minimum can climb, so a dealer who cannot match it loses the table.
        public bool IsPlayerBroke => PlayerCoins < MinimumBet;
        public bool IsOpponentBroke => opponent != null && OpponentCoins < MinimumBet;

        public event Action OnCoinsUpdated;
        public event Action<int> OnNpcProposal;
        public event Action<bool> OnGameOver; // true = player won, false = player lost
        public event Action<RoundOutcome, int> OnRoundSettled; // outcome + signed winnings, fired after coins are applied

        Wallet wallet;
        int tableMinimumBet;
        bool lastBetWasDouble;
        bool insuranceAvailable;

        /// <summary>True when the last lost round was refunded by the insurance Echo.</summary>
        public bool LastLossInsured { get; private set; }

        /// <summary>True when the table minimum comes from a previous seating (dealer remembers his raise).</summary>
        public bool TableMinimumActive => tableMinimumBet > minimumBet;

        /// <summary>
        /// Starts a seating. Pass the table's remembered state so a re-entered table
        /// keeps its opponent stack and escalated minimum (-1 / 0 = table defaults).
        /// </summary>
        public void Initialize(int opponentCoinsOverride = -1, int minimumBetOverride = 0)
        {
            wallet = GameManager.Instance?.Wallet;
            SyncFromWallet();
            tableMinimumBet = minimumBetOverride;
            insuranceAvailable = GameManager.Instance?.Modifiers?.HasFirstLossInsurance ?? false;
            lastBetWasDouble = false;
            LastLossInsured = false;
            LastBet = MinimumBet;
            NpcProposedBet = MinimumBet;

            if (opponent != null)
            {
                opponent.ResetSession();
                if (opponentCoinsOverride >= 0) opponent.Coins = opponentCoinsOverride;
                OpponentCoins = opponent.Coins;
            }
        }

        /// <summary>
        /// Player and opponent ante up the bet before the round plays. The
        /// combined pot is paid out to the winner on resolution.
        /// </summary>
        public void PlaceBets(int playerBet, bool doubled = false)
        {
            playerBet = Mathf.Clamp(playerBet, MinimumBet, MaxBet);
            LastBet = playerBet;
            lastBetWasDouble = doubled;

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
            LastLossInsured = false;
            var mods = GameManager.Instance?.Modifiers;

            switch (outcome)
            {
                case RoundOutcome.Win:
                    PlayerCoins += pot;   // take own ante back + opponent's
                    LastWinnings = bet;   // net gain over the ante paid

                    // "Codicia": doubling pays extra, out of the house pocket.
                    if (lastBetWasDouble && mods != null && mods.DoubleWinMultiplier > 1f)
                    {
                        int bonus = Mathf.RoundToInt(bet * (mods.DoubleWinMultiplier - 1f));
                        PlayerCoins += bonus;
                        LastWinnings += bonus;
                    }
                    break;

                case RoundOutcome.Lose:
                    if (opponent != null) opponent.Coins += pot;
                    LastWinnings = -bet;

                    // "Seguro del tahur": the house refunds the first loss of the seating.
                    if (insuranceAvailable)
                    {
                        insuranceAvailable = false;
                        LastLossInsured = true;
                        PlayerCoins += bet;
                        LastWinnings = 0;
                    }
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

        // ------------------------------------------------------------------ muerte súbita

        /// <summary>
        /// Pone todas las fichas de ambos lados en el pot para la muerte súbita.
        /// Devuelve el valor total del pot.
        /// </summary>
        public (int pot, int bossCoins) StartSuddenDeath()
        {
            int bossCoins = OpponentCoins;
            int pot       = PlayerCoins + OpponentCoins;
            PlayerCoins   = 0;
            if (opponent != null) opponent.Coins = 0;
            OpponentCoins = 0;
            SyncToWallet();
            OnCoinsUpdated?.Invoke();
            return (pot, bossCoins);
        }

        /// <summary>
        /// Distribuye el resultado de la muerte súbita.
        /// Si el jugador gana, recupera sus fichas + el doble de las del boss.
        /// Si pierde, el boss se lleva el pot completo.
        /// </summary>
        public void ResolveSuddenDeath(bool playerWon, int pot, int bossCoins)
        {
            if (playerWon)
            {
                int playerOriginal = pot - bossCoins;
                PlayerCoins = playerOriginal + bossCoins * 2;
            }
            else
            {
                if (opponent != null) opponent.Coins = pot;
                OpponentCoins = pot;
            }
            SyncToWallet();
            OnCoinsUpdated?.Invoke();
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

            var context = new BetContext(LastBet, MinimumBet, opponent.Coins);
            opponent.RequestBet(context, OnNpcBetDecided);
        }

        void OnNpcBetDecided(int proposedBet)
        {
            // Never propose more than either side can put on the table. MaxBet >= MinimumBet
            // is guaranteed here because broke sides end the match before a proposal.
            NpcProposedBet = Mathf.Clamp(proposedBet, MinimumBet, Mathf.Max(MinimumBet, MaxBet));
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
