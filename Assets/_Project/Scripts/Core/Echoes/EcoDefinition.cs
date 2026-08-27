using UnityEngine;

namespace EcosDelAzar.Core.Echoes
{
    /// <summary>What an Echo changes. One effect per Echo keeps them readable at a glance.</summary>
    public enum EcoEffect
    {
        /// <summary>Multiplies passive oxygen drain (value = multiplier, e.g. 0.6).</summary>
        PassiveDrain,
        /// <summary>Multiplies active (at-table) oxygen drain (value = multiplier).</summary>
        ActiveDrain,
        /// <summary>Net win multiplier when the player DOUBLED the bet (value = multiplier, e.g. 1.5).</summary>
        DoubleWinBonus,
        /// <summary>The first lost round of every seating refunds the bet (value unused).</summary>
        FirstLossInsurance,
        /// <summary>Once per run, hitting 0 oxygen restores value×tank instead of dying (value = 0..1).</summary>
        ReviveOnce,
        /// <summary>Multiplies the oxygen buy price at vending machines (value = multiplier, e.g. 0.7).</summary>
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

    /// <summary>
    /// A passive upgrade ("Eco") sold at the minibar. Narratively, the echo of a
    /// previous player who died in the casino.
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

        [Header("Minibar price")]
        [SerializeField] EcoPriceKind priceKind = EcoPriceKind.Coins;
        [SerializeField] int price = 100;

        public string Id => id;
        public string DisplayName => displayName;
        public string Description => description;
        public string Glyph => glyph;
        public EcoEffect Effect => effect;
        public float Value => value;
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
