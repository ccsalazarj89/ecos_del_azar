using UnityEngine;

namespace EcosDelAzar.Core.Echoes
{
    /// <summary>What an Echo changes. One effect per Echo keeps them readable at a glance.</summary>
    public enum EcoEffect
    {
        /// <summary>Multiplies passive oxygen drain while active (value = multiplier, e.g. 0.5). Timed.</summary>
        PassiveDrain,
        /// <summary>Multiplies active (at-table) oxygen drain while active (value = multiplier). Timed.</summary>
        ActiveDrain,
        /// <summary>Net win multiplier on a DOUBLED win (value = multiplier, e.g. 1.5). One charge per use.</summary>
        DoubleWinBonus,
        /// <summary>A lost round refunds the bet (value unused). One charge per use.</summary>
        FirstLossInsurance,
        /// <summary>Hitting 0 oxygen restores value×tank instead of dying (value = 0..1). One charge per use.</summary>
        ReviveOnce,
        /// <summary>Multiplies the oxygen buy price (value = multiplier, e.g. 0.5). One charge per purchase.</summary>
        OxygenBuyDiscount
    }

    /// <summary>What the minibar charges for an Echo. One currency per Echo, chosen by theme.</summary>
    public enum EcoPriceKind
    {
        /// <summary>Coins from the wallet.</summary>
        Coins,
        /// <summary>Percent of the oxygen tank, drained on purchase.</summary>
        OxygenPercent,
        /// <summary>House chips — delays the boss in exchange for power.</summary>
        Chips
    }

    /// <summary>How an Echo runs out: a number of uses, or a stretch of game time.</summary>
    public enum EcoUsage { Charges, Timed }

    /// <summary>
    /// A consumable upgrade ("Eco") sold at the minibar. Echoes never change the
    /// rules permanently: they are spent by use or by time, then can be bought
    /// again. Narratively, the echo of a previous player who died in the casino.
    /// </summary>
    [CreateAssetMenu(fileName = "SO_Eco", menuName = "Ecos del Azar/Echo")]
    public class EcoDefinition : ScriptableObject
    {
        [SerializeField] string id;
        [SerializeField] string displayName;
        [TextArea] [SerializeField] string description;
        [Tooltip("Short glyph shown on the HUD badge. Use characters the default UI font has (letters, digits, arrows).")]
        [SerializeField] string glyph = "?";
        [SerializeField] EcoEffect effect;
        [SerializeField] float value = 1f;

        [Header("How it runs out")]
        [SerializeField] EcoUsage usage = EcoUsage.Charges;
        [Tooltip("Uses before the Echo is gone (Charges).")]
        [SerializeField] int charges = 1;
        [Tooltip("Seconds of game time the Echo stays active (Timed). Only counts while oxygen drains.")]
        [SerializeField] float durationSeconds = 180f;

        [Header("Minibar price")]
        [SerializeField] EcoPriceKind priceKind = EcoPriceKind.Coins;
        [SerializeField] int price = 100;

        public string Id => id;
        public string DisplayName => displayName;
        public string Description => description;
        public string Glyph => glyph;
        public EcoEffect Effect => effect;
        public float Value => value;
        public EcoUsage Usage => usage;
        public int Charges => charges;
        public float DurationSeconds => durationSeconds;
        public EcoPriceKind PriceKind => priceKind;
        public int Price => price;

        /// <summary>Player-facing price tag, e.g. "150 MONEDAS", "30% DE O2", "1 FICHA".</summary>
        public string PriceLabel => priceKind switch
        {
            EcoPriceKind.Coins => $"{price} MONEDAS",
            EcoPriceKind.OxygenPercent => $"{price}% DE O2",
            _ => price == 1 ? "1 FICHA" : $"{price} FICHAS"
        };
    }
}
