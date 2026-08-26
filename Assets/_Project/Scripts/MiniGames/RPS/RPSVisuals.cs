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
        Label playerHandText;
        Label opponentHandText;
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
                playerHandText = root.Q<Label>("player-hand-text");
                opponentHandText = root.Q<Label>("opponent-hand-text");
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
            SetHand(playerHandImg, playerHandText, choice);
            SetHand(opponentHandImg, opponentHandText, RPSChoice.None);
        }

        void HandleChoicesRevealed(RPSChoice player, RPSChoice opponent)
        {
            SetHand(playerHandImg, playerHandText, player);
            SetHand(opponentHandImg, opponentHandText, opponent);
        }

        void ResetHands()
        {
            SetHand(playerHandImg, playerHandText, RPSChoice.None);
            SetHand(opponentHandImg, opponentHandText, RPSChoice.None);
        }

        void SetChoicePanelVisible(bool visible)
        {
            if (choicePanel != null)
                choicePanel.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }

        // Sprites are optional: without them the hand is shown as text.
        void SetHand(VisualElement img, Label text, RPSChoice choice)
        {
            Sprite sprite = SpriteFor(choice);

            if (img != null)
                img.style.backgroundImage = sprite != null ? new StyleBackground(sprite) : new StyleBackground();

            if (text != null)
            {
                text.text = TextFor(choice);
                text.style.display = sprite != null ? DisplayStyle.None : DisplayStyle.Flex;
            }
        }

        static string TextFor(RPSChoice c) => c switch
        {
            RPSChoice.Rock => "PIEDRA",
            RPSChoice.Paper => "PAPEL",
            RPSChoice.Scissors => "TIJERA",
            _ => "?"
        };

        Sprite SpriteFor(RPSChoice c) => c switch
        {
            RPSChoice.Rock => rockSprite,
            RPSChoice.Paper => paperSprite,
            RPSChoice.Scissors => scissorsSprite,
            _ => hiddenSprite
        };
    }
}
