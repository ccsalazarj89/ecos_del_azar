using System;
using UnityEngine;

namespace EcosDelAzar.Core
{
    public class OxygenTank : MonoBehaviour
    {
        const string PrefsKey = "oxygen";

        [Header("Oxygen Settings")]
        [SerializeField] float maxOxygen = 100f;
        [SerializeField] float startingOxygen = 100f;

        [Header("Drain Rates")]
        [SerializeField] float passiveDrainRate = 1f;
        [SerializeField] float activeDrainRate = 5f;

        public float Max => maxOxygen;
        public float Current { get; private set; }
        public float Ratio => maxOxygen > 0f ? Current / maxOxygen : 0f;
        public bool IsEmpty => Current <= 0f;
        public bool IsFull => Current >= maxOxygen;
        public bool IsActiveDrain { get; set; }

        /// <summary>Set by the floor (FloorDrain). Minigame scenes inherit the floor they were entered from.</summary>
        public float FloorDrainMultiplier { get; set; } = 1f;

        /// <summary>
        /// Cuando es true, el tanque no drena nada (p. ej. mientras GameManager.State == MainMenu o Paused).
        /// </summary>
        public bool IsPaused { get; set; }

        public event Action<float> OnOxygenChanged;
        public event Action OnDepleted;

        /// <summary>A notable one-off change (a penalty or a gift), as a signed % of the tank, with its reason.</summary>
        public event Action<int, string> OnOxygenEvent;

        /// <summary>Deplete/Restore with a reason so the HUD can tell the player why the air moved.</summary>
        public void Report(float amount, string reason)
        {
            if (Max <= 0f || Mathf.Abs(amount) < 0.01f) return;
            OnOxygenEvent?.Invoke(Mathf.RoundToInt(amount / Max * 100f), reason);
        }

        void Awake()
        {
            Current = RunPrefs.GetFloat(PrefsKey, startingOxygen);
        }

        void Update()
        {
            if (IsPaused || IsEmpty) return;

            var mods = GameManager.Instance?.Modifiers;
            mods?.Tick(Time.deltaTime);
            float drainRate = IsActiveDrain
                ? activeDrainRate * (mods?.ActiveDrainMultiplier ?? 1f)
                : passiveDrainRate * (mods?.PassiveDrainMultiplier ?? 1f);
            Deplete(drainRate * FloorDrainMultiplier * Time.deltaTime);
        }

        public void Deplete(float amount)
        {
            if (IsEmpty || amount <= 0f) return;

            Current = Mathf.Max(0f, Current - amount);
            NotifyAndSave();

            if (IsEmpty)
                OnDepleted?.Invoke();
        }

        public void Restore(float amount)
        {
            if (amount <= 0f) return;

            Current = Mathf.Min(maxOxygen, Current + amount);
            NotifyAndSave();
        }

        public void Reset()
        {
            Current = startingOxygen;
            NotifyAndSave();
        }

        void NotifyAndSave()
        {
            OnOxygenChanged?.Invoke(Current);
            RunPrefs.SetFloat(PrefsKey, Current);
            RunPrefs.Save();
        }
    }
}
