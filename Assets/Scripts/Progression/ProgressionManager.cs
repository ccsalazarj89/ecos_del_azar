using UnityEngine;

public static class ProgressManager
{
    private const string FloorUnlockPrefix = "floor_unlocked_";
    private const string CurrencyKey = "player_currency";

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
        if (floor == null)
        {
            return false;
        }

        if (floor.UnlockedByDefault)
        {
            return true;
        }

        return PlayerPrefs.GetInt(FloorUnlockPrefix + floor.FloorId, 0) == 1;
    }

    public static void UnlockFloor(ElevatorFloorData floor)
    {
        if (floor == null)
        {
            Debug.LogWarning("Tried to unlock a null floor.");
            return;
        }

        PlayerPrefs.SetInt(FloorUnlockPrefix + floor.FloorId, 1);
        PlayerPrefs.Save();

        Debug.Log($"Floor unlocked: {floor.DisplayName}");
    }

    public static bool CanAfford(int amount)
    {
        return Currency >= amount;
    }

    public static void AddCurrency(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        Currency += amount;

        Debug.Log($"Currency added: {amount}. Current currency: {Currency}");
    }

    public static bool TrySpendCurrency(int amount)
    {
        if (amount <= 0)
        {
            return true;
        }

        if (!CanAfford(amount))
        {
            return false;
        }

        Currency -= amount;

        Debug.Log($"Currency spent: {amount}. Current currency: {Currency}");

        return true;
    }

    public static bool TryPurchaseFloorAccess(ElevatorFloorData floor)
    {
        if (floor == null)
        {
            Debug.LogWarning("Cannot purchase access because floor is null.");
            return false;
        }

        if (IsFloorUnlocked(floor))
        {
            Debug.Log($"Floor already unlocked: {floor.DisplayName}");
            return true;
        }

        if (!floor.CanBePurchased)
        {
            Debug.Log($"Floor cannot be purchased: {floor.DisplayName}");
            return false;
        }

        if (!TrySpendCurrency(floor.AccessCost))
        {
            Debug.Log($"Not enough currency to purchase: {floor.DisplayName}");
            return false;
        }

        UnlockFloor(floor);

        return true;
    }

    public static void ResetProgress()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        Debug.Log("Progress reset.");
    }
}