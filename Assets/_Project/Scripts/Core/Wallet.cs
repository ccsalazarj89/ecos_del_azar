using System;
using UnityEngine;

namespace EcosDelAzar.Core
{
    public class Wallet : MonoBehaviour
    {
        const string PrefsKey = "player_coins";

        [SerializeField] int initialCoins = 1000;

        public int Coins { get; private set; }

        public event Action<int> OnCoinsChanged;

        void Awake()
        {
            // TODO: quitar el DeleteKey cuando termine el testing
            PlayerPrefs.DeleteKey(PrefsKey);
            Coins = PlayerPrefs.GetInt(PrefsKey, initialCoins);
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

        public void ResetToInitial()
        {
            Coins = initialCoins;
            Save();
        }

        void Save()
        {
            PlayerPrefs.SetInt(PrefsKey, Coins);
            PlayerPrefs.Save();
            OnCoinsChanged?.Invoke(Coins);
        }
    }
}
