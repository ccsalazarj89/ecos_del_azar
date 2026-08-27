using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using EcosDelAzar.Core;
using EcosDelAzar.Core.Echoes;

namespace EcosDelAzar.Shop
{
    /// <summary>
    /// Minibar panel: one card per Echo in the catalog with its price tag.
    /// Only renders and forwards clicks; EchoShop decides and charges.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class EchoShopUI : MonoBehaviour
    {
        const string HiddenClass = "shop-hidden";
        const float FeedbackSeconds = 2.5f;

        VisualElement root;
        VisualElement cards;
        Label coinsLabel;
        Label oxygenLabel;
        Label chipsLabel;
        Label feedback;
        Button btnClose;

        EchoShop shop;
        readonly Dictionary<string, Button> cardByEcoId = new();
        Coroutine feedbackRoutine;
        bool initialized;

        void Initialize()
        {
            if (initialized) return;
            var doc = GetComponent<UIDocument>();
            if (doc?.rootVisualElement == null) return;

            var r = doc.rootVisualElement;
            root = r.Q("shop-root");
            cards = r.Q("shop-cards");
            coinsLabel = r.Q<Label>("shop-coins");
            oxygenLabel = r.Q<Label>("shop-oxygen");
            chipsLabel = r.Q<Label>("shop-chips");
            feedback = r.Q<Label>("shop-feedback");
            btnClose = r.Q<Button>("btn-shop-close");

            if (btnClose != null) btnClose.clicked += () => shop?.Close();
            initialized = true;
        }

        void OnEnable()
        {
            Initialize();
            root?.AddToClassList(HiddenClass);
        }

        public void Open(EchoShop source)
        {
            Initialize();
            if (!initialized || source == null) return;

            shop = source;
            shop.OnPurchaseAttempted += OnPurchase;
            BindResources();

            BuildCards();
            ClearFeedback();
            root.RemoveFromClassList(HiddenClass);
            Refresh();
        }

        public void Close()
        {
            if (shop != null) shop.OnPurchaseAttempted -= OnPurchase;
            UnbindResources();
            shop = null;
            root?.AddToClassList(HiddenClass);
        }

        void BindResources()
        {
            var gm = GameManager.Instance;
            if (gm?.Wallet != null) gm.Wallet.OnCoinsChanged += OnResourceChanged;
            if (gm?.OxygenTank != null) gm.OxygenTank.OnOxygenChanged += OnResourceChanged;
            HouseChips.OnChipsChanged += OnResourceChanged;
        }

        void UnbindResources()
        {
            var gm = GameManager.Instance;
            if (gm?.Wallet != null) gm.Wallet.OnCoinsChanged -= OnResourceChanged;
            if (gm?.OxygenTank != null) gm.OxygenTank.OnOxygenChanged -= OnResourceChanged;
            HouseChips.OnChipsChanged -= OnResourceChanged;
        }

        void OnResourceChanged(int _) => Refresh();
        void OnResourceChanged(float _) => Refresh();

        void BuildCards()
        {
            if (cards == null) return;
            cards.Clear();
            cardByEcoId.Clear();

            var catalog = GameManager.Instance?.Modifiers?.Catalog;
            if (catalog == null) return;

            foreach (var eco in catalog.All)
            {
                if (eco == null) continue;
                var card = BuildCard(eco);
                cards.Add(card);
                cardByEcoId[eco.Id] = card;
            }
        }

        Button BuildCard(EcoDefinition eco)
        {
            var card = new Button { name = $"shop-card-{eco.Id}" };
            card.AddToClassList("shop-card");

            var glyph = new Label(eco.Glyph);
            glyph.AddToClassList("shop-card__glyph");

            var title = new Label(eco.DisplayName);
            title.AddToClassList("shop-card__title");

            var desc = new Label(eco.Description);
            desc.AddToClassList("shop-card__desc");

            var price = new Label(eco.PriceLabel) { name = "price" };
            price.AddToClassList("shop-card__price");
            price.AddToClassList(PriceClass(eco.PriceKind));

            card.Add(glyph);
            card.Add(title);
            card.Add(desc);
            card.Add(price);
            card.clicked += () => shop?.TryBuy(eco);
            return card;
        }

        static string PriceClass(EcoPriceKind kind) => kind switch
        {
            EcoPriceKind.Coins => "shop-card__price--coins",
            EcoPriceKind.OxygenPercent => "shop-card__price--oxygen",
            _ => "shop-card__price--chips"
        };

        void Refresh()
        {
            var gm = GameManager.Instance;
            if (gm == null || shop == null) return;

            if (coinsLabel != null) coinsLabel.text = gm.Wallet != null ? gm.Wallet.Coins.ToString() : "0";
            if (oxygenLabel != null) oxygenLabel.text = gm.OxygenTank != null ? $"{Mathf.RoundToInt(gm.OxygenTank.Ratio * 100f)}%" : "0%";
            if (chipsLabel != null) chipsLabel.text = HouseChips.Count.ToString();

            var mods = gm.Modifiers;
            foreach (var pair in cardByEcoId)
            {
                var eco = mods?.Catalog?.Find(pair.Key);
                var card = pair.Value;
                if (eco == null) continue;

                bool owned = mods.Owns(eco.Id);
                bool affordable = !owned && shop.CanAfford(eco);

                card.EnableInClassList("shop-card--owned", owned);
                card.EnableInClassList("shop-card--locked", !owned && !affordable);
                card.SetEnabled(!owned);

                var price = card.Q<Label>("price");
                if (price != null) price.text = owned ? "ADQUIRIDO" : eco.PriceLabel;
            }
        }

        void OnPurchase(EcoDefinition eco, ShopResult result)
        {
            Refresh();
            ShowFeedback(result switch
            {
                ShopResult.Bought => $"{eco.DisplayName.ToUpperInvariant()} SE QUEDA CONTIGO",
                ShopResult.AlreadyOwned => "YA LO TIENES",
                ShopResult.NotEnoughCoins => "MONEDAS INSUFICIENTES",
                ShopResult.NotEnoughOxygen => $"O2 INSUFICIENTE (RESERVA DEL {shop.MinOxygenPercentReserve}%)",
                ShopResult.NotEnoughChips => "NECESITAS UNA FICHA DE LA CASA",
                _ => "EL BAR NO ATIENDE"
            }, result == ShopResult.Bought);
        }

        void ShowFeedback(string text, bool positive)
        {
            if (feedback == null) return;
            if (feedbackRoutine != null) StopCoroutine(feedbackRoutine);
            feedback.text = text;
            feedback.EnableInClassList("shop-feedback--ok", positive);
            feedback.EnableInClassList("shop-feedback--error", !positive);
            feedbackRoutine = StartCoroutine(HideFeedbackLater());
        }

        IEnumerator HideFeedbackLater()
        {
            yield return new WaitForSeconds(FeedbackSeconds);
            ClearFeedback();
        }

        void ClearFeedback()
        {
            if (feedback != null) feedback.text = string.Empty;
        }
    }
}
