using System;
using System.Collections.Generic;
using UnityEngine;

namespace EcosDelAzar.Core.Echoes
{
    /// <summary>
    /// The Echoes active in the current run and the aggregated modifiers other
    /// systems read (oxygen drain, betting payouts, vending prices, revive).
    /// Owned by GameManager; persisted through RunPrefs so it survives reloads
    /// and is wiped with the run. Echoes are bought at the minibar (EchoShop).
    /// </summary>
    public class RunModifiers
    {
        const string OwnedKey = "echoes";
        const string ReviveUsedKey = "echoes.reviveUsed";

        readonly EcoCatalog catalog;
        readonly List<EcoDefinition> owned = new();

        public IReadOnlyList<EcoDefinition> Owned => owned;
        public EcoCatalog Catalog => catalog;

        /// <summary>Fired when the set of owned Echoes changes (purchase or run reset).</summary>
        public event Action OnChanged;
        /// <summary>Fired when the revive Echo saves the player (value = restored ratio).</summary>
        public event Action<float> OnReviveUsed;

        public RunModifiers(EcoCatalog catalog)
        {
            this.catalog = catalog;
            Reload();
        }

        // ── Aggregated modifiers ──────────────────────────────────────────

        public float PassiveDrainMultiplier => Product(EcoEffect.PassiveDrain);
        public float ActiveDrainMultiplier => Product(EcoEffect.ActiveDrain);
        public float OxygenBuyPriceMultiplier => Product(EcoEffect.OxygenBuyDiscount);
        public float DoubleWinMultiplier => Product(EcoEffect.DoubleWinBonus);
        public bool HasFirstLossInsurance => Has(EcoEffect.FirstLossInsurance);

        public bool HasReviveAvailable =>
            Has(EcoEffect.ReviveOnce) && RunPrefs.GetInt(ReviveUsedKey, 0) == 0;

        /// <summary>Spends the one-time revive. Returns the tank ratio to restore, or 0 when unavailable.</summary>
        public float TryConsumeRevive()
        {
            if (!HasReviveAvailable) return 0f;
            RunPrefs.SetInt(ReviveUsedKey, 1);
            RunPrefs.Save();

            float ratio = 0f;
            foreach (var e in owned)
                if (e.Effect == EcoEffect.ReviveOnce) ratio = Mathf.Max(ratio, e.Value);

            OnReviveUsed?.Invoke(ratio);
            return ratio;
        }

        // ── Ownership ────────────────────────────────────────────────────

        public void Acquire(EcoDefinition eco)
        {
            if (eco == null || Owns(eco.Id)) return;
            owned.Add(eco);
            Save();
            OnChanged?.Invoke();
        }

        public bool Owns(string id)
        {
            foreach (var e in owned)
                if (e.Id == id) return true;
            return false;
        }

        /// <summary>Re-reads the run prefs (after a new run or a run wipe).</summary>
        public void Reload()
        {
            owned.Clear();
            if (catalog != null)
            {
                foreach (var id in RunPrefs.GetString(OwnedKey, "").Split('|', StringSplitOptions.RemoveEmptyEntries))
                {
                    var def = catalog.Find(id);
                    if (def != null) owned.Add(def);
                }
            }
            OnChanged?.Invoke();
        }

        void Save()
        {
            var ids = new List<string>(owned.Count);
            foreach (var e in owned) ids.Add(e.Id);
            RunPrefs.SetString(OwnedKey, string.Join("|", ids));
            RunPrefs.Save();
        }

        float Product(EcoEffect effect)
        {
            float m = 1f;
            foreach (var e in owned)
                if (e.Effect == effect) m *= e.Value;
            return m;
        }

        bool Has(EcoEffect effect)
        {
            foreach (var e in owned)
                if (e.Effect == effect) return true;
            return false;
        }
    }
}
