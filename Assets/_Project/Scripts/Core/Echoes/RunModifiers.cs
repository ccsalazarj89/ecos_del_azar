using System;
using System.Collections.Generic;
using UnityEngine;

namespace EcosDelAzar.Core.Echoes
{
    /// <summary>
    /// The Echoes active in the current run and the read-outs other systems
    /// consult. Echoes are consumables: charged ones are spent by
    /// <see cref="TryConsume"/>, timed ones run down through <see cref="Tick"/>.
    /// A spent Echo disappears and can be bought again at the minibar.
    /// Owned by GameManager; persisted through RunPrefs.
    /// </summary>
    public class RunModifiers
    {
        const string ListKey = "echoes";
        const string ChargesSuffix = ".charges";
        const string SecondsSuffix = ".seconds";

        /// <summary>One active Echo with what is left of it.</summary>
        public class EcoState
        {
            public EcoDefinition Definition;
            public int ChargesLeft;
            public float SecondsLeft;

            public bool IsTimed => Definition.Usage == EcoUsage.Timed;
            public bool IsSpent => IsTimed ? SecondsLeft <= 0f : ChargesLeft <= 0;

            /// <summary>Badge text: "×2" for charges, "m:ss" for time.</summary>
            public string RemainingLabel => IsTimed
                ? $"{Mathf.FloorToInt(SecondsLeft / 60f)}:{Mathf.FloorToInt(SecondsLeft % 60f):00}"
                : $"×{ChargesLeft}";
        }

        readonly EcoCatalog catalog;
        readonly List<EcoState> active = new();

        public IReadOnlyList<EcoState> Active => active;
        public EcoCatalog Catalog => catalog;

        /// <summary>Fired when an Echo is bought, spent or expires (and on run reset).</summary>
        public event Action OnChanged;
        /// <summary>Fired when the revive Echo saves the player (value = restored ratio).</summary>
        public event Action<float> OnReviveUsed;

        public RunModifiers(EcoCatalog catalog)
        {
            this.catalog = catalog;
            Reload();
        }

        // ── Read-outs (timed effects) ─────────────────────────────────────

        public float PassiveDrainMultiplier => Product(EcoEffect.PassiveDrain);
        public float ActiveDrainMultiplier => Product(EcoEffect.ActiveDrain);

        /// <summary>Discount that would apply to the next oxygen purchase (1 = none).</summary>
        public float OxygenBuyPriceMultiplier => Peek(EcoEffect.OxygenBuyDiscount, out float v) ? v : 1f;

        public bool Has(EcoEffect effect) => Find(effect) != null;

        // ── Consumption (charged effects) ────────────────────────────────

        /// <summary>Spends one charge of an Echo with this effect. Returns its value.</summary>
        public bool TryConsume(EcoEffect effect, out float value)
        {
            var state = Find(effect);
            if (state == null || state.IsTimed) { value = 1f; return false; }

            value = state.Definition.Value;
            state.ChargesLeft--;
            if (state.IsSpent) active.Remove(state);
            Save();
            OnChanged?.Invoke();

            if (effect == EcoEffect.ReviveOnce) OnReviveUsed?.Invoke(value);
            return true;
        }

        /// <summary>Advances timed Echoes. Called from OxygenTank.Update, so it only runs while the player breathes.</summary>
        public void Tick(float deltaTime)
        {
            bool changed = false;
            for (int i = active.Count - 1; i >= 0; i--)
            {
                var s = active[i];
                if (!s.IsTimed) continue;
                s.SecondsLeft -= deltaTime;
                if (s.IsSpent) { active.RemoveAt(i); changed = true; }
            }

            if (changed) { Save(); OnChanged?.Invoke(); }
        }

        /// <summary>Persist timers (called on scene change so a reload does not refund time).</summary>
        public void SaveTimers() => Save();

        // ── Ownership ────────────────────────────────────────────────────

        public void Acquire(EcoDefinition eco)
        {
            if (eco == null || Owns(eco.Id)) return;
            active.Add(new EcoState
            {
                Definition = eco,
                ChargesLeft = eco.Charges,
                SecondsLeft = eco.DurationSeconds
            });
            Save();
            OnChanged?.Invoke();
        }

        public bool Owns(string id)
        {
            foreach (var s in active)
                if (s.Definition.Id == id) return true;
            return false;
        }

        /// <summary>Re-reads the run prefs (after a new run or a run wipe).</summary>
        public void Reload()
        {
            active.Clear();
            if (catalog != null)
            {
                foreach (var id in RunPrefs.GetString(ListKey, "").Split('|', StringSplitOptions.RemoveEmptyEntries))
                {
                    var def = catalog.Find(id);
                    if (def == null) continue;
                    var s = new EcoState
                    {
                        Definition = def,
                        ChargesLeft = RunPrefs.GetInt(ListKey + "." + id + ChargesSuffix, def.Charges),
                        SecondsLeft = RunPrefs.GetFloat(ListKey + "." + id + SecondsSuffix, def.DurationSeconds)
                    };
                    if (!s.IsSpent) active.Add(s);
                }
            }
            OnChanged?.Invoke();
        }

        void Save()
        {
            var ids = new List<string>(active.Count);
            foreach (var s in active)
            {
                ids.Add(s.Definition.Id);
                RunPrefs.SetInt(ListKey + "." + s.Definition.Id + ChargesSuffix, s.ChargesLeft);
                RunPrefs.SetFloat(ListKey + "." + s.Definition.Id + SecondsSuffix, s.SecondsLeft);
            }
            RunPrefs.SetString(ListKey, string.Join("|", ids));
            RunPrefs.Save();
        }

        EcoState Find(EcoEffect effect)
        {
            foreach (var s in active)
                if (s.Definition.Effect == effect) return s;
            return null;
        }

        bool Peek(EcoEffect effect, out float value)
        {
            var s = Find(effect);
            value = s != null ? s.Definition.Value : 1f;
            return s != null;
        }

        float Product(EcoEffect effect)
        {
            float m = 1f;
            foreach (var s in active)
                if (s.IsTimed && s.Definition.Effect == effect) m *= s.Definition.Value;
            return m;
        }
    }
}
