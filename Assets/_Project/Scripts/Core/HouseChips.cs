using System;
using UnityEngine;

namespace EcosDelAzar.Core
{
    /// <summary>
    /// "Fichas de la casa": one chip per table whose opponent the player bankrupts.
    /// They are the key to the boss floor — earned by beating dealers, not by
    /// hoarding coins. Chips also remember the floor they came from so the boss
    /// can require at least one from the upper floor.
    /// </summary>
    public static class HouseChips
    {
        const string CountKey = "chips.count";
        const string UpperKey = "chips.upper";

        /// <summary>Floor number from which a chip counts as "upper" for boss access.</summary>
        public const int UpperFloorThreshold = 2;

        public static int Count => RunPrefs.GetInt(CountKey, 0);
        public static int UpperFloorCount => RunPrefs.GetInt(UpperKey, 0);

        public static event Action<int> OnChipsChanged;

        public static void Award(int floorNumber)
        {
            RunPrefs.SetInt(CountKey, Count + 1);
            if (floorNumber >= UpperFloorThreshold)
                RunPrefs.SetInt(UpperKey, UpperFloorCount + 1);
            RunPrefs.Save();

            OnChipsChanged?.Invoke(Count);
        }

        public static bool Satisfies(int requiredChips, int requiredUpperFloorChips) =>
            Count >= requiredChips && UpperFloorCount >= requiredUpperFloorChips;

        /// <summary>Spends chips at the minibar. Upper-floor chips are kept as long as possible so boss access is not lost by accident.</summary>
        public static bool Spend(int amount)
        {
            if (amount <= 0 || Count < amount) return false;
            int remaining = Count - amount;
            RunPrefs.SetInt(CountKey, remaining);
            RunPrefs.SetInt(UpperKey, Mathf.Min(UpperFloorCount, remaining));
            RunPrefs.Save();
            OnChipsChanged?.Invoke(remaining);
            return true;
        }
    }
}
