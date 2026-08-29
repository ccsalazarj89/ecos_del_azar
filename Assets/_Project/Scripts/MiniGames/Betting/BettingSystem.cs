using System;
using UnityEngine;
using EcosDelAzar.Core;
using EcosDelAzar.Opponent;
using EcosDelAzar.Core.Echoes;

namespace EcosDelAzar.MiniGames.Betting
{
    /// <summary>The five combat actions of a round. Match/Double/PushLuck/Shield play on; Fold leaves.</summary>
    public enum BetResponse { Match, Double, PushLuck, Shield, Fold }

    /// <summary>How the player faces the round beyond the bet size.</summary>
    public enum RoundStance { Stand, PushLuck, Shield }

    public class BettingSystem : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] int minimumBet = 10;

        [Header("Opponent")]
        [SerializeField] OpponentBase opponent;
        public OpponentBase Opponent => opponent;

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
        bool syncingToWallet;
        bool lastBetWasDouble;
        RoundStance stance;

        [Header("Combat actions")]
        [Tooltip("Forzar la suerte: win pays this much extra of the bet; lose costs the same extra.")]
        [SerializeField, Range(0f, 1f)] float pushLuckEdge = 0.5f;
        [Tooltip("Blindarse: share of the bet refunded on a loss.")]
        [SerializeField, Range(0f, 1f)] float shieldMitigation = 0.5f;
        [Tooltip("Blindarse costs air: % of the tank drained when chosen.")]
        [SerializeField, Range(0f, 0.3f)] float shieldOxygenCost = 0.05f;

        public RoundStance LastStance => stance;
        public float PushLuckEdge => pushLuckEdge;
        public float ShieldMitigation => shieldMitigation;
        public float ShieldOxygenCost => shieldOxygenCost;

        /// <summary>True when the last lost round was refunded by the insurance Echo.</summary>
        public bool LastLossInsured { get; private set; }

        /// <summary>The dealer has made an offer and the player has not answered it yet.</summary>
        public bool HasStandingProposal { get; private set; }

        /// <summary>True when the table minimum comes from a previous seating (dealer remembers his raise).</summary>
        public bool TableMinimumActive => tableMinimumBet > minimumBet;

        void OnDisable()
        {
            if (wallet != null) wallet.OnCoinsChanged -= OnWalletChanged;
        }

        /// <summary>Re-opens the dealer's last offer on a re-entered table (no new ante yet).</summary>
        public void RestoreProposal()
        {
            if (MaxBet < MinimumBet) return;
            NpcProposedBet = MinimumBet;
            HasStandingProposal = true;
        }

        // Coins can change outside the table (debug cheats, a revive refund...). Keep the local copy honest.
        void OnWalletChanged(int coins)
        {
            if (syncingToWallet || PlayerCoins == coins) return;
            PlayerCoins = coins;
            OnCoinsUpdated?.Invoke();
        }

        /// <summary>
        /// Starts a seating. Pass the table's remembered state so a re-entered table
        /// keeps its opponent stack and escalated minimum (-1 / 0 = table defaults).
        /// </summary>
        public void Initialize(int opponentCoinsOverride = -1, int minimumBetOverride = 0)
        {
            if (wallet != null) wallet.OnCoinsChanged -= OnWalletChanged;
            wallet = GameManager.Instance?.Wallet;
            if (wallet != null) wallet.OnCoinsChanged += OnWalletChanged;
            SyncFromWallet();
            tableMinimumBet = minimumBetOverride;
            lastBetWasDouble = false;
            LastLossInsured = false;
            LastBet = MinimumBet;
            NpcProposedBet = MinimumBet;
            HasStandingProposal = false;

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
        public void PlaceBets(int playerBet, bool doubled = false, RoundStance roundStance = RoundStance.Stand)
        {
            playerBet = Mathf.Clamp(playerBet, MinimumBet, MaxBet);
            LastBet = playerBet;
            lastBetWasDouble = doubled;
            stance = roundStance;
            HasStandingProposal = false;

            // Blindarse is paid in air, up front, whatever the outcome.
            if (stance == RoundStance.Shield)
            {
                var tank = GameManager.Instance?.OxygenTank;
                if (tank != null)
                {
                    tank.Deplete(tank.Max * shieldOxygenCost);
                    tank.Report(-tank.Max * shieldOxygenCost, "Te blindaste");
                }
            }

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

                    // Forzar la suerte: the dealer pays the edge on top.
                    if (stance == RoundStance.PushLuck)
                    {
                        int edge = Mathf.RoundToInt(bet * pushLuckEdge);
                        edge = Mathf.Min(edge, opponent != null ? opponent.Coins : edge);
                        PlayerCoins += edge;
                        if (opponent != null) opponent.Coins -= edge;
                        LastWinnings += edge;
                    }

                    // "Codicia": a doubled win spends one charge and the house pays the extra.
                    if (lastBetWasDouble && mods != null && mods.TryConsume(EcoEffect.DoubleWinBonus, out float mult) && mult > 1f)
                    {
                        int bonus = Mathf.RoundToInt(bet * (mult - 1f));
                        PlayerCoins += bonus;
                        LastWinnings += bonus;
                    }
                    break;

                case RoundOutcome.Lose:
                    if (opponent != null) opponent.Coins += pot;
                    LastWinnings = -bet;

                    // Forzar la suerte backfires: the edge goes to the dealer too.
                    if (stance == RoundStance.PushLuck)
                    {
                        int edge = Mathf.Min(Mathf.RoundToInt(bet * pushLuckEdge), PlayerCoins);
                        PlayerCoins -= edge;
                        if (opponent != null) opponent.Coins += edge;
                        LastWinnings -= edge;
                    }

                    // Blindarse: part of the loss comes back.
                    if (stance == RoundStance.Shield)
                    {
                        int refund = Mathf.RoundToInt(bet * shieldMitigation);
                        PlayerCoins += refund;
                        if (opponent != null) opponent.Coins -= refund;
                        LastWinnings += refund;
                    }

                    // "Seguro del tahur": one charge refunds this loss.
                    if (mods != null && mods.TryConsume(EcoEffect.FirstLossInsurance, out _))
                    {
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
                HasStandingProposal = false;
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
            // The offer stays on the table for next time.
            SyncToWallet();
        }

        public void ForceGameOver(bool playerWon)
        {
            OnGameOver?.Invoke(playerWon);
        }

        // ------------------------------------------------------------------ muerte súbita

        /// <summary>Declining the boss's challenge is not free: the house keeps a share of the player's coins.</summary>
        public int TakeDeclineFee(float share)
        {
            int fee = Mathf.RoundToInt(PlayerCoins * Mathf.Clamp01(share));
            if (fee <= 0) return 0;
            PlayerCoins -= fee;
            if (opponent != null) { opponent.Coins += fee; OpponentCoins = opponent.Coins; }
            SyncToWallet();
            OnCoinsUpdated?.Invoke();
            return fee;
        }

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
            HasStandingProposal = true;
            OnNpcProposal?.Invoke(NpcProposedBet);
        }

        void SyncFromWallet()
        {
            PlayerCoins = wallet != null ? wallet.Coins : 0;
        }

        void SyncToWallet()
        {
            if (wallet == null) return;
            syncingToWallet = true;
            int diff = PlayerCoins - wallet.Coins;
            if (diff > 0)
                wallet.Add(diff);
            else if (diff < 0)
                wallet.TrySpend(-diff);
            syncingToWallet = false;
        }
    }
}
