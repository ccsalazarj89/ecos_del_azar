using System;
using UnityEngine;
using UnityEngine.InputSystem;
using EcosDelAzar.Core;
using EcosDelAzar.Core.Echoes;
using EcosDelAzar.UI;

namespace EcosDelAzar.Vending
{
    /// <summary>
    /// Interactable terminal that trades oxygen for coins in both directions.
    /// Owns the rates; <see cref="VendingMachineUI"/> only renders and forwards input.
    /// </summary>
    public class OxygenVendingMachine : InteractableBase
    {
        [SerializeField] VendingMachineUI vendingUI;
        [SerializeField] OxygenExchange exchange = new OxygenExchange();
        [SerializeField] InputActionReference exitAction;

        public OxygenExchange Exchange => exchange;
        public bool IsOpen { get; private set; }

        public Wallet Wallet => GameManager.Instance?.Wallet;
        public OxygenTank Tank => GameManager.Instance?.OxygenTank;

        public event Action<TradeQuote> OnTradeCompleted;
        public event Action<TradeQuote> OnTradeRejected;

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

            if (vendingUI == null || Wallet == null || Tank == null)
            {
                RaiseInteractionBlocked();
                return;
            }

            IsOpen = true;
            RaiseInteractionStarted();
            vendingUI.Open(this);
        }

        public void Close()
        {
            if (!IsOpen) return;
            IsOpen = false;
            vendingUI?.Close();
            NotifyHintVisible();
        }

        protected override void OnPlayerExitRange()
        {
            if (!IsOpen) return;
            IsOpen = false;
            vendingUI?.Close();
        }

        public int MaxSteps(TradeMode mode) => exchange.MaxSteps(mode, Wallet, Tank);

        public TradeQuote Quote(TradeMode mode, int steps) => exchange.Quote(mode, steps, Wallet, Tank);

        public TradeQuote Trade(TradeMode mode, int steps)
        {
            var result = exchange.Execute(mode, steps, Wallet, Tank);

            // "Cambio de manos": a discounted purchase spends one charge.
            if (result.IsValid && mode == TradeMode.Buy)
                GameManager.Instance?.Modifiers?.TryConsume(EcoEffect.OxygenBuyDiscount, out _);

            if (result.IsValid) OnTradeCompleted?.Invoke(result);
            else OnTradeRejected?.Invoke(result);

            return result;
        }

        void OnExit(InputAction.CallbackContext ctx) => Close();
    }
}
