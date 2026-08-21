using UnityEngine;
using UnityEngine.UIElements;
using EcosDelAzar.Core;
using EcosDelAzar.Vending;

namespace EcosDelAzar.UI
{
    /// <summary>
    /// Screen for the oxygen terminal: pick a direction (buy/sell), pick how much
    /// of the tank to move, preview the result, confirm. Everything the player sees
    /// is expressed as a tank percentage; pricing lives in <see cref="OxygenExchange"/>.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class VendingMachineUI : MonoBehaviour
    {
        const string HiddenClass = "vending-hidden";
        const float LowOxygenThreshold = 0.35f;
        const float CriticalOxygenThreshold = 0.15f;
        const float FeedbackDuration = 2.5f;

        VisualElement root;

        Label coinsValue;
        Label oxygenPercent;
        VisualElement oxygenFill;
        VisualElement oxygenGhost;

        Button btnModeBuy;
        Button btnModeSell;
        Label rateText;

        Button btnQtyDown;
        Button btnQtyUp;
        Button btnQtyMin;
        Button btnQtyHalf;
        Button btnQtyMax;
        Label qtyValue;
        Label qtyCaption;

        Label previewCoinsLabel;
        Label previewCoins;
        Label previewOxygen;
        Label feedbackText;

        Button btnConfirm;
        Button btnClose;

        OxygenVendingMachine machine;
        TradeMode mode = TradeMode.Buy;
        int steps = 1;

        string feedbackMessage;
        bool feedbackIsError;
        float feedbackTimer;

        bool initialized;

        void Awake()
        {
            Initialize();
            HideRoot();
        }

        void OnEnable() => Initialize();

        void OnDisable() => UnbindResources();

        void Initialize()
        {
            if (initialized) return;

            var doc = GetComponent<UIDocument>();
            if (doc?.rootVisualElement == null) return;

            var r = doc.rootVisualElement;

            root = r.Q("vending-root");
            if (root == null) return;

            coinsValue = r.Q<Label>("coins-value");
            oxygenPercent = r.Q<Label>("oxygen-percent");
            oxygenFill = r.Q("oxygen-fill");
            oxygenGhost = r.Q("oxygen-ghost");

            btnModeBuy = r.Q<Button>("btn-mode-buy");
            btnModeSell = r.Q<Button>("btn-mode-sell");
            rateText = r.Q<Label>("rate-text");

            btnQtyDown = r.Q<Button>("btn-qty-down");
            btnQtyUp = r.Q<Button>("btn-qty-up");
            btnQtyMin = r.Q<Button>("btn-qty-min");
            btnQtyHalf = r.Q<Button>("btn-qty-half");
            btnQtyMax = r.Q<Button>("btn-qty-max");
            qtyValue = r.Q<Label>("qty-value");
            qtyCaption = r.Q<Label>("qty-caption");

            previewCoinsLabel = r.Q<Label>("preview-coins-label");
            previewCoins = r.Q<Label>("preview-coins");
            previewOxygen = r.Q<Label>("preview-oxygen");
            feedbackText = r.Q<Label>("feedback-text");

            btnConfirm = r.Q<Button>("btn-confirm");
            btnClose = r.Q<Button>("btn-close");

            BindButtons();
            initialized = true;

            // The tree may only exist by OnEnable, so hide once it's actually built.
            HideRoot();
        }

        void BindButtons()
        {
            if (btnModeBuy != null) btnModeBuy.clicked += () => SetMode(TradeMode.Buy);
            if (btnModeSell != null) btnModeSell.clicked += () => SetMode(TradeMode.Sell);

            if (btnQtyDown != null) btnQtyDown.clicked += () => SetSteps(steps - 1);
            if (btnQtyUp != null) btnQtyUp.clicked += () => SetSteps(steps + 1);
            if (btnQtyMin != null) btnQtyMin.clicked += () => SetSteps(1);
            if (btnQtyHalf != null) btnQtyHalf.clicked += () => SetSteps(MaxSteps() / 2);
            if (btnQtyMax != null) btnQtyMax.clicked += () => SetSteps(MaxSteps());

            if (btnConfirm != null) btnConfirm.clicked += Confirm;
            if (btnClose != null) btnClose.clicked += RequestClose;
        }

        // ─── Open / Close ───

        public void Open(OxygenVendingMachine source)
        {
            Initialize();
            if (!initialized || source == null) return;

            UnbindResources();
            machine = source;
            BindResources();

            mode = TradeMode.Buy;
            steps = 1;
            ClearFeedback();

            root.RemoveFromClassList(HiddenClass);
            Refresh();
        }

        public void Close()
        {
            UnbindResources();
            machine = null;
            HideRoot();
        }

        void RequestClose()
        {
            // Route through the machine so it can clear its open state and
            // bring the world hint back.
            if (machine != null) machine.Close();
            else Close();
        }

        void HideRoot()
        {
            if (root != null && !root.ClassListContains(HiddenClass))
                root.AddToClassList(HiddenClass);
        }

        void BindResources()
        {
            if (machine?.Wallet != null) machine.Wallet.OnCoinsChanged += OnCoinsChanged;
            if (machine?.Tank != null) machine.Tank.OnOxygenChanged += OnOxygenChanged;
        }

        void UnbindResources()
        {
            if (machine?.Wallet != null) machine.Wallet.OnCoinsChanged -= OnCoinsChanged;
            if (machine?.Tank != null) machine.Tank.OnOxygenChanged -= OnOxygenChanged;
        }

        // Oxygen drains while the panel is open, so the preview has to track it.
        void OnCoinsChanged(int _) => Refresh();

        void OnOxygenChanged(float _) => Refresh();

        void Update()
        {
            if (machine == null || feedbackTimer <= 0f) return;

            feedbackTimer -= Time.deltaTime;
            if (feedbackTimer <= 0f)
            {
                feedbackMessage = null;
                RefreshFeedback(machine.Quote(mode, steps));
            }
        }

        // ─── Input ───

        void SetMode(TradeMode next)
        {
            if (machine == null || mode == next) return;
            mode = next;
            steps = 1;
            ClearFeedback();
            Refresh();
        }

        void SetSteps(int next)
        {
            if (machine == null) return;
            steps = Mathf.Clamp(next, 1, Mathf.Max(1, MaxSteps()));
            Refresh();
        }

        void Confirm()
        {
            if (machine == null) return;

            var result = machine.Trade(mode, steps);

            if (result.IsValid)
            {
                ShowFeedback(result.Mode == TradeMode.Buy
                    ? $"TANQUE +{result.Percent}% DE O2"
                    : $"+{result.Coins} MONEDAS RECIBIDAS", false);

                steps = Mathf.Clamp(steps, 1, Mathf.Max(1, MaxSteps()));
            }
            else
            {
                ShowFeedback(ErrorText(result.Error), true);
            }

            Refresh();
        }

        // ─── Refresh ───

        int MaxSteps() => machine?.MaxSteps(mode) ?? 0;

        void Refresh()
        {
            if (machine == null || !initialized) return;

            var tank = machine.Tank;
            var wallet = machine.Wallet;
            if (tank == null || wallet == null) return;

            int max = MaxSteps();
            steps = max <= 0 ? 0 : Mathf.Clamp(steps, 1, max);

            var quote = machine.Quote(mode, steps);

            RefreshModeTabs();
            RefreshRate();
            RefreshGauges(tank, wallet, quote);
            RefreshQuantity(max, quote);
            RefreshPreview(tank, quote);
            RefreshConfirm(quote);
            RefreshFeedback(quote);
        }

        void RefreshModeTabs()
        {
            btnModeBuy?.EnableInClassList("mode-tab--active", mode == TradeMode.Buy);
            btnModeSell?.EnableInClassList("mode-tab--active", mode == TradeMode.Sell);
        }

        void RefreshRate()
        {
            var exchange = machine.Exchange;

            SetText(rateText, mode == TradeMode.Buy
                ? $"CADA {exchange.PercentPerStep}% DE O2 CUESTA {exchange.BuyPrice} MONEDAS"
                : $"CADA {exchange.PercentPerStep}% DE O2 TE PAGA {exchange.SellPrice} MONEDAS");
        }

        void RefreshGauges(OxygenTank tank, Wallet wallet, TradeQuote quote)
        {
            SetText(coinsValue, wallet.Coins.ToString());

            float ratio = tank.Ratio;
            SetText(oxygenPercent, $"{Mathf.RoundToInt(ratio * 100f)}%");

            float targetRatio = ProjectedRatio(tank, quote);
            float solid = Mathf.Min(ratio, targetRatio);
            float ghost = Mathf.Max(ratio, targetRatio);

            if (oxygenFill != null)
            {
                oxygenFill.style.width = new Length(solid * 100f, LengthUnit.Percent);
                oxygenFill.EnableInClassList("oxygen-low", ratio <= LowOxygenThreshold && ratio > CriticalOxygenThreshold);
                oxygenFill.EnableInClassList("oxygen-critical", ratio <= CriticalOxygenThreshold);
            }

            if (oxygenGhost != null)
            {
                oxygenGhost.style.width = new Length(ghost * 100f, LengthUnit.Percent);
                oxygenGhost.EnableInClassList("ghost--loss", targetRatio < ratio);
            }
        }

        // The headline number is the percentage the tank actually moves, so a
        // step that only partially fits reads as the smaller amount it really gives.
        void RefreshQuantity(int max, TradeQuote quote)
        {
            bool buying = mode == TradeMode.Buy;
            int percent = quote.IsValid ? quote.Percent : 0;

            SetText(qtyValue, buying ? $"+{percent}%" : $"-{percent}%");
            SetText(qtyCaption, buying ? "AL TANQUE" : "DEL TANQUE");

            btnQtyDown?.SetEnabled(steps > 1);
            btnQtyUp?.SetEnabled(steps < max);
            btnQtyMin?.SetEnabled(max > 0);
            btnQtyHalf?.SetEnabled(max > 1);
            btnQtyMax?.SetEnabled(max > 0);
        }

        void RefreshPreview(OxygenTank tank, TradeQuote quote)
        {
            bool buying = mode == TradeMode.Buy;

            SetText(previewCoinsLabel, buying ? "COSTE" : "PAGO");

            if (previewCoins != null)
            {
                SetText(previewCoins, !quote.IsValid ? "—"
                    : buying ? $"-{quote.Coins}" : $"+{quote.Coins}");

                previewCoins.EnableInClassList("value--cost", quote.IsValid && buying);
                previewCoins.EnableInClassList("value--gain", quote.IsValid && !buying);
                previewCoins.EnableInClassList("preview-value--muted", !quote.IsValid);
            }

            if (previewOxygen != null)
            {
                int from = Mathf.RoundToInt(tank.Ratio * 100f);
                int to = Mathf.RoundToInt(ProjectedRatio(tank, quote) * 100f);

                SetText(previewOxygen, quote.IsValid ? $"{from}%  →  {to}%" : $"{from}%");

                previewOxygen.EnableInClassList("value--cost", quote.IsValid && !buying);
                previewOxygen.EnableInClassList("preview-value--muted", !quote.IsValid);
            }
        }

        void RefreshConfirm(TradeQuote quote)
        {
            if (btnConfirm == null) return;

            bool buying = mode == TradeMode.Buy;

            btnConfirm.SetEnabled(quote.IsValid);
            btnConfirm.EnableInClassList("confirm--sell", !buying);

            btnConfirm.text = !quote.IsValid
                ? (buying ? "COMPRAR" : "VENDER")
                : buying ? $"COMPRAR +{quote.Percent}%  ·  {quote.Coins} MONEDAS"
                         : $"VENDER -{quote.Percent}%  ·  +{quote.Coins} MONEDAS";
        }

        void RefreshFeedback(TradeQuote quote)
        {
            if (feedbackText == null) return;

            // A transaction message wins until it expires; otherwise explain why
            // the current selection can't be confirmed.
            string message = feedbackMessage;
            bool isError = feedbackIsError;

            if (message == null && !quote.IsValid)
            {
                message = BlockedText();
                isError = true;
            }

            SetText(feedbackText, message ?? string.Empty);
            feedbackText.EnableInClassList("feedback--ok", message != null && !isError);
            feedbackText.EnableInClassList("feedback--error", message != null && isError);
        }

        float ProjectedRatio(OxygenTank tank, TradeQuote quote)
        {
            if (tank.Max <= 0f) return 0f;
            if (!quote.IsValid) return tank.Ratio;

            float projected = quote.Mode == TradeMode.Buy
                ? tank.Current + quote.Oxygen
                : tank.Current - quote.Oxygen;

            return Mathf.Clamp01(projected / tank.Max);
        }

        string BlockedText()
        {
            var tank = machine.Tank;

            if (mode == TradeMode.Buy)
            {
                if (tank != null && tank.IsFull) return "TANQUE LLENO";
                return "MONEDAS INSUFICIENTES";
            }

            return $"RESERVA MÍNIMA DEL {machine.Exchange.MinPercentReserve}%";
        }

        string ErrorText(TradeError error) => error switch
        {
            TradeError.NotEnoughCoins => "MONEDAS INSUFICIENTES",
            TradeError.NotEnoughOxygen => "O2 INSUFICIENTE",
            TradeError.TankFull => "TANQUE LLENO",
            TradeError.NoSteps => "CANTIDAD NO VÁLIDA",
            _ => "TERMINAL NO DISPONIBLE"
        };

        void ShowFeedback(string message, bool isError)
        {
            feedbackMessage = message;
            feedbackIsError = isError;
            feedbackTimer = FeedbackDuration;
        }

        void ClearFeedback()
        {
            feedbackMessage = null;
            feedbackIsError = false;
            feedbackTimer = 0f;
        }

        static void SetText(Label label, string value)
        {
            if (label != null && label.text != value)
                label.text = value;
        }
    }
}
