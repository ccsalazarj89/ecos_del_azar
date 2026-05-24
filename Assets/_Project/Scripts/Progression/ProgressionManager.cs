using UnityEngine;
using EcosDelAzar.Elevator;

namespace EcosDelAzar.Progression
{
    public static class ProgressManager
    {
        const string FloorUnlockPrefix = "floor_unlocked_";
        const string CurrencyKey = "player_currency";

        public static int Currency
        {
            get => PlayerPrefs.GetInt(CurrencyKey, 0);
            private set
            {
                PlayerPrefs.SetInt(CurrencyKey, Mathf.Max(0, value));
                PlayerPrefs.Save();
            }
        }

        public static bool IsFloorUnlocked(ElevatorFloorData floor)
        {
            if (floor == null) return false;
            if (floor.UnlockedByDefault) return true;
            return PlayerPrefs.GetInt(FloorUnlockPrefix + floor.FloorId, 0) == 1;
        }

        public static void UnlockFloor(ElevatorFloorData floor)
        {
            if (floor == null) return;
            PlayerPrefs.SetInt(FloorUnlockPrefix + floor.FloorId, 1);
            PlayerPrefs.Save();
        }

        public static bool CanAfford(int amount) => Currency >= amount;

        public static void AddCurrency(int amount)
        {
            if (amount <= 0) return;
            Currency += amount;
        }

        public static bool TrySpendCurrency(int amount)
        {
            if (amount <= 0) return true;
            if (!CanAfford(amount)) return false;
            Currency -= amount;
            return true;
        }

        public static bool TryPurchaseFloorAccess(ElevatorFloorData floor)
        {
            if (floor == null) return false;
            if (IsFloorUnlocked(floor)) return true;
            if (!floor.CanBePurchased) return false;
            if (!TrySpendCurrency(floor.AccessCost)) return false;
            UnlockFloor(floor);
            return true;
        }

        public static void ResetProgress()
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
        }
    }
}
