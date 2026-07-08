using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace EcosDelAzar.MiniGames.Boss
{
    public class BossHighCardVisuals : MonoBehaviour
    {
        [SerializeField] UIDocument uiDocument;
        [SerializeField] BossOxygenModifier oxygenModifier;
        [SerializeField] SuddenDeathRound suddenDeath;

        VisualElement bossInfoBar;
        Label bosssSuitIcon;
        Button btnForceWin;
        Button btnToggleBar;
        Button btnRestoreBar;

        VisualElement proposalPanel;
        Label sdPotLabel;
        Button btnAcceptSd;
        Button btnDeclineSd;

        VisualElement cardsPanel;
        Label sdTurnLabel;
        List<Button> cardButtons = new();

        VisualElement resultPanel;
        Label sdResultLabel;
        Label sdResultSub;

        VisualElement forceWinToast;
        Coroutine toastCoroutine;

        void OnEnable()
        {
            var root = uiDocument?.rootVisualElement;
            if (root == null) return;

            bossInfoBar   = root.Q<VisualElement>("boss-info-bar");
            bosssSuitIcon = root.Q<Label>("boss-suit-icon");
            btnForceWin   = root.Q<Button>("btn-force-win");
            btnToggleBar  = root.Q<Button>("btn-toggle-bar");
            btnRestoreBar = root.Q<Button>("btn-restore-bar");

            if (btnToggleBar  != null) btnToggleBar.clicked  += OnToggleBar;
            if (btnRestoreBar != null) btnRestoreBar.clicked += OnRestoreBar;

            if (PlayerPrefs.GetInt(PrefBarVisible, 1) == 0)
                SetBarVisible(false, animate: false);

            MakeDraggable(bossInfoBar);

            proposalPanel = root.Q<VisualElement>("sudden-death-proposal");
            sdPotLabel    = root.Q<Label>("sd-pot-label");
            btnAcceptSd   = root.Q<Button>("btn-accept-sd");
            btnDeclineSd  = root.Q<Button>("btn-decline-sd");

            cardsPanel  = root.Q<VisualElement>("sudden-death-cards");
            sdTurnLabel = root.Q<Label>("sd-turn-label");
            cardButtons.Clear();
            for (int i = 0; i < 6; i++)
            {
                int idx = i;
                var btn = root.Q<Button>($"card-{i}");
                if (btn != null)
                {
                    btn.clicked += () => suddenDeath?.PlayerPickCard(idx);
                    cardButtons.Add(btn);
                }
            }

            forceWinToast = root.Q<VisualElement>("force-win-toast");

            resultPanel   = root.Q<VisualElement>("sudden-death-result");
            sdResultLabel = root.Q<Label>("sd-result-label");
            sdResultSub   = root.Q<Label>("sd-result-sub");

            if (btnForceWin  != null) btnForceWin.clicked  += OnForceWinClicked;
            if (btnAcceptSd  != null) btnAcceptSd.clicked  += OnAcceptSuddenDeath;
            if (btnDeclineSd != null) btnDeclineSd.clicked += OnDeclineSuddenDeath;

            if (oxygenModifier != null)
            {
                ShowBossSuit(oxygenModifier.AssignedSuit);
                oxygenModifier.OnForceWinAvailabilityChanged += RefreshForceWinButton;
            }

            if (suddenDeath != null)
            {
                suddenDeath.OnSuddenDeathProposed  += ShowProposalPanel;
                suddenDeath.OnCardDrawn            += OnCardDrawn;
                suddenDeath.OnSuddenDeathComplete  += ShowResultPanel;
            }

            HideAllOverlays();
            RefreshForceWinButton();
        }

        void OnDisable()
        {
            if (btnForceWin  != null) btnForceWin.clicked  -= OnForceWinClicked;
            if (btnAcceptSd  != null) btnAcceptSd.clicked  -= OnAcceptSuddenDeath;
            if (btnDeclineSd != null) btnDeclineSd.clicked -= OnDeclineSuddenDeath;
            if (btnToggleBar  != null) btnToggleBar.clicked  -= OnToggleBar;
            if (btnRestoreBar != null) btnRestoreBar.clicked -= OnRestoreBar;

            if (oxygenModifier != null)
                oxygenModifier.OnForceWinAvailabilityChanged -= RefreshForceWinButton;

            if (suddenDeath != null)
            {
                suddenDeath.OnSuddenDeathProposed  -= ShowProposalPanel;
                suddenDeath.OnCardDrawn            -= OnCardDrawn;
                suddenDeath.OnSuddenDeathComplete  -= ShowResultPanel;
            }
        }

        void ShowBossSuit(Suit suit)
        {
            if (bosssSuitIcon == null) return;

            (string icon, string colorHex) = suit switch
            {
                Suit.Hearts   => ("♥", "#DC3C32"),
                Suit.Diamonds => ("♦", "#DC3C32"),
                Suit.Spades   => ("♠", "#E8D080"),
                Suit.Clubs    => ("♣", "#E8D080"),
                _             => ("?",  "#888888")
            };

            bosssSuitIcon.text = icon;
            bosssSuitIcon.style.color = new StyleColor(HexToColor(colorHex));
        }

        void OnForceWinClicked()
        {
            if (oxygenModifier == null) return;
            bool activated = oxygenModifier.TryActivateForceWin();
            if (activated) RefreshForceWinButton();
        }

        void RefreshForceWinButton()
        {
            if (btnForceWin == null) return;

            bool available = oxygenModifier != null && oxygenModifier.IsForceWinAvailable;
            btnForceWin.SetEnabled(available);
            btnForceWin.EnableInClassList("force-win-btn--disabled", !available);

            if (available)
                ShowForceWinToast();
        }

        void ShowForceWinToast()
        {
            if (forceWinToast == null) return;
            if (toastCoroutine != null) StopCoroutine(toastCoroutine);
            toastCoroutine = StartCoroutine(ToastRoutine());
        }

        IEnumerator ToastRoutine()
        {
            forceWinToast.RemoveFromClassList("force-win-toast--hidden");
            forceWinToast.style.display = DisplayStyle.Flex;

            yield return new WaitForSeconds(3f);

            forceWinToast.AddToClassList("force-win-toast--hidden");
            yield return new WaitForSeconds(0.5f);

            forceWinToast.style.display = DisplayStyle.None;
            toastCoroutine = null;
        }

        void ShowProposalPanel()
        {
            HideAllOverlays();
            if (proposalPanel == null) return;

            if (sdPotLabel != null && suddenDeath != null)
            {
                var betting = GetComponentInParent<EcosDelAzar.MiniGames.Betting.BettingSystem>(true)
                           ?? GetComponent<EcosDelAzar.MiniGames.Betting.BettingSystem>();
                if (betting != null)
                    sdPotLabel.text = $"POT TOTAL: {betting.PlayerCoins + betting.OpponentCoins} fichas";
            }

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
                btn.RemoveFromClassList("sd-card--revealed");
                btn.RemoveFromClassList("sd-card--joker");
                btn.RemoveFromClassList("sd-card--winner");
            }
        }

        void OnCardDrawn(int cardIndex, Card card, bool isPlayerTurn)
        {
            Debug.Log($"[BossHighCardVisuals] OnCardDrawn — índice={cardIndex}, carta={card}, botones={cardButtons.Count}");
            if (cardIndex < 0 || cardIndex >= cardButtons.Count) return;

            var btn = cardButtons[cardIndex];
            btn.SetEnabled(false);
            btn.AddToClassList("sd-card--revealed");

            bool isJoker = card.Rank == Rank.Joker;
            btn.text = isJoker ? "JOKER" : CardToSymbol(card);

            if (isJoker)
            {
                btn.AddToClassList("sd-card--joker");

                if (isPlayerTurn) MarkLoser(cardIndex);
                else              MarkWinner(cardIndex);
            }

            if (!isJoker)
                SetTurnLabel(isPlayerTurn: !isPlayerTurn);
        }

        void MarkLoser(int idx)
        {
            if (idx >= 0 && idx < cardButtons.Count)
                cardButtons[idx].style.borderTopColor = new StyleColor(new Color(0.85f, 0.23f, 0.2f));
        }

        void MarkWinner(int idx)
        {
            if (idx >= 0 && idx < cardButtons.Count)
                cardButtons[idx].AddToClassList("sd-card--winner");
        }

        void SetTurnLabel(bool isPlayerTurn)
        {
            if (sdTurnLabel == null) return;
            sdTurnLabel.text = isPlayerTurn
                ? "TU TURNO — Elige una carta"
                : "TURNO DEL BOSS...";
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
                sdResultSub.text = playerWon
                    ? "Te llevas todo el pot"
                    : "El boss se lleva todo el pot";

            if (cardsPanel != null) cardsPanel.style.display = DisplayStyle.None;
            resultPanel.style.display = DisplayStyle.Flex;
        }

        void HideAllOverlays()
        {
            if (proposalPanel != null) proposalPanel.style.display = DisplayStyle.None;
            if (cardsPanel   != null) cardsPanel.style.display    = DisplayStyle.None;
            if (resultPanel  != null) resultPanel.style.display   = DisplayStyle.None;
        }

        string CardToSymbol(Card card)
        {
            string rankStr = card.Rank switch
            {
                Rank.Jack  => "J",
                Rank.Queen => "Q",
                Rank.King  => "K",
                Rank.Ace   => "A",
                Rank.Joker => "★",
                _          => ((int)card.Rank).ToString()
            };

            string suitStr = card.Suit switch
            {
                Suit.Hearts   => "♥",
                Suit.Diamonds => "♦",
                Suit.Spades   => "♠",
                Suit.Clubs    => "♣",
                _             => ""
            };

            return $"{rankStr}{suitStr}";
        }

        static Color HexToColor(string hex)
        {
            ColorUtility.TryParseHtmlString(hex, out Color c);
            return c;
        }

        const string PrefBarVisible = "boss_bar_visible";

        void OnToggleBar()  => SetBarVisible(false);
        void OnRestoreBar() => SetBarVisible(true);

        void SetBarVisible(bool visible, bool animate = true)
        {
            if (bossInfoBar  != null) bossInfoBar.style.display  = visible ? DisplayStyle.Flex : DisplayStyle.None;
            if (btnRestoreBar != null) btnRestoreBar.style.display = visible ? DisplayStyle.None : DisplayStyle.Flex;

            if (btnRestoreBar != null && oxygenModifier != null)
            {
                string icon = oxygenModifier.AssignedSuit switch
                {
                    Suit.Hearts   => "♥",
                    Suit.Diamonds => "♦",
                    Suit.Spades   => "♠",
                    Suit.Clubs    => "♣",
                    _             => "B"
                };
                btnRestoreBar.text = icon;
            }

            PlayerPrefs.SetInt(PrefBarVisible, visible ? 1 : 0);
            PlayerPrefs.Save();
        }

        const string PrefBarX = "boss_bar_x";
        const string PrefBarY = "boss_bar_y";

        void MakeDraggable(VisualElement el)
        {
            if (el == null) return;

            el.RegisterCallback<GeometryChangedEvent>(OnBarReady);

            bool dragging = false;
            Vector2 pointerStart = Vector2.zero;
            Vector2 posStart     = Vector2.zero;

            el.RegisterCallback<PointerDownEvent>(evt =>
            {
                el.style.translate = new StyleTranslate(new Translate(0, 0));
                el.style.left   = el.layout.x;
                el.style.top    = el.layout.y;
                el.style.bottom = StyleKeyword.Auto;

                dragging     = true;
                pointerStart = evt.position;
                posStart     = new Vector2(el.layout.x, el.layout.y);
                el.CapturePointer(evt.pointerId);
                evt.StopPropagation();
            });

            el.RegisterCallback<PointerMoveEvent>(evt =>
            {
                if (!dragging) return;
                Vector2 delta = (Vector2)evt.position - pointerStart;
                el.style.left = posStart.x + delta.x;
                el.style.top  = posStart.y + delta.y;
            });

            el.RegisterCallback<PointerUpEvent>(evt =>
            {
                if (!dragging) return;
                dragging = false;
                el.ReleasePointer(evt.pointerId);

                PlayerPrefs.SetFloat(PrefBarX, el.layout.x);
                PlayerPrefs.SetFloat(PrefBarY, el.layout.y);
                PlayerPrefs.Save();
            });
        }

        void OnBarReady(GeometryChangedEvent evt)
        {
            if (bossInfoBar == null) return;

            if (!PlayerPrefs.HasKey(PrefBarX)) return;

            bossInfoBar.UnregisterCallback<GeometryChangedEvent>(OnBarReady);

            float x = PlayerPrefs.GetFloat(PrefBarX);
            float y = PlayerPrefs.GetFloat(PrefBarY);

            bossInfoBar.style.translate = new StyleTranslate(new Translate(0, 0));
            bossInfoBar.style.left   = x;
            bossInfoBar.style.top    = y;
            bossInfoBar.style.bottom = StyleKeyword.Auto;
        }
    }
}
