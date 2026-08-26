using UnityEngine;
using UnityEngine.InputSystem;

namespace EcosDelAzar.Core
{
    /// <summary>
    /// Testing helpers for coins and oxygen. Lives on the GameManager prefab.
    /// Hotkeys work in the Editor and development builds only; the inspector
    /// fields + context-menu commands work anywhere in the Editor.
    /// </summary>
    public class DebugCheats : MonoBehaviour
    {
        [Header("Hotkeys (Editor / dev build)")]
        [SerializeField] bool hotkeysEnabled = true;
        [SerializeField] int coinsStep = 100;
        [Range(1f, 100f)] [SerializeField] float oxygenStepPercent = 25f;

        [Header("Set exact values, then use the ⋮ menu → 'Apply'")]
        [SerializeField] int coinsToSet = 500;
        [Range(0f, 100f)] [SerializeField] float oxygenPercentToSet = 100f;

        Wallet Wallet => GameManager.Instance?.Wallet;
        OxygenTank Tank => GameManager.Instance?.OxygenTank;

        void Update()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (!hotkeysEnabled) return;
            var kb = Keyboard.current;
            if (kb == null) return;

            if (kb.f1Key.wasPressedThisFrame) AddCoins(coinsStep);
            if (kb.f2Key.wasPressedThisFrame) AddCoins(-coinsStep);
            if (kb.f3Key.wasPressedThisFrame) AddOxygenPercent(oxygenStepPercent);
            if (kb.f4Key.wasPressedThisFrame) AddOxygenPercent(-oxygenStepPercent);
            if (kb.f5Key.wasPressedThisFrame) ApplyValues();
#endif
        }

        [ContextMenu("Apply coins + oxygen values")]
        public void ApplyValues()
        {
            Wallet?.Set(Mathf.Max(0, coinsToSet));
            SetOxygenPercent(oxygenPercentToSet);
            Debug.Log($"[DebugCheats] Coins = {coinsToSet}, O2 = {oxygenPercentToSet}%");
        }

        [ContextMenu("Coins +100")] void CoinsPlus() => AddCoins(coinsStep);
        [ContextMenu("Coins -100")] void CoinsMinus() => AddCoins(-coinsStep);
        [ContextMenu("Oxygen +25%")] void O2Plus() => AddOxygenPercent(oxygenStepPercent);
        [ContextMenu("Oxygen -25%")] void O2Minus() => AddOxygenPercent(-oxygenStepPercent);
        [ContextMenu("Oxygen to 1% (near death)")] void O2Critical() => SetOxygenPercent(1f);

        void AddCoins(int amount)
        {
            if (Wallet == null) return;
            if (amount >= 0) Wallet.Add(amount);
            else Wallet.Set(Mathf.Max(0, Wallet.Coins + amount));
        }

        void AddOxygenPercent(float percent)
        {
            if (Tank == null) return;
            float amount = Tank.Max * Mathf.Abs(percent) / 100f;
            if (percent >= 0) Tank.Restore(amount);
            else Tank.Deplete(amount);
        }

        void SetOxygenPercent(float percent)
        {
            if (Tank == null) return;
            float target = Tank.Max * Mathf.Clamp01(percent / 100f);
            float delta = target - Tank.Current;
            if (delta > 0) Tank.Restore(delta);
            else if (delta < 0) Tank.Deplete(-delta);
        }
    }
}
