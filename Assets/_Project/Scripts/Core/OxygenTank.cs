using System;
using UnityEngine;

namespace EcosDelAzar.Core
{
    public class OxygenTank : MonoBehaviour
    {
        const string PrefsKey = "player_oxygen";

        [Header("Oxygen Settings")]
        [SerializeField] float maxOxygen = 100f;
        [SerializeField] float startingOxygen = 100f;

        [Header("Refill Settings")]
        [SerializeField] float oxygenPerCoin = 5f;
        [SerializeField] int coinCostPerRefill = 10;

        [Header("Drain Rates")]
        [SerializeField] float passiveDrainRate = 1f;
        [SerializeField] float activeDrainRate = 5f;

        public float Max => maxOxygen;
        public float Current { get; private set; }
        public float Ratio => maxOxygen > 0f ? Current / maxOxygen : 0f;
        public bool IsEmpty => Current <= 0f;
        public bool IsFull => Current >= maxOxygen;
        public bool IsActiveDrain { get; set; }

        /// <summary>
        /// Cuando es true, el tanque no drena nada (p. ej. mientras GameManager.State == MainMenu o Paused).
        /// </summary>
        public bool IsPaused { get; set; }

        public event Action<float> OnOxygenChanged;
        public event Action OnDepleted;

        void Awake()
        {
            Current = PlayerPrefs.GetFloat(PrefsKey, startingOxygen);
        }

        void Update()
        {
            if (IsPaused || IsEmpty) return;

            float drainRate = IsActiveDrain ? activeDrainRate : passiveDrainRate;
            Deplete(drainRate * Time.deltaTime);
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

        public bool TryBuyOxygen(Wallet wallet)
        {
            if (wallet == null) return false;
            if (IsFull) return false;
            if (!wallet.TrySpend(coinCostPerRefill)) return false;

            Restore(oxygenPerCoin);
            return true;
        }

        public void Reset()
        {
            Current = startingOxygen;
            NotifyAndSave();
        }

        void NotifyAndSave()
        {
            OnOxygenChanged?.Invoke(Current);
            PlayerPrefs.SetFloat(PrefsKey, Current);
            PlayerPrefs.Save();
        }
    }
}
