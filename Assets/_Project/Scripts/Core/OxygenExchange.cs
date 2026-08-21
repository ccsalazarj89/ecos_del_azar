using System;
using UnityEngine;

namespace EcosDelAzar.Core
{
    public enum TradeMode { Buy, Sell }

    public enum TradeError
    {
        None,
        NoSteps,
        NotEnoughCoins,
        NotEnoughOxygen,
        TankFull,
        Unavailable
    }

    /// <summary>Result of pricing a trade. Also returned after executing one.</summary>
    public readonly struct TradeQuote
    {
        public TradeMode Mode { get; }
        /// <summary>How many price steps the trade covers.</summary>
        public int Steps { get; }
        /// <summary>Coins spent (Buy) or earned (Sell).</summary>
        public int Coins { get; }
        /// <summary>Oxygen gained (Buy) or given up (Sell), in tank units.</summary>
        public float Oxygen { get; }
        /// <summary>The same amount as a percentage of the full tank — what the player is shown.</summary>
        public int Percent { get; }
        public TradeError Error { get; }

        public bool IsValid => Error == TradeError.None;

        public TradeQuote(TradeMode mode, int steps, int coins, float oxygen, int percent, TradeError error)
        {
            Mode = mode;
            Steps = steps;
            Coins = coins;
            Oxygen = oxygen;
            Percent = percent;
            Error = error;
        }

        public static TradeQuote Invalid(TradeMode mode, TradeError error) =>
            new TradeQuote(mode, 0, 0, 0f, 0, error);
    }

    /// <summary>
    /// Pricing and execution of oxygen-for-coins trades. Serialized inside whatever
    /// sells oxygen (vending machines, traders), so each one can set its own rates.
    /// Oxygen is priced in fixed percentage steps of the tank — the same percentage
    /// the HUD shows — and the buy/sell spread is the house profit.
    /// </summary>
    [Serializable]
    public class OxygenExchange
    {
        [Header("Step size (percentage of the full tank)")]
        [Range(1, 50)]
        [SerializeField] int percentPerStep = 10;

        [Header("Rates (coins per step)")]
        [SerializeField] int buyPrice = 25;
        [SerializeField] int sellPrice = 15;

        [Header("Safety")]
        [Tooltip("Tank percentage the player can never sell below.")]
        [Range(0, 90)]
        [SerializeField] int minPercentReserve = 10;

        public int PercentPerStep => percentPerStep;
        public int BuyPrice => buyPrice;
        public int SellPrice => sellPrice;
        public int MinPercentReserve => minPercentReserve;

        public int PriceOf(TradeMode mode) => mode == TradeMode.Buy ? buyPrice : sellPrice;

        public int MaxSteps(TradeMode mode, Wallet wallet, OxygenTank tank)
        {
            if (wallet == null || tank == null) return 0;

            float step = OxygenPerStep(tank);
            if (step <= 0f) return 0;

            if (mode == TradeMode.Buy)
            {
                if (buyPrice <= 0) return 0;
                // Ceil so the last step can top off a partially empty tank.
                int fits = Mathf.CeilToInt((tank.Max - tank.Current) / step);
                return Mathf.Max(0, Mathf.Min(fits, wallet.Coins / buyPrice));
            }

            return Mathf.Max(0, Mathf.FloorToInt(SellableOxygen(tank) / step));
        }

        public TradeQuote Quote(TradeMode mode, int steps, Wallet wallet, OxygenTank tank)
        {
            if (wallet == null || tank == null || OxygenPerStep(tank) <= 0f)
                return TradeQuote.Invalid(mode, TradeError.Unavailable);

            if (steps <= 0)
                return TradeQuote.Invalid(mode, TradeError.NoSteps);

            return mode == TradeMode.Buy
                ? QuoteBuy(steps, wallet, tank)
                : QuoteSell(steps, tank);
        }

        /// <summary>Prices the trade and applies it. The returned quote is valid only if it went through.</summary>
        public TradeQuote Execute(TradeMode mode, int steps, Wallet wallet, OxygenTank tank)
        {
            // Oxygen drains continuously, so the amount the UI last showed can be
            // slightly stale by the time the player clicks. Trade what is possible
            // right now instead of rejecting the whole transaction.
            steps = Mathf.Min(steps, MaxSteps(mode, wallet, tank));

            var quote = Quote(mode, steps, wallet, tank);
            if (!quote.IsValid) return quote;

            if (mode == TradeMode.Buy)
            {
                if (!wallet.TrySpend(quote.Coins))
                    return TradeQuote.Invalid(mode, TradeError.NotEnoughCoins);

                tank.Restore(quote.Oxygen);
            }
            else
            {
                // Pay before moving the oxygen: Deplete raises OnOxygenChanged, and a
                // subscriber that throws must never be able to swallow the payout.
                wallet.Add(quote.Coins);
                tank.Deplete(quote.Oxygen);
            }

            return quote;
        }

        /// <summary>Oxygen moved by one price step, in tank units.</summary>
        public float OxygenPerStep(OxygenTank tank) =>
            tank == null ? 0f : tank.Max * percentPerStep / 100f;

        TradeQuote QuoteBuy(int steps, Wallet wallet, OxygenTank tank)
        {
            float headroom = tank.Max - tank.Current;
            if (headroom <= 0f)
                return TradeQuote.Invalid(TradeMode.Buy, TradeError.TankFull);

            // A step that only partially fits fills what it can and is charged
            // pro rata, so the player never pays for oxygen the tank can't hold.
            float requested = steps * OxygenPerStep(tank);
            float gained = Mathf.Min(requested, headroom);
            int cost = Mathf.CeilToInt(steps * buyPrice * (gained / requested));

            if (!wallet.CanAfford(cost))
                return TradeQuote.Invalid(TradeMode.Buy, TradeError.NotEnoughCoins);

            return new TradeQuote(TradeMode.Buy, steps, cost, gained, ToPercent(gained, tank), TradeError.None);
        }

        TradeQuote QuoteSell(int steps, OxygenTank tank)
        {
            float given = steps * OxygenPerStep(tank);
            if (given > SellableOxygen(tank))
                return TradeQuote.Invalid(TradeMode.Sell, TradeError.NotEnoughOxygen);

            return new TradeQuote(TradeMode.Sell, steps, steps * sellPrice, given, ToPercent(given, tank), TradeError.None);
        }

        float SellableOxygen(OxygenTank tank) =>
            Mathf.Max(0f, tank.Current - tank.Max * minPercentReserve / 100f);

        static int ToPercent(float oxygen, OxygenTank tank) =>
            tank.Max > 0f ? Mathf.RoundToInt(oxygen / tank.Max * 100f) : 0;
    }
}
