using System;
using UnityEngine;
using UnityEngine.InputSystem;
using EcosDelAzar.Core;
using EcosDelAzar.Core.Echoes;
using EcosDelAzar.UI;

namespace EcosDelAzar.Shop
{
    public enum ShopResult { Bought, AlreadyOwned, NotEnoughCoins, NotEnoughOxygen, NotEnoughChips, Unavailable }

    /// <summary>
    /// The minibar: sells Echoes for coins, oxygen or house chips. One currency
    /// per Echo (set on its asset). Owns no stock state — anything not yet owned
    /// is for sale, so the shop reads straight from the run's modifiers.
    /// </summary>
    public class EchoShop : InteractableBase
    {
        [Tooltip("Oxygen the player can never spend below, as % of the tank (same idea as the vending machine reserve).")]
        [SerializeField, Range(0, 90)] int minOxygenPercentReserve = 10;
        [SerializeField] EchoShopUI shopUI;
        [SerializeField] InputActionReference exitAction;

        public bool IsOpen { get; private set; }
        public int MinOxygenPercentReserve => minOxygenPercentReserve;

        public event Action<EcoDefinition, ShopResult> OnPurchaseAttempted;

        RunModifiers Modifiers => GameManager.Instance?.Modifiers;

        protected override void OnEnable()
        {
            base.OnEnable();
            if (exitAction?.action == null) return;
            exitAction.action.performed += OnExit;
            exitAction.action.Enable();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (exitAction?.action != null)
                exitAction.action.performed -= OnExit;
        }

        protected override void OnInteract()
        {
            if (IsOpen) return;
            if (shopUI == null || Modifiers?.Catalog == null)
            {
                RaiseInteractionBlocked();
                return;
            }

            IsOpen = true;
            RaiseInteractionStarted();
            shopUI.Open(this);
        }

        public void Close()
        {
            if (!IsOpen) return;
            IsOpen = false;
            shopUI?.Close();
            NotifyHintVisible();
        }

        protected override void OnPlayerExitRange()
        {
            if (!IsOpen) return;
            IsOpen = false;
            shopUI?.Close();
        }

        void OnExit(InputAction.CallbackContext _) => Close();

        public bool CanAfford(EcoDefinition eco) => Check(eco) == ShopResult.Bought;

        /// <summary>Pays and grants the Echo. Never grants without charging.</summary>
        public ShopResult TryBuy(EcoDefinition eco)
        {
            var result = Check(eco);
            if (result == ShopResult.Bought)
            {
                Charge(eco);
                Modifiers.Acquire(eco);
            }

            OnPurchaseAttempted?.Invoke(eco, result);
            return result;
        }

        ShopResult Check(EcoDefinition eco)
        {
            var mods = Modifiers;
            var wallet = GameManager.Instance?.Wallet;
            var tank = GameManager.Instance?.OxygenTank;
            if (eco == null || mods == null || wallet == null || tank == null) return ShopResult.Unavailable;
            if (mods.Owns(eco.Id)) return ShopResult.AlreadyOwned;

            return eco.PriceKind switch
            {
                EcoPriceKind.Coins => wallet.CanAfford(eco.Price) ? ShopResult.Bought : ShopResult.NotEnoughCoins,
                EcoPriceKind.OxygenPercent => SpendableOxygen(tank) >= OxygenCost(eco, tank) ? ShopResult.Bought : ShopResult.NotEnoughOxygen,
                _ => HouseChips.Count >= eco.Price ? ShopResult.Bought : ShopResult.NotEnoughChips
            };
        }

        void Charge(EcoDefinition eco)
        {
            switch (eco.PriceKind)
            {
                case EcoPriceKind.Coins:
                    GameManager.Instance.Wallet.TrySpend(eco.Price);
                    break;
                case EcoPriceKind.OxygenPercent:
                    var tank = GameManager.Instance.OxygenTank;
                    tank.Deplete(OxygenCost(eco, tank));
                    break;
                default:
                    HouseChips.Spend(eco.Price);
                    break;
            }
        }

        float OxygenCost(EcoDefinition eco, OxygenTank tank) => tank.Max * eco.Price / 100f;

        float SpendableOxygen(OxygenTank tank) =>
            Mathf.Max(0f, tank.Current - tank.Max * minOxygenPercentReserve / 100f);
    }
}
