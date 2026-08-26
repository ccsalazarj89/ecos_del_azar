using System;
using UnityEngine;

namespace EcosDelAzar.Core
{
    public class Wallet : MonoBehaviour
    {
        const string PrefsKey = "coins";

        [SerializeField] int initialCoins = 0;

        public int Coins { get; private set; }

        public event Action<int> OnCoinsChanged;

        void Awake()
        {
            Coins = RunPrefs.GetInt(PrefsKey, initialCoins);
        }

        public bool CanAfford(int amount) => amount >= 0 && Coins >= amount;

        public void Add(int amount)
        {
            if (amount <= 0) return;
            Coins += amount;
            Save();
        }

        public bool TrySpend(int amount)
        {
            if (amount <= 0) return true;
            if (!CanAfford(amount)) return false;
            Coins -= amount;
            Save();
            return true;
        }

        public void Set(int amount)
        {
            Coins = Mathf.Max(0, amount);
            Save();
        }

        /// <summary>Starting coins of a new run. Only RunState should call this.</summary>
        public void ResetToInitial()
        {
            Coins = initialCoins;
            Save();
        }

        void Save()
        {
            RunPrefs.SetInt(PrefsKey, Coins);
            RunPrefs.Save();
            OnCoinsChanged?.Invoke(Coins);
        }
    }
}
