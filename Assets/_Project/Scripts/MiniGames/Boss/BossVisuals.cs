using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using EcosDelAzar.MiniGames.Betting;

namespace EcosDelAzar.MiniGames.Boss
{
    /// <summary>
    /// Boss-table overlay: the boss's suit badge, a short toast, and the
    /// sudden-death panels (proposal, cards, result). Renders only; rules live
    /// in BossOxygenModifier and SuddenDeathRound.
    /// </summary>
    public class BossVisuals : MonoBehaviour
    {
        [SerializeField] UIDocument uiDocument;
        [SerializeField] BossOxygenModifier oxygenModifier;
        [SerializeField] SuddenDeathRound suddenDeath;
        [SerializeField] BettingSystem bettingSystem;

        Label bossSuitIcon;
        Label bossSuitName;
        Label bossSuitRule;

        VisualElement toast;
        Label toastTitle;
        Label toastSub;
        Coroutine toastCoroutine;

        VisualElement proposalPanel;
        Label sdPotLabel;
        Button btnAcceptSd;
        Button btnDeclineSd;

        VisualElement cardsPanel;
        Label sdTurnLabel;
        readonly List<Button> cardButtons = new();

        VisualElement resultPanel;
        Label sdResultLabel;
        Label sdResultSub;

        void OnEnable()
        {
            var root = uiDocument?.rootVisualElement;
            if (root == null) return;

            bossSuitIcon = root.Q<Label>("boss-suit-icon");
            bossSuitName = root.Q<Label>("boss-suit-name");
            bossSuitRule = root.Q<Label>("boss-suit-rule");

            toast = root.Q<VisualElement>("boss-toast");
            toastTitle = root.Q<Label>("toast-title");
            toastSub = root.Q<Label>("toast-sub");

            proposalPanel = root.Q<VisualElement>("sudden-death-proposal");
            sdPotLabel = root.Q<Label>("sd-pot-label");
            btnAcceptSd = root.Q<Button>("btn-accept-sd");
            btnDeclineSd = root.Q<Button>("btn-decline-sd");

            cardsPanel = root.Q<VisualElement>("sudden-death-cards");
            sdTurnLabel = root.Q<Label>("sd-turn-label");
            cardButtons.Clear();
            for (int i = 0; i < SuddenDeathRound.TotalCards; i++)
            {
                int idx = i;
                var btn = root.Q<Button>($"card-{i}");
                if (btn == null) continue;
                btn.clicked += () => suddenDeath?.PlayerPickCard(idx);
                cardButtons.Add(btn);
            }

            resultPanel = root.Q<VisualElement>("sudden-death-result");
            sdResultLabel = root.Q<Label>("sd-result-label");
            sdResultSub = root.Q<Label>("sd-result-sub");

            if (btnAcceptSd != null) btnAcceptSd.clicked += OnAcceptSuddenDeath;
            if (btnDeclineSd != null) btnDeclineSd.clicked += OnDeclineSuddenDeath;

            if (suddenDeath != null)
            {
                suddenDeath.OnSuddenDeathProposed += ShowProposalPanel;
                suddenDeath.OnCardDrawn += OnCardDrawn;
                suddenDeath.OnReshuffled += OnReshuffled;
                suddenDeath.OnDeclined += OnDeclined;
                suddenDeath.OnSuddenDeathComplete += ShowResultPanel;
            }

            if (oxygenModifier != null) ShowBossSuit(oxygenModifier.AssignedSuit);
            HideAllOverlays();
        }

        void OnDisable()
        {
            if (btnAcceptSd != null) btnAcceptSd.clicked -= OnAcceptSuddenDeath;
            if (btnDeclineSd != null) btnDeclineSd.clicked -= OnDeclineSuddenDeath;

            if (suddenDeath != null)
            {
                suddenDeath.OnSuddenDeathProposed -= ShowProposalPanel;
                suddenDeath.OnCardDrawn -= OnCardDrawn;
                suddenDeath.OnReshuffled -= OnReshuffled;
                suddenDeath.OnDeclined -= OnDeclined;
                suddenDeath.OnSuddenDeathComplete -= ShowResultPanel;
            }
        }

        void ShowBossSuit(Suit suit)
        {
            (string icon, string name, Color color) = suit switch
            {
                Suit.Hearts => ("♥", "CORAZONES", new Color(0.86f, 0.24f, 0.2f)),
                Suit.Diamonds => ("♦", "DIAMANTES", new Color(0.86f, 0.24f, 0.2f)),
                Suit.Spades => ("♠", "PICAS", new Color(0.91f, 0.82f, 0.5f)),
                Suit.Clubs => ("♣", "TRÉBOLES", new Color(0.91f, 0.82f, 0.5f)),
                _ => ("?", "?", Color.gray)
            };

            if (bossSuitIcon != null)
            {
                bossSuitIcon.text = icon;
                bossSuitIcon.style.color = color;
            }

            if (bossSuitName != null)
            {
                bossSuitName.text = name;
                bossSuitName.style.color = color;
            }

            // The rule is not obvious from a suit symbol: spell out what it does.
            if (bossSuitRule != null)
                bossSuitRule.text = $"Con J, Q, K o As de {name.ToLowerInvariant()} en tu mano: ganas aire si vences la ronda, lo pierdes si caes.";
        }

        void ShowToast(string title, string sub = "")
        {
            if (toast == null) return;
            if (toastTitle != null) toastTitle.text = title;
            if (toastSub != null) toastSub.text = sub;
            if (toastCoroutine != null) StopCoroutine(toastCoroutine);
            toastCoroutine = StartCoroutine(ToastRoutine());
        }

        IEnumerator ToastRoutine()
        {
            toast.RemoveFromClassList("boss-toast--hidden");
            toast.style.display = DisplayStyle.Flex;
            yield return new WaitForSeconds(3f);
            toast.AddToClassList("boss-toast--hidden");
            yield return new WaitForSeconds(0.5f);
            toast.style.display = DisplayStyle.None;
            toastCoroutine = null;
        }

        void ShowProposalPanel()
        {
            HideAllOverlays();
            if (proposalPanel == null) return;

            if (sdPotLabel != null && bettingSystem != null)
                sdPotLabel.text = $"POT TOTAL: {bettingSystem.PlayerCoins + bettingSystem.OpponentCoins} fichas  ·  Rechazar cuesta {suddenDeath.DeclineFeeFor(bettingSystem.PlayerCoins)}";

            proposalPanel.style.display = DisplayStyle.Flex;
        }

        void OnAcceptSuddenDeath()
        {
            HideAllOverlays();
            ShowCardsPanel();
            suddenDeath?.Accept();
        }

        void OnDeclineSuddenDeath()
        {
            HideAllOverlays();
            suddenDeath?.Decline();
        }

        void OnDeclined(int fee)
        {
            if (fee > 0) ShowToast("DUELO RECHAZADO", $"El director se queda {fee} monedas.");
        }

        void OnReshuffled(int round)
        {
            ResetCardButtons();
            if (sdTurnLabel != null) sdTurnLabel.text = $"RONDA {round} — Sin comodín. Se reparte de nuevo...";
        }

        void ShowCardsPanel()
        {
            if (cardsPanel == null) return;
            cardsPanel.style.display = DisplayStyle.Flex;
            ResetCardButtons();
            SetTurnLabel(isPlayerTurn: true);
        }

        void ResetCardButtons()
        {
            foreach (var btn in cardButtons)
            {
                btn.text = "?";
                btn.SetEnabled(true);
                btn.style.borderTopColor = StyleKeyword.Null;
                btn.RemoveFromClassList("sd-card--revealed");
                btn.RemoveFromClassList("sd-card--joker");
                btn.RemoveFromClassList("sd-card--winner");
            }
        }

        void OnCardDrawn(int cardIndex, Card card, bool isPlayerTurn)
        {
            if (cardIndex < 0 || cardIndex >= cardButtons.Count) return;

            var btn = cardButtons[cardIndex];
            btn.SetEnabled(false);
            btn.AddToClassList("sd-card--revealed");

            bool isJoker = card.Rank == Rank.Joker;
            btn.text = isJoker ? "JOKER" : CardToSymbol(card);

            if (isJoker)
            {
                btn.AddToClassList("sd-card--joker");
                if (isPlayerTurn) btn.style.borderTopColor = new StyleColor(new Color(0.85f, 0.23f, 0.2f));
                else btn.AddToClassList("sd-card--winner");
                return;
            }

            SetTurnLabel(isPlayerTurn: !isPlayerTurn);
        }

        void SetTurnLabel(bool isPlayerTurn)
        {
            if (sdTurnLabel == null) return;
            sdTurnLabel.text = isPlayerTurn ? "TU TURNO — Elige una carta" : "TURNO DEL BOSS...";
        }

        void ShowResultPanel(bool playerWon)
        {
            if (resultPanel == null) return;

            if (sdResultLabel != null)
            {
                sdResultLabel.text = playerWon ? "¡VICTORIA!" : "DERROTA";
                sdResultLabel.RemoveFromClassList("sd-result-label--win");
                sdResultLabel.RemoveFromClassList("sd-result-label--lose");
                sdResultLabel.AddToClassList(playerWon ? "sd-result-label--win" : "sd-result-label--lose");
            }

            if (sdResultSub != null)
                sdResultSub.text = playerWon ? "Te llevas todo el pot" : "El boss se lleva todo el pot";

            if (cardsPanel != null) cardsPanel.style.display = DisplayStyle.None;
            resultPanel.style.display = DisplayStyle.Flex;
        }

        void HideAllOverlays()
        {
            if (proposalPanel != null) proposalPanel.style.display = DisplayStyle.None;
            if (cardsPanel != null) cardsPanel.style.display = DisplayStyle.None;
            if (resultPanel != null) resultPanel.style.display = DisplayStyle.None;
        }

        static string CardToSymbol(Card card)
        {
            string rank = card.Rank switch
            {
                Rank.Jack => "J",
                Rank.Queen => "Q",
                Rank.King => "K",
                Rank.Ace => "A",
                _ => ((int)card.Rank).ToString()
            };

            string suit = card.Suit switch
            {
                Suit.Hearts => "♥",
                Suit.Diamonds => "♦",
                Suit.Spades => "♠",
                Suit.Clubs => "♣",
                _ => ""
            };

            return rank + suit;
        }
    }
}
