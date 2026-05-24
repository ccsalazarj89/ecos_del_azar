using UnityEngine;
using EcosDelAzar.Elevator;

namespace EcosDelAzar.Core
{
    /// <summary>
    /// Which elevator floors are unlocked.
    /// </summary>
    public class FloorProgress : MonoBehaviour
    {
        const string FloorUnlockPrefix = "floor_unlocked_";

        public bool IsUnlocked(ElevatorFloorData floor)
        {
            if (floor == null) return false;
            if (floor.UnlockedByDefault) return true;
            return PlayerPrefs.GetInt(FloorUnlockPrefix + floor.FloorId, 0) == 1;
        }

        public void Unlock(ElevatorFloorData floor)
        {
            if (floor == null) return;
            PlayerPrefs.SetInt(FloorUnlockPrefix + floor.FloorId, 1);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Try to purchase floor access using the given Wallet.
        /// </summary>
        public bool TryPurchaseAccess(ElevatorFloorData floor, Wallet wallet)
        {
            if (floor == null || wallet == null) return false;
            if (IsUnlocked(floor)) return true;
            if (!floor.CanBePurchased) return false;
            if (!wallet.TrySpend(floor.AccessCost)) return false;
            Unlock(floor);
            return true;
        }

        public void ResetAll()
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
        }
    }
}
