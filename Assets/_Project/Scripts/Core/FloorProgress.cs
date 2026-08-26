using UnityEngine;
using EcosDelAzar.Elevator;

namespace EcosDelAzar.Core
{
    /// <summary>
    /// Which elevator floors are open in the current run: unlocked by default,
    /// bought with coins, or earned with house chips.
    /// </summary>
    public class FloorProgress : MonoBehaviour
    {
        const string FloorUnlockPrefix = "floor.";

        public bool IsUnlocked(ElevatorFloorData floor)
        {
            if (floor == null) return false;
            if (floor.UnlockedByDefault) return true;
            if (floor.RequiresChips)
                return HouseChips.Satisfies(floor.RequiredChips, floor.RequiredUpperFloorChips);
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
            if (!floor.CanBePurchased || floor.RequiresChips) return false;
            if (!wallet.TrySpend(floor.AccessCost)) return false;
            Unlock(floor);
            return true;
        }
    }
}
