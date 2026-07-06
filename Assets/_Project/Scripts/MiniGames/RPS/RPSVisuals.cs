using UnityEngine;
using UnityEngine.UIElements;

namespace EcosDelAzar.MiniGames.RPS
{
    public class RPSVisuals : MonoBehaviour
    {
        [SerializeField] RPSGame game;
        [SerializeField] UIDocument uiDocument;

        [Header("Choice Sprites")]
        [SerializeField] Sprite rockSprite;
        [SerializeField] Sprite paperSprite;
        [SerializeField] Sprite scissorsSprite;
        [SerializeField] Sprite hiddenSprite;

        VisualElement playerHandImg;
        VisualElement opponentHandImg;
        VisualElement choicePanel;
        Button btnRock;
        Button btnPaper;
        Button btnScissors;

        void OnEnable()
        {
            if (uiDocument?.rootVisualElement != null)
            {
                var root = uiDocument.rootVisualElement;
                playerHandImg = root.Q<VisualElement>("player-hand-img");
                opponentHandImg = root.Q<VisualElement>("opponent-hand-img");
                choicePanel = root.Q<VisualElement>("rps-choice-panel");
                btnRock = root.Q<Button>("btn-rock");
                btnPaper = root.Q<Button>("btn-paper");
                btnScissors = root.Q<Button>("btn-scissors");
            }

            btnRock?.RegisterCallback<ClickEvent>(OnRock);
            btnPaper?.RegisterCallback<ClickEvent>(OnPaper);
            btnScissors?.RegisterCallback<ClickEvent>(OnScissors);

            if (game != null)
            {
                game.OnAwaitingPlayerChoice += HandleAwaitingChoice;
                game.OnPlayerChoiceLocked += HandlePlayerChoiceLocked;
                game.OnChoicesRevealed += HandleChoicesRevealed;
                game.OnReadyForNextRound += ResetHands;
            }

            ResetHands();
            SetChoicePanelVisible(false);
        }

        void OnDisable()
        {
            btnRock?.UnregisterCallback<ClickEvent>(OnRock);
            btnPaper?.UnregisterCallback<ClickEvent>(OnPaper);
            btnScissors?.UnregisterCallback<ClickEvent>(OnScissors);

            if (game != null)
            {
                game.OnAwaitingPlayerChoice -= HandleAwaitingChoice;
                game.OnPlayerChoiceLocked -= HandlePlayerChoiceLocked;
                game.OnChoicesRevealed -= HandleChoicesRevealed;
                game.OnReadyForNextRound -= ResetHands;
            }
        }

        void OnRock(ClickEvent _) => game?.SubmitPlayerChoice(RPSChoice.Rock);
        void OnPaper(ClickEvent _) => game?.SubmitPlayerChoice(RPSChoice.Paper);
        void OnScissors(ClickEvent _) => game?.SubmitPlayerChoice(RPSChoice.Scissors);

        void HandleAwaitingChoice()
        {
            ResetHands();
            SetChoicePanelVisible(true);
        }

        void HandlePlayerChoiceLocked(RPSChoice choice)
        {
            SetChoicePanelVisible(false);
            SetHand(playerHandImg, SpriteFor(choice));
            SetHand(opponentHandImg, hiddenSprite);
        }

        void HandleChoicesRevealed(RPSChoice player, RPSChoice opponent)
        {
            SetHand(playerHandImg, SpriteFor(player));
            SetHand(opponentHandImg, SpriteFor(opponent));
        }

        void ResetHands()
        {
            SetHand(playerHandImg, hiddenSprite);
            SetHand(opponentHandImg, hiddenSprite);
        }

        void SetChoicePanelVisible(bool visible)
        {
            if (choicePanel != null)
                choicePanel.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }

        void SetHand(VisualElement el, Sprite sprite)
        {
            if (el == null || sprite == null) return;
            el.style.backgroundImage = new StyleBackground(sprite);
        }

        Sprite SpriteFor(RPSChoice c) => c switch
        {
            RPSChoice.Rock => rockSprite,
            RPSChoice.Paper => paperSprite,
            RPSChoice.Scissors => scissorsSprite,
            _ => hiddenSprite
        };
    }
}
