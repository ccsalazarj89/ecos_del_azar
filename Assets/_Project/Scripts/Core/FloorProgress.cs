using UnityEngine;
using EcosDelAzar.Elevator;

namespace EcosDelAzar.Core
{
    /// <summary>
    /// Which elevator floors are unlocked in the current run.
    /// </summary>
    public class FloorProgress : MonoBehaviour
    {
        const string FloorUnlockPrefix = "floor.";

        public bool IsUnlocked(ElevatorFloorData floor)
        {
            if (floor == null) return false;
            if (floor.UnlockedByDefault) return true;
            return RunPrefs.GetInt(FloorUnlockPrefix + floor.FloorId, 0) == 1;
        }

        public void Unlock(ElevatorFloorData floor)
        {
            if (floor == null) return;
            RunPrefs.SetInt(FloorUnlockPrefix + floor.FloorId, 1);
            RunPrefs.Save();
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
    }
}
