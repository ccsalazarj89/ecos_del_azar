using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace EcosDelAzar.MiniGames.Blackjack
{
    public class BlackjackVisuals : MonoBehaviour
    {
        [SerializeField] BlackjackGame game;
        [SerializeField] UIDocument uiDocument;
        [SerializeField] CardSpriteMapper spriteMapper;
        [SerializeField] Sprite cardBackSprite;

        VisualElement playerHandRow;
        VisualElement opponentHandRow;
        Label playerScoreLabel;
        Label opponentScoreLabel;
        VisualElement actionPanel;
        Button btnHit;
        Button btnStand;

        readonly List<VisualElement> opponentCardEls = new();
        bool opponentHoleHidden;

        void OnEnable()
        {
            if (uiDocument?.rootVisualElement != null)
            {
                var root = uiDocument.rootVisualElement;
                playerHandRow = root.Q<VisualElement>("player-hand-row");
                opponentHandRow = root.Q<VisualElement>("opponent-hand-row");
                playerScoreLabel = root.Q<Label>("player-score");
                opponentScoreLabel = root.Q<Label>("opponent-score");
                actionPanel = root.Q<VisualElement>("bj-action-panel");
                btnHit = root.Q<Button>("btn-hit");
                btnStand = root.Q<Button>("btn-stand");
            }

            btnHit?.RegisterCallback<ClickEvent>(OnHit);
            btnStand?.RegisterCallback<ClickEvent>(OnStand);

            if (game != null)
            {
                game.OnPlayerCardDealt += HandlePlayerCard;
                game.OnOpponentCardDealt += HandleOpponentCard;
                game.OnAwaitingPlayerAction += HandleAwaitingAction;
                game.OnPlayerStood += HandlePlayerStood;
                game.OnOpponentHoleCardRevealed += HandleHoleRevealed;
                game.OnRoundStarted += ResetTable;
                game.OnReadyForNextRound += ResetTable;
            }

            ResetTable();
            SetActionPanelVisible(false);
        }

        void OnDisable()
        {
            btnHit?.UnregisterCallback<ClickEvent>(OnHit);
            btnStand?.UnregisterCallback<ClickEvent>(OnStand);

            if (game != null)
            {
                game.OnPlayerCardDealt -= HandlePlayerCard;
                game.OnOpponentCardDealt -= HandleOpponentCard;
                game.OnAwaitingPlayerAction -= HandleAwaitingAction;
                game.OnPlayerStood -= HandlePlayerStood;
                game.OnOpponentHoleCardRevealed -= HandleHoleRevealed;
                game.OnRoundStarted -= ResetTable;
                game.OnReadyForNextRound -= ResetTable;
            }
        }

        void OnHit(ClickEvent _) => game?.Hit();
        void OnStand(ClickEvent _) => game?.Stand();

        void HandlePlayerCard(Card card)
        {
            AppendCard(playerHandRow, card, faceDown: false);
            RefreshPlayerScore();
        }

        void HandleOpponentCard(Card card)
        {
            bool faceDown = opponentCardEls.Count == 1;
            var el = AppendCard(opponentHandRow, card, faceDown);
            opponentCardEls.Add(el);
            if (faceDown) opponentHoleHidden = true;
            RefreshOpponentScore();
        }

        void HandleAwaitingAction()
        {
            SetActionPanelVisible(true);
        }

        void HandlePlayerStood()
        {
            SetActionPanelVisible(false);
        }

        void HandleHoleRevealed()
        {
            SetActionPanelVisible(false);

            if (opponentHoleHidden && opponentCardEls.Count >= 2 && game != null && game.OpponentHand.Count >= 2)
            {
                var holeCard = game.OpponentHand.Cards[1];
                SetCardSprite(opponentCardEls[1], spriteMapper.GetSprite(holeCard));
                opponentHoleHidden = false;
            }

            RefreshOpponentScore();
        }

        void ResetTable()
        {
            playerHandRow?.Clear();
            opponentHandRow?.Clear();
            opponentCardEls.Clear();
            opponentHoleHidden = false;
            if (playerScoreLabel != null) playerScoreLabel.text = "0";
            if (opponentScoreLabel != null) opponentScoreLabel.text = "0";
            SetActionPanelVisible(false);
        }

        VisualElement AppendCard(VisualElement row, Card card, bool faceDown)
        {
            if (row == null) return null;
            var el = new VisualElement();
            el.AddToClassList("bj-card");
            SetCardSprite(el, faceDown ? cardBackSprite : spriteMapper.GetSprite(card));
            row.Add(el);
            return el;
        }

        void SetCardSprite(VisualElement el, Sprite sprite)
        {
            if (el == null || sprite == null) return;
            el.style.backgroundImage = new StyleBackground(sprite);
        }

        void RefreshPlayerScore()
        {
            if (playerScoreLabel != null && game != null)
                playerScoreLabel.text = game.PlayerHand.Score.ToString();
        }

        void RefreshOpponentScore()
        {
            if (opponentScoreLabel == null || game == null) return;

            if (opponentHoleHidden && game.OpponentHand.Count >= 2)
            {
                int visible = BlackjackHand.CardValue(game.OpponentHand.Cards[0]);
                opponentScoreLabel.text = $"{visible}+?";
            }
            else
            {
                opponentScoreLabel.text = game.OpponentHand.Score.ToString();
            }
        }

        void SetActionPanelVisible(bool visible)
        {
            if (actionPanel != null)
                actionPanel.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}
